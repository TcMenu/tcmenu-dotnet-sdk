using embedControl.Services;
using embedCONTROL.Services;

namespace embedControl.Views;

public partial class MyConnectionsPage : ContentPage
{
    public IReadOnlyCollection<TcMenuPanelSettings> AllConnections =>ApplicationContext.Instance.MenuPersitence.AllItems;
    public IReadOnlyCollection<TcMenuPanelSettings> ActiveConnections =>ApplicationContext.Instance.MenuPersitence.AllItems;

    public MyConnectionsPage()
	{
        InitializeComponent();
        BindingContext = this;
    }
}