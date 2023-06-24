using System;
using System.Xml.Linq;
using TcMenu.CoreSdk.Serialisation;
using TcMenuCoreMaui.Controls;
using TcMenuCoreMaui.Services;

namespace TcMenuCoreMaui.Controls
{
    public enum ColorComponentType { TEXT_FIELD, BUTTON, HIGHLIGHT, CUSTOM, DIALOG, ERROR, PENDING }

    public interface IConditionalColoring
    {
        public string ColorName { get; }

        PortableColor ForegroundFor(RenderStatus status, ColorComponentType compType);
        PortableColor BackgroundFor(RenderStatus status, ColorComponentType compType);
    }

    public class NullConditionalColoring : IConditionalColoring
    {
        public string ColorName => "Null";

        public PortableColor BackgroundFor(RenderStatus status, ColorComponentType ty)
        {
            return PortableColors.WHITE;
        }

        public PortableColor ForegroundFor(RenderStatus status, ColorComponentType ty)
        {
            return PortableColors.BLACK;
        }
    }

    public class PrefsConditionalColoring : IConditionalColoring
    {
        public static readonly string GlobalColorSetName = "Global";
        private readonly PrefsAppSettings _settings;

        public string ColorName => "Preferences";

        public PrefsConditionalColoring(PrefsAppSettings settings)
        {
            _settings = settings;
        }

        public PortableColor BackgroundFor(RenderStatus status, ColorComponentType compType)
        {
            return status switch
            {
                RenderStatus.RecentlyUpdated => _settings.UpdateColor.Bg,
                RenderStatus.EditInProgress => _settings.PendingColor.Bg,
                RenderStatus.CorrelationError => _settings.ErrorColor.Bg,
                _ => ColorCompForType(compType).Bg
            };
        }

        private ControlColor ColorCompForType(ColorComponentType compType)
        {
            return compType switch
            {
                ColorComponentType.TEXT_FIELD => _settings.TextColor,
                ColorComponentType.BUTTON => _settings.ButtonColor,
                ColorComponentType.HIGHLIGHT => _settings.HighlightColor,
                ColorComponentType.CUSTOM => _settings.HighlightColor,
                ColorComponentType.DIALOG => _settings.DialogColor,
                ColorComponentType.PENDING => _settings.PendingColor,
                _ => throw new ArgumentOutOfRangeException(nameof(compType), compType, null)
            };
        }

        public PortableColor ForegroundFor(RenderStatus status, ColorComponentType compType)
        {
            switch (status)
            {
                case RenderStatus.RecentlyUpdated:
                    return _settings.UpdateColor.Fg;
                case RenderStatus.EditInProgress:
                    return _settings.PendingColor.Fg;
                case RenderStatus.CorrelationError:
                    return _settings.ErrorColor.Fg;
                default:
                    return ColorCompForType(compType).Fg;
            }
        }
    }

    public class NamedXmlConditionalColoring : IConditionalColoring
    {
        public static readonly string GlobalColorSetName = "Global";
        private readonly PrefsAppSettings _settings;
        private readonly ControlColor _textColor;
        private readonly ControlColor _buttonColor;
        private readonly ControlColor _highlightedColor;
        private readonly ControlColor _errorColor;
        private readonly ControlColor _customColor;
        private readonly ControlColor _dialogColor;
        private readonly ControlColor _pendingColor;

        public string ColorName { get; }

        public NamedXmlConditionalColoring(PrefsAppSettings settings, XElement colorSetElement)
        {
            _settings = settings;
            ColorName = colorSetElement.Attribute("name")?.Value;
            _textColor = ProcessColor(colorSetElement, "text");
            _buttonColor = ProcessColor(colorSetElement, "button");
            _highlightedColor = ProcessColor(colorSetElement, "highlight");
            _errorColor = ProcessColor(colorSetElement, "error");
            _customColor = ProcessColor(colorSetElement, "custom");
            _dialogColor = ProcessColor(colorSetElement, "dialog");
            _pendingColor = ProcessColor(colorSetElement, "pending");
        }

        private ControlColor ProcessColor(XContainer colSet, string eleName)
        {
            var namedCol = colSet.Element(eleName);
            if (namedCol?.Attribute("isPresent")?.Value.Equals("true") ?? false)
            {
                var fgText = namedCol.Attribute("fg")?.Value ?? "#000";
                var bgText = namedCol.Attribute("bg")?.Value ?? "#FFF";

                return new ControlColor(new PortableColor(fgText), new PortableColor(bgText));
            }
            else
            {
                return null;
            }
        }

        public PortableColor BackgroundFor(RenderStatus status, ColorComponentType compType)
        {
            compType = status switch
            {
                RenderStatus.RecentlyUpdated => ColorComponentType.HIGHLIGHT,
                RenderStatus.EditInProgress => ColorComponentType.PENDING,
                RenderStatus.CorrelationError => ColorComponentType.ERROR,
                _ => compType
            };
            return ColorCompForType(compType).Bg;
        }

        private ControlColor ColorCompForType(ColorComponentType compType)
        {
            return compType switch
            {
                ColorComponentType.TEXT_FIELD => _textColor ?? _settings.TextColor,
                ColorComponentType.BUTTON => _buttonColor ?? _settings.ButtonColor,
                ColorComponentType.HIGHLIGHT => _highlightedColor ?? _settings.HighlightColor,
                ColorComponentType.CUSTOM => _customColor ?? _settings.HighlightColor,
                ColorComponentType.DIALOG => _dialogColor ?? _settings.DialogColor,
                ColorComponentType.PENDING => _pendingColor ?? _settings.PendingColor,
                ColorComponentType.ERROR => _errorColor ?? _settings.ErrorColor,
                _ => throw new ArgumentOutOfRangeException(nameof(compType), compType, null)
            };
        }

        public PortableColor ForegroundFor(RenderStatus status, ColorComponentType compType)
        {
            compType = status switch
            {
                RenderStatus.RecentlyUpdated => ColorComponentType.HIGHLIGHT,
                RenderStatus.EditInProgress => ColorComponentType.PENDING,
                RenderStatus.CorrelationError => ColorComponentType.ERROR,
                _ => compType
            };
            return ColorCompForType(compType).Fg;
        }
    }
}
