using LazyRegion.Core;
using System.Windows.Controls;

namespace StaticSample.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent ();
    }

    private async void GoToHome_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await LazyRegionApp.Default.RegionManager.NavigateAsync ("Root", "home");
    }
}
