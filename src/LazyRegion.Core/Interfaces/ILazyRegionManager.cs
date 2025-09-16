using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyRegionManager
{
    void RegisterRegion(string regionName, ILazyRegion region);
    Task NavigateAsync(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
    void RegisterView<T>(string viewKey, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class, new();
}

public class LazyRegionService : ILazyRegionManager
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ILazyRegion>> _regionWaiters = new ();
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ViewRegistration> _viewRegistrations = new ();
    private readonly Dictionary<string, object> _singletonCache = new ();
    private readonly Dictionary<string, ILazyRegion> _regions = new ();

    public LazyRegionService(IServiceProvider serviceProvider,
                             LazyViewRegistry registry)
    {
        _serviceProvider = serviceProvider;
        LazyRegionManager.Initialize (this);

        var actions =_serviceProvider.GetServices<IStartupAction> ();
        foreach (var action in actions)
            action.Execute ();
        registry.Initialize (this);
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

    private async Task<ILazyRegion> GetRegionAsync(string regionName, TimeSpan? regionWaitTimeout = null)
    {
        if (_regions.TryGetValue (regionName, out var region))
            return region;

        // Region이 등록될 때까지 대기
        var tcs = _regionWaiters.GetOrAdd (regionName, _ => new TaskCompletionSource<ILazyRegion> ());

        var actualTimeout = regionWaitTimeout ?? TimeSpan.FromSeconds (30); // 기본 30초

        var timeoutTask = Task.Delay (actualTimeout);
        var completedTask = await Task.WhenAny (tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException ($"등록되지 않은 Region : {regionName} 입니다.");
        }
        return await tcs.Task;
    }

    // Region 등록 시 호출
    public void RegisterRegion(string regionName, ILazyRegion region)
    {
        _regions[regionName] = region;

        // 대기 중인 Task들에게 알림
        if (_regionWaiters.TryRemove (regionName, out var tcs))
        {
            tcs.SetResult (region);
        }
    }

    public async Task NavigateAsync(string regionName, string viewKey, TimeSpan? timeout = null)
    {
        var region = await GetRegionAsync (regionName);

        region.Set(await GetOrCreateView (viewKey));
    }

    public async Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? timeout = null)
    {
        var region = await GetRegionAsync (regionName);

        var vm = this._serviceProvider.GetService<T> ();
        if(vm == null)
            throw new InvalidOperationException ($"ViewModel Not found");

        var viewType = await GetOrCreateView (viewKey);
        region.Set (viewType, vm);
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
            return () => _serviceProvider.GetService<T> () ?? new T ();
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