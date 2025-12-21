using LazyRegion.Core;

namespace LazyRegion.Maui;

public static class Extensions
{
    public static MauiAppBuilder UseLazyRegion(
        this MauiAppBuilder builder,
        Action<LazyRegionBuilder>? lazy = null)
    {
        if (lazy != null)
        {
            var lazyBuilder = new LazyRegionBuilder (builder.Services);
            lazy (lazyBuilder);
        }

        builder.Services.UseLazyRegionCore ();

        LazyRegionRegistry.NavigateHandler = async (mgr, regionName, viewKey) =>
        {
            await MainThread.InvokeOnMainThreadAsync (async () =>
            {
                await mgr.NavigateAsync (regionName, viewKey);
            });
        };

        return builder;
    }
}
