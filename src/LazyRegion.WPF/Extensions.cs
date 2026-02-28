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
        if (LazyRegionApp.HasPendingRegistrations)
            LazyRegionApp.ApplyTo (services);

        if (configure != null)
        {
            var lazyBuilder = new LazyRegionBuilder (services);
            configure (lazyBuilder);
        }

        services.UseLazyRegionCore ();

        SetWpfNavigateHandler ();

        return services;
    }

    /// <summary>
    /// Static 방식에서 WPF Dispatcher 연결 및 RegionManager 지연 초기화를 설정합니다.
    /// Register/Configure 앞뒤 어디서든 호출할 수 있습니다.
    /// </summary>
    public static LazyRegionApp UseWpf(this LazyRegionApp app)
    {
        SetWpfNavigateHandler ();

        // 첫 번째 Region이 등록될 때 RegionManager 초기화 (위치 무관)
        LazyRegionRegistry.OnManagerRequested = () => _ = app.RegionManager;

        return app;
    }

    public static void UseWpf(this LazyRegionApp app, Action<LazyRegionApp> configure)
    {
        SetWpfNavigateHandler ();
        configure (app);
        _ = app.RegionManager; // configure 뒤에 초기화
    }

    private static void SetWpfNavigateHandler()
    {
        LazyRegionRegistry.NavigateHandler = async (mgr, regionName, viewKey) =>
        {
            await Application.Current.Dispatcher.InvokeAsync (async () =>
            {
                await mgr.NavigateAsync (regionName, viewKey);
            });
        };
    }
}
