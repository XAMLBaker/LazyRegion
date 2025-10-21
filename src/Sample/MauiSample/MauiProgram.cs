using Microsoft.Extensions.Logging;
using LazyRegion.Core;
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
                .ConfigureFonts (fonts =>
                {
                    fonts.AddFont ("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont ("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.UseLazyRegion ()
                            .AddLazyView<ScreenA> ("A")
                            .AddLazyView<ScreenB> ("B")
                            .ConfigureInitialNavigation (configure =>
                            {
                                configure.NavigateAsync ("Root", "A");
                            });
            return builder.Build ();
        }
    }
}
