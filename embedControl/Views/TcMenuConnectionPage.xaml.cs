using embedCONTROL.Services;

namespace embedControl.Views;

public partial class TcMenuConnectionPage : ContentPage
{
    public TcMenuConnectionModel ConnectionModel;
	public TcMenuConnectionPage()
	{
		InitializeComponent();
        ConnectionModel = new TcMenuConnectionModel(null, ApplicationContext.Instance.AppSettings);
        BindingContext = ConnectionModel;
    }

    private void OnDialogChanged(object sender, EventArgs e)
    {
            ConnectionModel.SendDialogEvent(sender.Equals(DlgBtn1) ? 0 : 1);
    }
}