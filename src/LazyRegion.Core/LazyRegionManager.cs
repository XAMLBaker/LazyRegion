using LazyRegion.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public sealed class LazyRegionManager : ILazyRegionManager
    {
        private readonly IServiceProvider _sp;
        private readonly Dictionary<string, ViewRegistration> _views = new();
        private readonly ConcurrentDictionary<string, object?> _regionViewModels = new();
        private readonly ConcurrentDictionary<string, string?> _previousViewKeys = new();
        private readonly ConcurrentDictionary<string, string> _currentViewKeys = new();

        public IServiceProvider ServiceProvider => _sp;
        public LazyRegionManager(
            IServiceProvider sp,
            LazyViewRegistry registry,
            RegionLoadingOptions? options = null)
        {
            _sp = sp;
            registry.Initialize(this);

            if (options != null)
                LazyRegionRegistry.Initialize(options, this);
        }

        public void RegisterView(
            string viewKey,
            ServiceLifetime lifetime,
            LazyFactory factory,
            Type? viewModelType = null)
        {
            _views[viewKey] = new ViewRegistration
            {
                ViewKey = viewKey,
                Lifetime = lifetime,
                Factory = factory,
                ViewModelType = viewModelType
            };
        }

        public object GetView(string viewKey) => GetOrCreate(viewKey);

        public Task NavigateAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, null, null, null, timeout);

        public Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, _sp.GetRequiredService<T>(), null, null, timeout);

        public Task NavigateAsync(
            string regionName,
            string viewKey,
            LazyNavigationParameters parameters,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, null, parameters, null, timeout);

        public Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            LazyNavigationParameters parameters,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, _sp.GetRequiredService<T>(), parameters, null, timeout);

        public Task NavigateAsync(
            string regionName,
            string viewKey,
            TransitionAnimation animation,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, null, null, animation, timeout);

        public Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            TransitionAnimation animation,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, _sp.GetRequiredService<T>(), null, animation, timeout);

        public Task NavigateAsync(
            string regionName,
            string viewKey,
            LazyNavigationParameters parameters,
            TransitionAnimation animation,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, null, parameters, animation, timeout);

        public Task NavigateAsync<T>(
            string regionName,
            string viewKey,
            LazyNavigationParameters parameters,
            TransitionAnimation animation,
            TimeSpan? timeout = null)
            => NavigateInternalAsync(regionName, viewKey, _sp.GetRequiredService<T>(), parameters, animation, timeout);

        private async Task NavigateInternalAsync(
            string regionName,
            string viewKey,
            object? viewModel,
            LazyNavigationParameters? parameters,
            TransitionAnimation? animationOverride,
            TimeSpan? timeout)
        {
            var context = new LazyNavigationContext(regionName, viewKey, parameters);

            var baseRegion = await LazyRegionRegistry.WaitForRegionAsync(regionName, timeout);

            // 구 ViewModel: 플랫폼에서 직접 읽음 (Guard/OnNavigatedFrom 용)
            var oldVm = (baseRegion as ILazyRegion)?.GetCurrentDataContext();

            if (oldVm is ILazyNavigationGuard guard)
            {
                if (!await guard.CanNavigateAsync(context))
                    return;
            }

            if (oldVm is ILazyNavigationAware oldAware)
                oldAware.OnNavigatedFrom(context);

            var view = GetOrCreate(viewKey);

            if (baseRegion is ILazyRegion region)
            {
                if (animationOverride.HasValue)
                    region.Set(view, viewModel, animationOverride);
                else
                    region.Set(view, viewModel);
            }

            // 신 ViewModel: 명시적 파라미터 우선, 없으면 플랫폼에서 읽음 (생성자 주입 포함)
            var newVm = viewModel ?? (baseRegion as ILazyRegion)?.GetStagingDataContext();

            if (newVm is ILazyNavigationAware newAware)
                newAware.OnNavigatedTo(context);

            _regionViewModels[regionName] = newVm;

            // Track viewKeys for GoBack
            _currentViewKeys.TryGetValue(regionName, out var oldViewKey);
            _previousViewKeys[regionName] = oldViewKey;
            _currentViewKeys[regionName] = viewKey;

            LazyRegionRegistry.NotifyNavigationCompleted(regionName, viewKey);
            RegionMap.Register(regionName, view);
        }

        public bool CanGoBack(string regionName)
            => _previousViewKeys.TryGetValue(regionName, out var key) && key != null;

        public async Task<bool> GoBackAsync(string regionName, TimeSpan? timeout = null)
        {
            if (!CanGoBack(regionName))
                return false;

            var prevKey = _previousViewKeys[regionName]!;
            var baseRegion = await LazyRegionRegistry.WaitForRegionAsync(regionName, timeout);

            TransitionAnimation? reverseAnim = null;
            if (baseRegion is ILazyRegion region)
                reverseAnim = TransitionAnimationHelper.GetReverse(region.CurrentAnimation);

            // GoBack 후에는 다시 GoBack 불가 (depth=1)
            _previousViewKeys[regionName] = null;

            await NavigateInternalAsync(regionName, prevKey, null, null, reverseAnim, timeout);
            return true;
        }

        public Task NavigateGroupAsync(
            params (string regionName, string viewKey, TransitionAnimation? animation)[] navigations)
        {
            var tasks = navigations.Select(n =>
                NavigateInternalAsync(n.regionName, n.viewKey, null, null, n.animation, null));
            return Task.WhenAll(tasks);
        }

        private object GetOrCreate(string key)
        {
            if (string.IsNullOrWhiteSpace (key))
                throw new ArgumentException ("View key cannot be null or empty.", nameof (key));

            if (!_views.TryGetValue (key, out var reg))
                throw new InvalidOperationException (
                    $"View '{key}' is not registered. Call AddLazyView or Register before navigating.");

            if (reg.Lifetime == ServiceLifetime.Singleton)
            {
                if (reg.Instance == null)
                {
                    reg.Instance = reg.Factory(_sp);
                }
                return reg.Instance;
            }

            return reg.Factory(_sp);
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
                await itemsRegion.AddItem (viewKey, view);
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
                    await itemsRegion.AddItem(viewKey, view);
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
                await itemsRegion.RemoveItem (viewKey, view);
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
                await itemsRegion.ClearItems ();
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
                await itemsRegion.InsertItem (index, viewKey, view);
            }
        }
    }
}

