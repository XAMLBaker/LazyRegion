using LazyRegion.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace LazyRegion.WinUI3;

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
    public static LazyRegionApp UseWinUI3(this LazyRegionApp app)
    {
        SetWpfNavigateHandler ();

        // 첫 번째 Region이 등록될 때 RegionManager 초기화 (위치 무관)
        LazyRegionRegistry.OnManagerRequested = () => _ = app.RegionManager;

        return app;
    }

    public static void UseWinUI3(this LazyRegionApp app, Action<LazyRegionApp> configure)
    {
        SetWpfNavigateHandler ();
        configure (app);
        _ = app.RegionManager; // configure 뒤에 초기화
    }

    private static void SetWpfNavigateHandler()
    {
        LazyRegionRegistry.NavigateHandler = async (mgr, regionName, viewKey) =>
        {
            // WinUI3: 현재 스레드의 DispatcherQueue 가져오기
            var dq = DispatcherQueue.GetForCurrentThread ();
            if (dq != null)
            {
                var tcs = new TaskCompletionSource<bool> ();
                dq.TryEnqueue (async () =>
                {
                    try
                    {
                        await mgr.NavigateAsync (regionName, viewKey);
                        tcs.SetResult (true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException (ex);
                    }
                });
                await tcs.Task;
            }
            else
            {
                // DispatcherQueue를 못 구하면 바로 호출(테스트/비UI 상황)
                await mgr.NavigateAsync (regionName, viewKey);
            }
        };
    }
}
