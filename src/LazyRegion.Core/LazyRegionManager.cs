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
            var baseRegion = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            var view = GetOrCreate (viewKey);
            if (baseRegion is ILazyRegion region)
                region.Set (view);


            LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
            RegionMap.Register (regionName, view);
        }

        public async Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var baseRegion = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            var vm = _sp.GetRequiredService<T> ();
            var view = GetOrCreate (viewKey);

            if (baseRegion is ILazyRegion region)
                region.Set (view,vm);

            LazyRegionRegistry.NotifyNavigationCompleted (regionName, viewKey);
            RegionMap.Register (regionName, view);
        }

        private object GetOrCreate(string key)
        {
            var reg = _views[key];

            var view = reg.Factory (_sp);

            return view;
        }
        /// <summary>
        /// ILazyRegionItems를 구현한 Region에 항목을 추가합니다.
        /// </summary>
        public async Task AddItemAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            if (region is ILazyRegionItems itemsRegion)
            {
                var view = GetOrCreate (viewKey);
                itemsRegion.AddItem (viewKey, view);
            }
        }

        /// <summary>
        /// ILazyRegionItems를 구현한 Region에 여러 항목을 추가합니다.
        /// </summary>
        public async Task AddItemsAsync(
            string regionName,
            string[] viewKeys,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync(regionName, timeout);
            if (region is ILazyRegionItems itemsRegion)
            {
                foreach (var viewKey in viewKeys)
                {
                    var view = GetOrCreate(viewKey);
                    itemsRegion.AddItem(viewKey, view);
                    await Task.Delay (150);
                }
            }
        }

        /// <summary>
        /// ILazyRegionItems를 구현한 Region에서 항목을 제거합니다.
        /// </summary>
        public async Task RemoveItemAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            if (region is ILazyRegionItems itemsRegion)
            {
                var view = GetOrCreate (viewKey);
                itemsRegion.RemoveItem (viewKey, view);
            }
        }

        /// <summary>
        /// ILazyRegionItems를 구현한 Region의 모든 항목을 제거합니다.
        /// </summary>
        public async Task ClearItemsAsync(
            string regionName,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            if (region is ILazyRegionItems itemsRegion)
            {
                itemsRegion.ClearItems ();
            }
        }

        /// <summary>
        /// ILazyRegionItems를 구현한 Region에 항목을 지정된 위치에 삽입합니다.
        /// </summary>
        public async Task InsertItemAsync(
            string regionName,
            int index,
            string viewKey,
            TimeSpan? timeout = null)
        {
            var region = await LazyRegionRegistry.WaitForRegionAsync (regionName, timeout);
            if (region is ILazyRegionItems itemsRegion)
            {
                var view = GetOrCreate (viewKey);
                itemsRegion.InsertItem (index, viewKey, view);
            }
        }
    }
}
