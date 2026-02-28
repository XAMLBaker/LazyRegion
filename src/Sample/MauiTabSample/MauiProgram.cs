using LazyRegion.Core;
using LazyRegion.Maui;
using MauiTabSample.Views;

namespace MauiTabSample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Register tab views via LazyRegionApp (no DI constructor needed)
        LazyRegionApp.Default
            .Register<TabScreenA> ("TabA")
            .Register<TabScreenB> ("TabB")
            .Register<TabScreenC> ("TabC")
            .Register<TabScreenD> ("TabD");

        var builder = MauiApp.CreateBuilder ();
        builder
            .UseMauiApp<App> ()
            .UseLazyRegion ();  // drains pending registrations into DI

        return builder.Build ();
    }
}
