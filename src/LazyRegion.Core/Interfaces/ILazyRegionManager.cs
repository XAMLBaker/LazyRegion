using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyRegionManagerBase
{
    Task NavigateAsync(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
}

public interface ILazyRegionManager : ILazyRegionManagerBase
{
    void RegisterView<T>(string viewKey, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, new();
}

public class LazyRegionManager : ILazyRegionManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ViewRegistration> _viewRegistrations = new ();
    private readonly Dictionary<string, object> _singletonCache = new ();
    private readonly Dictionary<string, RegionLoadingConfig> _loadingConfigs;

    public LazyRegionManager(IServiceProvider serviceProvider,
                             LazyViewRegistry registry,
                             Dictionary<string, RegionLoadingConfig> loadingConfigs)
    {
        _serviceProvider = serviceProvider;
        registry.Initialize (this);
        _loadingConfigs = loadingConfigs;
    }

    public void RegisterView<T>(string viewKey, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, new()
    {
        _viewRegistrations[viewKey] = new ViewRegistration
        {
            ViewType = typeof (T),
            Lifetime = lifetime,
            Factory = CreateFactory<T> ()
        };
    }

    public async Task NavigateAsync(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null)
    {
        var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, regionWaitTimeout);
        var view = await GetOrCreateView (viewKey);
        region.Set (view);

        LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
    }
    public async Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null)
    {
        var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, regionWaitTimeout);

        var vm = _serviceProvider.GetService<T> ();
        if (vm == null)
            throw new InvalidOperationException ($"ViewModel Not found: {typeof (T).Name}");

        var view = await GetOrCreateView (viewKey);
        region.Set (view, vm);

        LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
    }

    private async Task<object> GetOrCreateView(string viewKey)
    {
         if (!_viewRegistrations.TryGetValue (viewKey, out var registration))
            throw new InvalidOperationException ($"View '{viewKey}' not registered");

        // Singleton이면 캐시 확인
        if (registration.Lifetime == ServiceLifetime.Singleton)
        {
            if (_singletonCache.TryGetValue (viewKey, out var cachedView))
                return cachedView;
        }

        //// 무거운 작업 시뮬레이션
        //await Task.Delay (100);

        // 뷰 생성
        var view = registration.Factory ();

        // Singleton이면 캐시에 저장
        if (registration.Lifetime == ServiceLifetime.Singleton)
        {
            _singletonCache[viewKey] = view;
        }

        return view;
    }

    private Func<object> CreateFactory<T>() where T : class, new()
    {
        if (_serviceProvider != null)
        {
            return () => _serviceProvider.GetService<T> () ?? ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T));
        }

        return () => new T ();
    }

    private class ViewRegistration
    {
        public Type ViewType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Func<object> Factory { get; set; }
    }
}