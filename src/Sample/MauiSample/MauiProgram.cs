using Microsoft.Extensions.Logging;
using LazyRegion.Core;
using LazyRegion.Maui;
using MauiSample.Views;
namespace MauiSample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder ();
            builder
                .UseMauiApp<App> ()
                .UseLazyRegion (lazy =>
                {
                    lazy.Register<ScreenA> ("A");
                    lazy.Register<ScreenB> ("B");

                    lazy.ConfigureRegions (config =>
                          {
                              config.ForRegion ("Root")
                                    .WithInitialFlow (flow =>
                                    {
                                        flow.Show ("A");
                                    });
                          });
                })
                .ConfigureFonts (fonts =>
                {
                    fonts.AddFont ("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont ("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            return builder.Build ();
        }
    }
}
