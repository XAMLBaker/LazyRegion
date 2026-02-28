using LazyRegion.Core;
using LazyRegion.WPF;
using StaticSample.Views;
using System.Windows;

namespace StaticSample;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup (e);

        // 🔥 Static 방식으로 View 등록 및 InitialFlow 설정
        LazyRegionApp.Default
            .UseWpf(app =>
            {
                app.Register<HomeView> ("home")
                   .Register<LoginView> ("login")
                   .Configure (config =>
                   {
                       config.ForRegion ("Root")
                             .WithInitialFlow (flow => flow.Show ("home"));
                   });
            });

        new MainWindow ().Show ();
    }
}
