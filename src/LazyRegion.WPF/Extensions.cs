using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace LazyRegion.WPF;

public static class Extensions
{
    public static IServiceCollection UseLazyRegion(
        this IServiceCollection services,
        Action<LazyRegionBuilder> configure = null)
    {
        if (configure != null)
        {
            var lazyBuilder = new LazyRegionBuilder (services);
            configure (lazyBuilder);
        }

        services.UseLazyRegionCore ();

        LazyRegionRegistry.NavigateHandler = async (mgr, regionName, viewKey) =>
        {
            await Application.Current.Dispatcher.InvokeAsync (async () =>
            {
                await mgr.NavigateAsync (regionName, viewKey);
            });
        };

        return services;
    }
}
