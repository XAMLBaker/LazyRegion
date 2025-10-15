using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public static class LazyRegionRegistry
    {
        private static readonly Dictionary<string, ILazyRegion> _regions = new ();
        private static readonly Dictionary<string, TaskCompletionSource<ILazyRegion>> _regionWaiters = new ();
        // ⭐ Loading 설정 추가
        private static Dictionary<string, RegionLoadingConfig>? _loadingConfigs;
        private static ILazyRegionManagerBase _regionManager; 
        private static readonly Dictionary<string, LoadingRegionBehavior> _behaviors = new ();
        public static void SetLoadingConfigs(Dictionary<string, RegionLoadingConfig> configs, ILazyRegionManagerBase regionManager)
        {
            _loadingConfigs = configs;
            _regionManager = regionManager;
        }
        public static void RegisterRegion(string regionName, ILazyRegion region)
        {
            _regions[regionName] = region;

            // ⭐ Loading Behavior 시작
            if (_loadingConfigs?.TryGetValue (regionName, out var config) == true
                        && _regionManager != null)
            {
                var behavior = new LoadingRegionBehavior (
                    regionName,
                    region,
                    config,
                    _regionManager);

                behavior.Attach ();
                _behaviors[regionName] = behavior;
            }

            // 대기 중인 Navigation이 있으면 완료 처리
            if (_regionWaiters.TryGetValue (regionName, out var tcs))
            {
                tcs.TrySetResult (region);
                _regionWaiters.Remove (regionName);
            }
        }

        public static ILazyRegion? GetRegion(string regionName)
        {
            return _regions.TryGetValue (regionName, out var region) ? region : null;
        }

        public static async Task<ILazyRegion> WaitForRegionAsync(string regionName, TimeSpan? timeout = null)
        {
            if (_regions.TryGetValue (regionName, out var region))
                return region;

            if (!_regionWaiters.TryGetValue (regionName, out var tcs))
            {
                tcs = new TaskCompletionSource<ILazyRegion> ();
                _regionWaiters[regionName] = tcs;
            }

            var timeoutTask = timeout.HasValue
                ? Task.Delay (timeout.Value)
                : Task.Delay (Timeout.Infinite);

            var completedTask = await Task.WhenAny (tcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
                throw new TimeoutException ($"Region '{regionName}' was not registered within the timeout period.");

            return await tcs.Task;
        }
        public static void NotifyNavigationCompleted(string regionName, string viewKey)
        {
            if (_behaviors.TryGetValue (regionName, out var behavior))
            {
                behavior.OnNavigationCompleted (viewKey);  // ⭐
            }
        }
    }
}
