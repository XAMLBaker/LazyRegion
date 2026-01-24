using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public sealed class LazyRegionManager : ILazyRegionManager
    {
        private readonly IServiceProvider _sp;
        private readonly Dictionary<string, ViewRegistration> _views = new ();
        private readonly Dictionary<string, object> _singletons = new ();

        public IServiceProvider ServiceProvider => _sp;
        public LazyRegionManager(
            IServiceProvider sp,
            LazyViewRegistry registry,
            RegionLoadingOptions? options = null)
        {
            _sp = sp;
            registry.Initialize (this);

            if (options != null)
                LazyRegionRegistry.Initialize (options, this);
        }

        public void RegisterView(
            string viewKey,
            ServiceLifetime lifetime,
            LazyFactory factory)
        {
            _views[viewKey] = new ViewRegistration
            {
                ViewKey = viewKey,
                Lifetime = lifetime,
                Factory = factory
            };
        }

        public async Task NavigateAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            var view = GetOrCreate (viewKey);
            region.Set (view);

            LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
            RegionMap.Register (regionName, view);
        }

        public async Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            var vm = _sp.GetRequiredService<T> ();
            var view = GetOrCreate (viewKey);

            region.Set (view, vm);
            LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
            RegionMap.Register (regionName, view);
        }

        private object GetOrCreate(string key)
        {
            var reg = _views[key];

            if (reg.Lifetime == ServiceLifetime.Singleton &&
                _singletons.TryGetValue (key, out var cached))
                return cached;

            var view = reg.Factory (_sp);

            if (reg.Lifetime == ServiceLifetime.Singleton)
                _singletons[key] = view;

            return view;
        }
    }
}
