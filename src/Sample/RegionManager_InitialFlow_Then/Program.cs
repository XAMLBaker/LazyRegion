using LazyRegion.Core;
using LazyVoom.Core;
using RegionManager_InitialFlow_Then;
using RegionManager_InitialFlow_Then.ViewModels;
using RegionManager_InitialFlow_Then.Views;

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
                                 flow.Show ("Splash");
                                     //.Then ("Main", () => 
                                     //{
                                     //    return true;
                                     //})
                                     //.Then ("Login");
                             });
                });
var app = builder.BuildApp<App, MainWindow> ();  // 🔥

app.OnStartUpAsync = async provider =>
{
    Voom.Instance.WithMapping<MainWindow, MainViewModel> ();
};
// Exit 시 정리
app.OnExitAsync = async provider =>
{
    Console.WriteLine ("앱 종료 중...");
    await Task.Delay (200);
};

app.Run ();
