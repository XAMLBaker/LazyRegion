using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core;

public static class LazyExtensions
{
    private static readonly List<Action<LazyViewRegistry>> _pendingRegistrations = new ();
    private static readonly List<Action<ILazyRegionManager>> _pendingNavigations = new (); // ✅ 추가

    public static IServiceCollection UseLazyRegion(this IServiceCollection services,
                                                   Action<ILazyRegionManager>? configure = null)
    {
        services.AddSingleton<LazyViewRegistry> (sp =>
        {
            var registry = new LazyViewRegistry ();
            foreach (var action in _pendingRegistrations)
            {
                action (registry);
            }
            _pendingRegistrations.Clear ();
            return registry;
        });

        services.AddSingleton<ILazyRegionManager> (sp =>
        {
            var registry = sp.GetRequiredService<LazyViewRegistry> ();
            var manager = new LazyRegionManager (sp, registry, _regionLoadingConfigs);

            // ✅ 저장된 Navigation들 실행
            foreach (var navigation in _pendingNavigations)
            {
                navigation (manager);
            }
            _pendingNavigations.Clear ();
            // ⭐ ConfigureRegions에서 등록한 초기화 실행
            var initializers = sp.GetService<Action<ILazyRegionManager>> ();
            initializers?.Invoke (manager);
            return manager;
        });
        return services;
    }

    public static IServiceCollection AddLazyView<T>(this IServiceCollection services,
                                                    string viewKey,
                                                    ServiceLifetime lifetime = ServiceLifetime.Singleton)
                                           where T : class, new()
    {
        // DI Container에 뷰 등록
        services.Add (new ServiceDescriptor (typeof (T), typeof (T), lifetime));

        _pendingRegistrations.Add (registry => registry.Add<T> (viewKey, lifetime));
        return services;
    }
    private static Dictionary<string, RegionLoadingConfig>? _regionLoadingConfigs;

    public static IServiceCollection ConfigureRegions(
        this IServiceCollection services,
        Action<IRegionConfigurationBuilder> configure)
    {
        var builder = new RegionConfigurationBuilder (services);
        configure (builder);

        _regionLoadingConfigs = builder.Build ();

        // DI에 설정 등록
        services.AddSingleton (_regionLoadingConfigs);
        // ⭐ 초기화 시 실행될 액션 저장
        services.AddSingleton<Action<ILazyRegionManager>> (manager =>
        {
            LazyRegionRegistry.SetLoadingConfigs (_regionLoadingConfigs, manager);
        });
        return services;
    }

    public static RegionLoadingConfig? GetLoadingConfig(string regionName)
    {
        return _regionLoadingConfigs?.GetValueOrDefault (regionName);
    }
    public static IServiceCollection ConfigureInitialNavigation(
                this IServiceCollection services,
                Action<ILazyRegionManagerBase> configure)
    {
        // ✅ 일단 static에 저장
        _pendingNavigations.Add (configure);
        return services;
    }
}
