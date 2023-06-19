using System;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.Serialisation;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public abstract class BaseUpDownIntEditorComponent<T> : BaseEditorComponent
    {
        protected T _currentVal;

        public abstract int CurrentInt { get; }
        
        protected BaseUpDownIntEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item)
            : base(controller, settings, item)
        {
            OnItemUpdated(controller.ManagedMenu.GetState(item) as MenuState<T>);
        }

        protected virtual async void BumpCount(int delta)
        {
            if (_status == RenderStatus.EditInProgress) return;
            try
            {
                var correlation = await _remoteController.SendDeltaChange(_item, delta);
                EditStarted(correlation);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to send message to {_remoteController.Connector.ConnectionName}");
            }
        }

        public override string GetControlText()
        {
            var str = "";
            if ((DrawingSettings.DrawMode & RedrawingMode.ShowName) != 0) str = _item.Name;
            if ((DrawingSettings.DrawMode & RedrawingMode.ShowValue) == 0) return str;

            str += " ";
            str += MenuItemFormatter.FormatForDisplay(_item, _currentVal);

            return str;
        }

        public override void OnItemUpdated(AnyMenuState newValue)
        {
            if (newValue is MenuState<T> strVal)
            {
                _currentVal = strVal.Value;
                MarkRecentlyUpdated(RenderStatus.RecentlyUpdated);
            }
        }
    }

}
