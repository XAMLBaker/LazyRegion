using LazyRegion.WPF;
using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using RegionManager_RegionState.ViewModels;
using RegionManager_RegionState.Views;
using SampleScreen.Base;
using System.Windows;

namespace RegionManager_RegionState
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
                             .AddLazyView<LoadingView>("Loading")
                             .AddLazyView<ErrorView>("Error")
                             .ConfigureRegions (configure =>
                             {
                                 configure.ForRegion ("Root")
                                          .WithLoadingBehavior (state =>
                                          {
                                              state.Loading ("Loading")
                                                     .MinDisplayTime (TimeSpan.FromSeconds (5));

                                              state.Error ("Error")
                                                     .Timeout (TimeSpan.FromSeconds (10));
                                          });
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
