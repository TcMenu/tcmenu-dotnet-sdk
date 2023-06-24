using System.Collections.Immutable;
using embedControl.Services;
using embedCONTROL.Services;

namespace embedControl.Views;

public partial class MyConnectionsPage : ContentPage
{
    public IReadOnlyCollection<TcMenuPanelSettings> AllConnections =>ApplicationContext.Instance.MenuPersitence.AllItems;
    public IReadOnlyCollection<TcMenuPanelSettings> RecentConnections =>ApplicationContext.Instance.MenuPersitence.AllItems
        .OrderByDescending(ps => ps.LastOpened).Take(2).ToImmutableList();

    public MyConnectionsPage()
	{
        InitializeComponent();
        BindingContext = this;
    }

    private async void NavigateSelectedItem(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is TcMenuPanelSettings settings && sender is ListView lv)
        {
            settings.LastOpened = DateTime.Now;
            ApplicationContext.Instance.MenuPersitence.Update(settings);

            var page = new TcMenuConnectionPage(settings);
            await Shell.Current.Navigation.PushAsync(page);

            lv.SelectedItem = null;
        }
    }
}