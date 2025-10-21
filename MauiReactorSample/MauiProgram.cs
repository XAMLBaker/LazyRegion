using MauiReactorSample.Components;
using MauiReactorSample.Resources.Styles;

namespace MauiReactorSample
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
                .ConfigureFonts (fonts =>
                {
                    fonts.AddFont ("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont ("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            return builder.Build ();
        }
    }
}
