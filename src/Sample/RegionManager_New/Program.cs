using LazyRegion.WPF;
using LazyVoom.Core;
using RegionManager_Version2;
using RegionManager_Version2.ViewModels;
using RegionManager_Version2.Views;

var builder = Host.CreateApplicationBuilder ();
builder.Services.AddSingleton<MainViewModel> ();

builder.Services.UseLazyRegion (lazy =>
                {
                    lazy.Register<SplashView> ("Splash");
                    lazy.Register<LoginView> ("Login");
                    lazy.Register<MainView> ("Main");

                    lazy.ConfigureRegions (config =>
                    {
                        config.ForRegion ("Root")
                              .WithInitialFlow (flow =>
                              {
                                  flow.Show ("Splash")
                                      .Then ("Main", async () =>
                                      {
                                          await Task.Delay (5000);
                                          return false;
                                      })
                                      .Then ("Login");
                              });
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
