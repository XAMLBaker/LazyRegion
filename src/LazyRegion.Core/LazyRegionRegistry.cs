using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public static class LazyRegionRegistry
    {
        private static readonly Dictionary<string, ILazyRegionBase> _regions = new ();
        private static readonly Dictionary<string, TaskCompletionSource<ILazyRegionBase>> _waiters = new ();
        private static readonly Dictionary<string, LoadingRegionBehavior> _behaviors = new ();

        private static RegionLoadingOptions? _options;
        private static ILazyRegionManagerBase? _manager;

        private static readonly HashSet<string> _initialFlowExecuted = new ();
        public static Func<ILazyRegionManager, string, string, Task>? NavigateHandler;
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
        }

        public static void RegisterRegion(string name, ILazyRegionBase region)
        {
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
            if (_manager is not LazyRegionManager mgr)
                return;
            if(flow.InitialViewKey != null)
            {
                await _manager.NavigateAsync (regionName, flow.InitialViewKey);
                // ⭐ UI가 한 번 렌더링될 기회를 줌
                await Task.Yield ();
            }

            // 2️⃣ Step 순회
            foreach (var step in flow.Steps)
            {
                if (step.Condition != null)
                {
                    bool ok;

                    try
                    {
                        ok = await step.Condition (mgr.ServiceProvider); // Task<bool> await
                    }
                    catch
                    {
                        // Condition에서 예외 발생 시 스킵
                        continue;
                    }

                    if (!ok)
                        continue;
                }

                // ⭐ Step Navigate는 안전하게 래퍼 사용
                await NavigateStepAsync (regionName, step.ViewKey);
                break; // 최초 만족 조건만 실행
            }
        }
        // Step Navigate 전용 래퍼
        public static async Task NavigateStepAsync(string regionName, string viewKey)
        {
            if (_manager is not LazyRegionManager mgr)
                return;

            // Step Navigate 전용 UI 안전 호출
            await Task.Yield ();                // Dispatcher에 제어권 반환

            if (NavigateHandler != null)
            {
                // UI 환경: Handler 사용 (WPF/Maui/Avalonia에서 설정됨)
                await NavigateHandler.Invoke (mgr, regionName, viewKey);
            }
            else
            {
                // 유닛테스트: Handler 없으면 직접 실행
                await mgr.NavigateAsync (regionName, viewKey);
            }
        }

        public static async Task<ILazyRegionBase> WaitForRegionAsync(
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
    }

}
