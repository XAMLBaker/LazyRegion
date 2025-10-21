using MauiReactorSample.Components;
using Test.Components;
using Test.Resources.Styles;
using LazyRegion.Maui.ReactorMaui;
using LazyRegion.Core;

namespace Test
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder ();
            builder
                .UseMauiReactorApp<HomePage> (app =>
                {
                    app.UseTheme<ApplicationTheme> ();
                },
                unhandledExceptionAction: e =>
                {
                    System.Diagnostics.Debug.WriteLine (e.ExceptionObject);
                })
                .UseLazyRegion (region =>
                {
                    region.AddLazyView<ScreenAComponent> ("A")
                        .AddLazyView<ScreenBComponent> ("B")
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

            return builder.Build ();
        }
    }
}
