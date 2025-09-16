using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using SampleScreen.Base;
using System.Windows;

namespace RegionManager_1
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
            serviceCollection.UseLazyRegion ();
            serviceCollection.AddLazyView<ScreenA> ("a");
            serviceCollection.AddLazyView<ScreenB> ("b");
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
