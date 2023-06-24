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
    public delegate void SubMenuNavigator(SubMenuItem item);

    public class TcMenuGridComponent : IDisposable
    {
        private readonly Dictionary<int, IEditorComponent> _editorComponents = new();
        private readonly IRemoteController _controller;
        private readonly PrefsAppSettings _appSettings;
        private readonly IMenuEditorFactory _factory;
        private readonly Grid _grid;
        private volatile bool _started = false;
        private readonly MenuFormLoader _formLoader;
        private int _row = 0;
        private readonly IConditionalColoring _globalColorScheme;
        private readonly SubMenuNavigator _subNavigator;
        private MenuItem _menuItem;

        public TcMenuGridComponent(IRemoteController controller, IMenuEditorFactory factory, PrefsAppSettings appSettings, 
                                   MenuFormLoader formLoader, Grid grid, SubMenuNavigator navigator)
        {
            _globalColorScheme = new PrefsConditionalColoring(appSettings);
            _factory = factory;
            _appSettings = appSettings;
            _grid = grid;
            _controller = controller;
            _subNavigator = navigator;
            _formLoader = formLoader;
        }

        public void Start(MenuItem menuItem)
        {
            _menuItem = menuItem;
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
                    CompletelyResetGrid(_menuItem);
                });
            }
        }

        public void CompletelyResetGrid(MenuItem menuItem)
        {
            // completely clear down all components.
            _editorComponents.Clear();
            _grid.Clear();
            _grid.ColumnDefinitions.Clear();
            _grid.RowDefinitions.Clear();
            _editorComponents.Clear();

            // if a menu item is provided navigate to it, if not, leave the grid cleared down and on Root.
            if (menuItem != null)
            {
                _menuItem = menuItem;
                RenderMenuRecursive(_menuItem as SubMenuItem, _appSettings, 0);
            }
            else
            {
                _menuItem = MenuTree.ROOT;
            }
        }

        public void AddItemAtPosition(ComponentPositioning position, IEditorComponent component, bool liveUpdates)
        {
            if (liveUpdates)
            {
                _editorComponents[component.Id] = component;
            }

            var comp = component.ViewItem;
            EnsureEnoughRowColPositions(position);
            Grid.SetRowSpan(comp, position.RowSpan);
            Grid.SetColumnSpan(comp, position.ColSpan);
            _grid.Add(comp, position.Col, position.Row);
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
            EnsureEnoughRowColPositions(position);
            Grid.SetRowSpan(label, position.RowSpan);
            Grid.SetColumnSpan(label, position.ColSpan);
            _grid.Add(label, position.Col, position.Row);
        }

        private void EnsureEnoughRowColPositions(ComponentPositioning position)
        {
            while(_grid.ColumnDefinitions.Count < position.Col)
            {
                _grid.ColumnDefinitions.Add(new ColumnDefinition{ Width = GridLength.Star});
            }
            while (_grid.RowDefinitions.Count < position.Row)
            {
                _grid.RowDefinitions.Add(new RowDefinition{ Height= GridLength.Auto});
            }
        }

        public IEditorComponent GetComponentEditorItem(IMenuEditorFactory editorFactory, MenuItem item, ComponentSettings componentSettings)
        {
            if (componentSettings.DrawMode == RedrawingMode.Hidden) return null;

            if (item is SubMenuItem sub) {
                return editorFactory.CreateButtonWithAction(sub, sub.Name, componentSettings,
                    subMenuItem=> _subNavigator?.Invoke(subMenuItem as SubMenuItem));
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
        public void RenderMenuRecursive(SubMenuItem sub, PrefsAppSettings appSettings, int level)
        {
            var tree = _controller.ManagedMenu;

            if (_formLoader.HasLayoutFor(sub))
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
                            AddItemAtPosition(settings.Positioning, editorControl, true);
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
                _globalColorScheme,
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
                _globalColorScheme,
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
            var gridSize = _formLoader.GridSize;
            var pos = new ComponentPositioning(_row, 0, 1, gridSize);
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
            Stop();
            _grid.Children.Clear();
            _editorComponents.Clear();
        }
    }
}