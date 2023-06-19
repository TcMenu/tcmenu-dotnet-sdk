using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.Protocol;
using TcMenu.CoreSdk.RemoteCore;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public abstract class BaseEditorComponent : IEditorComponent
    {
        public const int MAX_CORRELATION_WAIT = 5;

        protected static Serilog.ILogger logger = Serilog.Log.Logger.ForContext(typeof(IEditorComponent));
        protected readonly IRemoteController _remoteController;

        public ComponentSettings DrawingSettings { get; }

        public int Id => _item.Id;

        protected MenuItem _item;

        private readonly object _tickLock = new object();
        private CorrelationId _correlation;
        private DateTime _lastCorrelation = DateTime.Now;
        private DateTime _lastUpdate = DateTime.Now;
        protected volatile RenderStatus _status = RenderStatus.Normal;

        protected BaseEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item)
        {
            _remoteController = controller;
            _item = item;
            DrawingSettings = settings;
        }

        public abstract View ViewItem { get; }
        public abstract void OnItemUpdated(AnyMenuState newValue);

        public abstract void ChangeControlSettings(RenderStatus status, string text);

        public abstract string GetControlText();

        public async void UpdateEditor()
        {
            logger.Information($"Updating editor for {_item}, status is {_status}");
            string str = GetControlText();
            ChangeControlSettings(_status, str);
        }

        public void EditStarted(CorrelationId correlation)
        {
            _status = RenderStatus.EditInProgress;
            lock (_tickLock)
            {
                _lastCorrelation = DateTime.Now;
                _correlation = correlation;
            }
            UpdateEditor();
        }

        public void MarkRecentlyUpdated(RenderStatus status)
        {
            lock (_tickLock)
            {
                _status = status;
                _lastUpdate = DateTime.Now;
            }
            UpdateEditor();
        }

        public void Tick()
        {
            if (_status is RenderStatus.RecentlyUpdated or RenderStatus.CorrelationError)
            {
                lock (_tickLock)
                {
                    var span = DateTime.Now - _lastUpdate;
                    if (span.TotalSeconds > 1)
                    {
                        _status = RenderStatus.Normal;
                        UpdateEditor();
                    }
                }
            }

            var updateErr = false;
            var correlation = CorrelationId.EMPTY_CORRELATION;
            lock (_tickLock)
            {
                var span = DateTime.Now - _lastCorrelation;
                if (_correlation != null && span.TotalSeconds > MAX_CORRELATION_WAIT)
                {
                    correlation = _correlation;
                    _correlation = null;
                    updateErr = true;
                }
            }

            if (updateErr)
            {
                logger.Error($"No correlation update recieved for {correlation}");
                MarkRecentlyUpdated(RenderStatus.CorrelationError);
            }
        }

        public void OnCorrelation(CorrelationId correlationId, AckStatus status)
        {
            var ourUpdate = false;
            lock (_tickLock)
            {
                if (_correlation != null && _correlation.Equals(correlationId))
                {
                    _correlation = null;
                    ourUpdate = true;
                }
            }

            if (ourUpdate)
            {
                logger.Information($"Correlation update received for {correlationId}, status = {status}");
                if (status != AckStatus.SUCCESS)
                {
                    MarkRecentlyUpdated(RenderStatus.CorrelationError);
                }
                else
                {
                    MarkRecentlyUpdated(RenderStatus.Normal);
                }
            }
        }

        private readonly ISet<Type> _userEditableMenuItemTypes = new HashSet<Type>
        {
            typeof(EditableTextMenuItem),
            typeof(Rgb32MenuItem),
            typeof(ScrollChoiceMenuItem),
            typeof(LargeNumberMenuItem),
            typeof(AnalogMenuItem),
            typeof(EnumMenuItem),
            typeof(BooleanMenuItem)
        };

        public bool IsItemEditable(MenuItem item)
        {
            return _userEditableMenuItemTypes.Contains(item.GetType()) && !item.ReadOnly;
        }
    }
}
