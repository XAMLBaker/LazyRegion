using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public static class LazyRegionRegistry
    {
        private static readonly Dictionary<string, ILazyRegion> _regions = new ();
        private static readonly Dictionary<string, TaskCompletionSource<ILazyRegion>> _waiters = new ();
        private static readonly Dictionary<string, LoadingRegionBehavior> _behaviors = new ();
        private static readonly Dictionary<string, ILazyRegion> _pendingRegions = new ();

        private static RegionLoadingOptions? _options;
        private static ILazyRegionManagerBase? _manager;

        private static readonly HashSet<string> _initialFlowExecuted = new ();

        // 기존 API 유지
        public static void SetLoadingConfigs(
            Dictionary<string, RegionLoadingConfig> configs,
            ILazyRegionManagerBase manager)
        {
            _options = new RegionLoadingOptions ();
            foreach (var kv in configs)
                _options.Regions[kv.Key] = kv.Value;

            _manager = manager;
        }

        // 신규 API
        public static void Initialize(
            RegionLoadingOptions options,
            ILazyRegionManagerBase manager)
        {
            _options = options;
            _manager = manager;

            // 🔥 이미 생성된 Region이 있다면 즉시 소비
            foreach (var kv in _pendingRegions)
            {
                RegisterRegion (kv.Key, kv.Value);
            }

            _pendingRegions.Clear ();
        }

        public static void RegisterRegion(string name, ILazyRegion region)
        {
            if (_manager == null)
            { 
                // Manager가 아직 없으면 예약
                _pendingRegions[name] = region;
            }

            _regions[name] = region;

            if (_options?.TryGet (name, out var cfg) == true &&
             _manager != null)
            {
                if (cfg.InitialFlow != null &&
                    !_initialFlowExecuted.Contains (name))
                {
                    _initialFlowExecuted.Add (name);
                    _ = RunInitialFlowAsync (name, cfg.InitialFlow);
                }

                if (!_behaviors.ContainsKey (name))
                {
                    var b = new LoadingRegionBehavior (name, region, cfg, _manager);
                    b.Attach ();
                    _behaviors[name] = b;
                }
            }

            if (_waiters.TryGetValue (name, out var tcs))
            {
                tcs.TrySetResult (region);
                _waiters.Remove (name);
            }
        }
        private static async Task RunInitialFlowAsync(
        string regionName,
        InitialRegionFlow flow)
        {
            foreach (var step in flow.Steps)
            {
                if (step.Condition != null &&
                    !Evaluate (step.Condition))
                    continue;

                await _manager!.NavigateAsync (regionName, step.ViewKey);
                break; // ⭐ 최초 만족 조건만 실행
            }
        }
        private static bool Evaluate(Func<IServiceProvider, bool> cond)
        {
            // LazyRegionManager에 ServiceProvider 노출 필요
            if (_manager is LazyRegionManager mgr)
                return cond (mgr.ServiceProvider);

            return false;
        }

        public static async Task<ILazyRegion> WaitForRegionAsync(
            string name,
            TimeSpan? timeout = null)
        {
            if (_regions.TryGetValue (name, out var region))
                return region;

            var tcs = _waiters.TryGetValue (name, out var w)
                ? w
                : _waiters[name] = new ();

            var delay = timeout.HasValue
                ? Task.Delay (timeout.Value)
                : Task.Delay (Timeout.Infinite);

            var done = await Task.WhenAny (tcs.Task, delay);
            if (done == delay)
                throw new TimeoutException (name);

            return await tcs.Task;
        }

        public static void NotifyNavigationCompleted(string region, string viewKey)
        {
            if (_behaviors.TryGetValue (region, out var b))
                b.OnNavigationCompleted (viewKey);
        }

        public static void UnregisterRegion(string name)
        {
            if (_manager != null)
            {
                //_manager.UnregisterRegion (name);
            }
            else
            {
                _pendingRegions.Remove (name);
            }
        }
    }

}
