using System;
using System.Threading.Tasks;
using TcMenu.CoreSdk.MenuItems;
using TcMenu.CoreSdk.RemoteCore;
using TcMenu.CoreSdk.Serialisation;
using MenuItem = TcMenu.CoreSdk.MenuItems.MenuItem;

namespace TcMenuCoreMaui.Controls
{
    public abstract class BaseTextEditorComponent<TVal> : BaseEditorComponent
    {
        public TVal CurrentVal { get; private set; }

        protected BaseTextEditorComponent(IRemoteController controller, ComponentSettings settings, MenuItem item)
            : base(controller, settings, item)
        {
        }

        protected async Task ValidateAndSend(string text)
        {
            if (_status == RenderStatus.EditInProgress) return;
            try
            {
                var toSend = MenuItemFormatter.FormatToWire(_item, text);
                var correlation = await _remoteController.SendAbsoluteChange(_item, toSend);
                EditStarted(correlation);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to send message to {_remoteController.Connector.ConnectionName}");
                MarkRecentlyUpdated(RenderStatus.CorrelationError);
            }
        }

        public override void OnItemUpdated(AnyMenuState newValue)
        {
            if (newValue is MenuState<TVal> strVal)
            {
                CurrentVal = strVal.Value;
                MarkRecentlyUpdated(RenderStatus.RecentlyUpdated);
            }
        }

        public override string GetControlText()
        {
            var str = "";
            if ((DrawingSettings.DrawMode & RedrawingMode.ShowName) != 0) str = _item.Name + " ";
            if ((DrawingSettings.DrawMode & RedrawingMode.ShowValue) != 0) str += MenuItemFormatter.FormatForDisplay(_item, CurrentVal);
            return str;
        }

    }

}
