using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.RemoteStates;
using TcMenuCoreMaui.FormUi;
using TcMenuCoreMaui.Services;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public class TcMenuGridComponent : IDisposable
    {
        private readonly Dictionary<int, IEditorComponent> _editorComponents = new();
        private readonly IRemoteController _controller;
        private readonly PrefsAppSettings _appSettings;
        private readonly IMenuEditorFactory _factory;
        private readonly IMauiNavigation _navMgr;
        private readonly Grid _grid;
        private volatile bool _started = false;
        private readonly LoadedMenuForm _loadedForm;
        private int _row = 0;

        public TcMenuGridComponent(IRemoteController controller, PrefsAppSettings appSettings, LoadedMenuForm loadedForm, Grid grid)
        {

            _appSettings = appSettings;
            _grid = grid;
            _controller = controller;
            _loadedForm = loadedForm;
        }

        public void Start(MenuItem menuItem)
        {
            _controller.MenuChangedEvent += Controller_MenuChangedEvent;
            _controller.AcknowledgementsReceived += Controller_AcknowledgementsReceived;
            _controller.Connector.ConnectionChanged += Connector_ConnectionChanged;

            // handle the case where it's already connected really quick!
            if (_controller.Connector.AuthStatus == AuthenticationStatus.CONNECTION_READY)
            {
                Connector_ConnectionChanged(AuthenticationStatus.CONNECTION_READY);
            }

            _started = true;
            TimerLoop();
        }

        public void Stop()
        {
            _controller.MenuChangedEvent -= Controller_MenuChangedEvent;
            _controller.AcknowledgementsReceived -= Controller_AcknowledgementsReceived;
            _controller.Connector.ConnectionChanged -= Connector_ConnectionChanged;
            _started = false;
        }

        private void Connector_ConnectionChanged(AuthenticationStatus status)
        {
            if (status == AuthenticationStatus.CONNECTION_READY)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _editorComponents.Clear();
                    _grid.Clear();
                    RenderMenuRecursive(MenuTree.ROOT, _appSettings, 0);
                });
            }
        }

        public void AddItemAtPosition(ComponentPositioning position, IEditorComponent component, bool liveUpdates)
        {
            if (liveUpdates)
            {
                _editorComponents[component.Id] = component;
            }

            var comp = component.ViewItem;
            _grid.Add(comp, position.Col, position.Row);
            Grid.SetRowSpan(comp, position.RowSpan);
            Grid.SetColumnSpan(comp, position.ColSpan);
        }

        public void AddTextAtPosition(ComponentSettings settings, string toAdd)
        {
            var label = new Label
            {
                Text = toAdd,
                TextColor = settings.Colors.ForegroundFor(RenderStatus.Normal, ColorComponentType.TEXT_FIELD).AsXamarin(),
                FontAttributes = FontAttributes.Bold
            };
            var position = settings.Positioning;
            _grid.Add(label, position.Col, position.Row);
            Grid.SetRowSpan(label, position.RowSpan);
            Grid.SetColumnSpan(label, position.ColSpan);
        }

        public IEditorComponent GetComponentEditorItem(IMenuEditorFactory editorFactory, MenuItem item, ComponentSettings componentSettings)
        {
            if (componentSettings.DrawMode == RedrawingMode.Hidden) return null;

            if (item is SubMenuItem sub) {
                return editorFactory.CreateButtonWithAction(sub, sub.Name, componentSettings,
                    subMenuItem=>_navMgr.PushMenuNavigation(subMenuItem as SubMenuItem, _loadedForm));
            }

            return componentSettings.ControlType switch
            {
                ControlType.HorizontalSlider => editorFactory.CreateHorizontalSlider(item, componentSettings),
                ControlType.UpDownControl => editorFactory.CreateUpDown(item, componentSettings),
                ControlType.TextControl => editorFactory.CreateText<string>(item, componentSettings),
                ControlType.ButtonControl => editorFactory.CreateBooleanButton(item, componentSettings),
                ControlType.VuMeter => throw new FeatureNotSupportedException("create Vu Meter"),
                ControlType.DateControl  => editorFactory.CreateDate(item, componentSettings),
                ControlType.TimeControl => editorFactory.CreateTime(item, componentSettings),
                ControlType.RgbControl => editorFactory.CreateRgbColor(item, componentSettings),
                ControlType.ListControl => editorFactory.CreateList(item, componentSettings, s => s),
                ControlType.IoTControl => throw new FeatureNotSupportedException("No IoT Monitor"),
                _ => null
            };
        }

        /// <summary>
        /// Use this to get a component position that needs a full row of its own
        /// </summary>
        /// <returns>A component position that takes a full row of the layout</returns>
        private void RenderMenuRecursive(SubMenuItem sub, PrefsAppSettings appSettings, int level)
        {
            var tree = _controller.ManagedMenu;

            if (_loadedForm.HasLayoutFor(sub))
            {
                throw new FeatureNotSupportedException();
            }
            else
            {
                if (level != 0)
                {
                    AddTextAtPosition(GetSettingsForStaticItem(DefaultSpaceForItem(sub)), sub.Name);
                }

                foreach(var item in tree.GetMenuItems(sub))
                {
                    if (!item.Visible) continue;
                    if (item is SubMenuItem si && appSettings.RecurseIntoSub)
                    {
                        RenderMenuRecursive(si, appSettings, level + 1);
                    }
                    else
                    {
                        if (item is RuntimeListMenuItem rli)
                        {
                            var pos = DefaultSpaceForItem(rli);
                            AddTextAtPosition(GetSettingsForStaticItem(pos), item.Name);
                        }

                        var settings = GetSettingsForMenuItem(item);
                        var editorControl = GetComponentEditorItem(_factory, item, settings);
                        if (editorControl != null)
                        {
                            MenuItemHelper.GetValueFor(item, tree, MenuItemHelper.GetDefaultFor(item));
                            AddItemAtPosition(DefaultSpaceForItem(item), editorControl, true);
                            editorControl.OnItemUpdated(tree.GetState(item));
                        }
                    }
                }
            }
        }

        private ComponentSettings GetSettingsForMenuItem(MenuItem item)
        {
            var pos = DefaultSpaceForItem(item);
            return new ComponentSettings(
                _loadedForm.ColorSchemeAtPosition(pos),
                FontInformation.Font100Percent,
                pos,
                DefaultJustificationForItem(item),
                DefaultRedrawModeForItem(item),
                ComponentSettings.DefaultComponentTypeFor(item)
            );
        }

        private ComponentSettings GetSettingsForStaticItem(ComponentPositioning positioning)
        {
            return new ComponentSettings(
                _loadedForm.ColorSchemeAtPosition(positioning),
                FontInformation.Font100Percent, positioning, PortableAlignment.Left,
                RedrawingMode.ShowName, ControlType.TextControl
            );
        }

        protected RedrawingMode DefaultRedrawModeForItem(MenuItem item)
        {
            return RedrawingMode.ShowNameAndValue;
        }

        protected PortableAlignment DefaultJustificationForItem(MenuItem item)
        {
            return PortableAlignment.Center;
        }

        protected ComponentPositioning DefaultSpaceForItem(MenuItem item)
        {
            var pos = new ComponentPositioning(_row, 0, 1, _loadedForm.GridSize);
            _row++;
            return pos;
        }

        private async void TimerLoop()
        {
            while (_started)
            {
                await Task.Delay(1000);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var component in _editorComponents)
                    {
                        component.Value.Tick();
                    }
                });
            }
        }

        private void Controller_AcknowledgementsReceived(CorrelationId correlation, AckStatus status)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var uiItem in _editorComponents.Values)
                {
                    uiItem.OnCorrelation(correlation, status);
                }
            });
        }

        private void Controller_MenuChangedEvent(MenuItem changed, bool valueOnly)
        {
            if (_editorComponents?.ContainsKey(changed.Id) ?? false)
            {
                _editorComponents[changed.Id].OnItemUpdated(_controller.ManagedMenu.GetState(changed));
            }
        }

        public void Dispose()
        {
            _grid.Children.Clear();
            _editorComponents.Clear();
        }
    }
}