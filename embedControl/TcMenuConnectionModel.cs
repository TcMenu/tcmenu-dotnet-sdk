using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcMenu.CoreSdk.Commands;
using TcMenu.CoreSdk.RemoteCore;
using TcMenuCoreMaui.Controls;
using TcMenuCoreMaui.Services;

namespace embedControl
{
    public class ControlToMauiColorHelper
    {
        private readonly IConditionalColoring _conditionalColoring;
        private readonly ColorComponentType _colorType;

        public RenderStatus CurrentStatus { get; set; }
        public Color Fg => _conditionalColoring.ForegroundFor(CurrentStatus, _colorType).AsXamarin();
        public Color Bg => _conditionalColoring.BackgroundFor(CurrentStatus, _colorType).AsXamarin();

        public ControlToMauiColorHelper(IConditionalColoring condColors, ColorComponentType colorType)
        {
            _colorType = colorType;
            _conditionalColoring = condColors;
        }

    }

    public class TcMenuConnectionModel
    {
        private readonly IRemoteController _remoteController;
        private readonly PrefsAppSettings _appSettings;
        private MenuButtonType _button1Type = MenuButtonType.NONE;
        private MenuButtonType _button2Type = MenuButtonType.NONE;

        public string ConnectionName => _remoteController?.Connector?.ConnectionName + " " + _remoteController?.Connector?.AuthStatus;
        public ControlToMauiColorHelper DialogColor { get; }
        public ControlToMauiColorHelper ButtonColor { get; }
        public bool DialogOnDisplay { get; set; } = false;
        public string DialogHeader{ get; set; } = "Header";
        public string DialogBuffer{ get; set; } = "Buffer";
        public string Button1Text { get; set; } = "OK";
        public string Button2Text { get; set; } = "Cancel";

        public TcMenuConnectionModel(IRemoteController controller, PrefsAppSettings settings)
        {
            _remoteController = controller;
            _appSettings = settings;
            var settingsConditional = new PrefsConditionalColoring(settings);
            DialogColor = new ControlToMauiColorHelper(settingsConditional, ColorComponentType.DIALOG);
            ButtonColor = new ControlToMauiColorHelper(settingsConditional, ColorComponentType.BUTTON);
        }

        public void SendDialogEvent(int btnNum)
        {
            if (_remoteController != null)
            {
                _remoteController.SendDialogAction(btnNum == 0 ? _button1Type : _button2Type);
            }
        }
    }
}
