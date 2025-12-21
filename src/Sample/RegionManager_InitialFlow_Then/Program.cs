using LazyRegion.WPF;
using LazyRegion.Core;
using RegionManager_InitialFlow_Then;
using RegionManager_InitialFlow_Then.ViewModels;
using RegionManager_InitialFlow_Then.Views;
using LazyVoom.Core;

var builder = Host.CreateApplicationBuilder ();
builder.Services.AddSingleton<MainViewModel> ();

builder.Services.UseLazyRegion ()
                .AddLazyView<SplashView>("Splash")
                .AddLazyView<LoginView> ("Login")
                .AddLazyView<MainView> ("Main")
                .ConfigureRegions (configure =>
                {
                    configure.ForRegion ("Root")
                             .WithInitialFlow (flow =>
                             {
                                 flow.Show ("Splash")
                                     .Then ("Main", async() =>
                                     {
                                         await Task.Delay (5000);
                                         return false;
                                     })
                                     .Then ("Login");
                             });
                });



var app = builder.BuildApp<App, MainWindow> ();  // 🔥

app.OnStartUpAsync = async provider =>
{
    Voom.Instance
        .WithContainerResolver (type =>
        {
            return provider.GetService (type);
        })
        .WithMapping<MainWindow, MainViewModel> ();
};
// Exit 시 정리
app.OnExitAsync = async provider =>
{
    Console.WriteLine ("앱 종료 중...");
    await Task.Delay (200);
};

app.Run ();
