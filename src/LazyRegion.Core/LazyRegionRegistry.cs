using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public static class LazyRegionRegistry
    {
        private static readonly Dictionary<string, ILazyRegionBase> _regions = new ();
        private static readonly Dictionary<string, List<TaskCompletionSource<ILazyRegionBase>>> _waiters = new ();
        private static readonly Dictionary<string, LoadingRegionBehavior> _behaviors = new ();

        private static RegionLoadingOptions? _options;
        private static ILazyRegionManagerBase? _manager;

        private static readonly HashSet<string> _initialFlowExecuted = new ();
        public static Func<ILazyRegionManager, string, string, Task>? NavigateHandler;

        /// <summary>
        /// Static 경로에서 RegisterRegion 최초 호출 시 매니저를 초기화하는 콜백입니다.
        /// UseWpf() / UseMaui() 등에서 설정되며, 한 번 실행 후 즉시 해제됩니다.
        /// </summary>
        public static Action? OnManagerRequested { get; set; }

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
            // Static 경로: 창이 열리기 직전 최초 1회만 초기화
            if (OnManagerRequested != null)
            {
                OnManagerRequested.Invoke ();
                OnManagerRequested = null;
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

            if (_waiters.TryGetValue (name, out var waitList))
            {
                foreach (var tcs in waitList)
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
            if (string.IsNullOrWhiteSpace (name))
                throw new ArgumentException ("Region name cannot be null or empty.", nameof (name));

            if (_regions.TryGetValue (name, out var region))
                return region;

            // 호출자별 독립 TCS 생성
            var tcs = new TaskCompletionSource<ILazyRegionBase> ();

            if (!_waiters.TryGetValue (name, out var list))
            {
                list = new List<TaskCompletionSource<ILazyRegionBase>> ();
                _waiters[name] = list;
            }
            list.Add (tcs);

            using var cts = new CancellationTokenSource ();

            var delay = timeout.HasValue
                ? Task.Delay (timeout.Value, cts.Token)
                : Task.Delay (Timeout.Infinite, cts.Token);

            var done = await Task.WhenAny (tcs.Task, delay);

            if (done == tcs.Task)
            {
                cts.Cancel ();
                return await tcs.Task;
            }

            // 타임아웃: 자신의 TCS만 제거, 리스트가 비면 키 자체를 정리
            if (_waiters.TryGetValue (name, out var timeoutList))
            {
                timeoutList.Remove (tcs);
                if (timeoutList.Count == 0)
                    _waiters.Remove (name);
            }

            throw new TimeoutException (
                $"Region '{name}' was not registered within the specified timeout.");
        }

        public static void NotifyNavigationCompleted(string region, string viewKey)
        {
            if (_behaviors.TryGetValue (region, out var b))
                b.OnNavigationCompleted (viewKey);
        }
    }

}
