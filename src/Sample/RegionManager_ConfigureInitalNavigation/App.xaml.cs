using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using RegionManager_ConfigureInitalNavigation.ViewModels;
using SampleScreen.Base;
using System.Windows;

namespace RegionManager_ConfigureInitalNavigation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null;

        public App()
        {
            var serviceCollection = new ServiceCollection ();
            serviceCollection.UseLazyRegion ()
                             .AddLazyView<ScreenA> ("a")
                             .AddLazyView<ScreenB> ("b")
                             .ConfigureInitialNavigation (configure =>
                             {
                                 configure.NavigateAsync ("Root", "a");
                             });

            serviceCollection.AddSingleton<MainViewModel> ();
            serviceCollection.AddSingleton<MainWindow> ();

            Services = serviceCollection.BuildServiceProvider ();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = Services.GetService<MainWindow> ();
            mainWindow.Show ();
        }
    }
}
