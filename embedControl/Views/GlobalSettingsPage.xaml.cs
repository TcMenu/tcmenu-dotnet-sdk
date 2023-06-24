using embedCONTROL.Services;
using System.Globalization;
using System.Xml;
using TcMenuCoreMaui.Services;

namespace embedControl.Views;

public partial class GlobalSettingsPage : ContentPage
{
    public PrefsAppSettings LocalSettings { get; set; }
	
    public GlobalSettingsPage()
	{
        LocalSettings = new PrefsAppSettings();
        LocalSettings.CloneSettingsFrom(ApplicationContext.Instance.AppSettings);

        InitializeComponent();

        BindingContext = LocalSettings;
        UpdateAllColors();
    }

    private void UUIDButton_Clicked(object sender, EventArgs e)
    {
        LocalSettings.UniqueId = Guid.NewGuid().ToString();
        UUIDTextField.Text = LocalSettings.UniqueId;
    }

    private void ResetColorsToDefault(object sender, EventArgs e)
    {
        LocalSettings.SetColorsForMode();
        UpdateAllColors();
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        var settings = ApplicationContext.Instance.AppSettings;
        settings.CloneSettingsFrom(LocalSettings);

        settings.ButtonColor = ButtonColorPicker.ItemColor;
        settings.ErrorColor = ErrorColorPicker.ItemColor;
        settings.PendingColor = PendingColorPicker.ItemColor;
        settings.UpdateColor = UpdateColorPicker.ItemColor;
        settings.TextColor = TextColorPicker.ItemColor;
        settings.HighlightColor = HighlightColorPicker.ItemColor;
        settings.DialogColor = DialogColorPicker.ItemColor;
        ApplicationContext.Instance.SaveAllSettings();
    }

    private void UpdateAllColors()
    {
        TextColorPicker.ItemColor = LocalSettings.TextColor;
        ButtonColorPicker.ItemColor = LocalSettings.ButtonColor;
        UpdateColorPicker.ItemColor = LocalSettings.UpdateColor;
        PendingColorPicker.ItemColor = LocalSettings.PendingColor;
        HighlightColorPicker.ItemColor = LocalSettings.HighlightColor;
        ErrorColorPicker.ItemColor = LocalSettings.ErrorColor;
        DialogColorPicker.ItemColor = LocalSettings.DialogColor;
    }
}