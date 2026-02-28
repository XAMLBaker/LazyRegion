using LazyRegion.Core;
using System.Windows.Controls;

namespace StaticSample.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent ();
    }

    private async void GoToLogin_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await LazyRegionApp.Default.RegionManager.NavigateAsync ("Root", "login");
    }
}
