using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace LazyRegion.WPF;

public static class Extensions
{
    public static IServiceCollection UseLazyRegion(this IServiceCollection services)
    {
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
