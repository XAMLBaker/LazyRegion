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
                .UseLazyRegion (region =>
                {
                    region.AddLazyView<ScreenA> ("A")
                          .AddLazyView<ScreenB> ("B")
                          .ConfigureInitialNavigation (configure =>
                          {
                              configure.NavigateAsync ("Root", "A");
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
