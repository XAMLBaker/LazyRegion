using System;
using System.Threading;
using System.Threading.Tasks;

namespace LazyRegion.Core
{
    public class LoadingRegionBehavior
    {
        private readonly SynchronizationContext? _uiContext;
        private readonly string _regionName;
        private readonly ILazyRegionBase _region;
        private readonly RegionLoadingConfig _config;
        private readonly ILazyRegionManagerBase _regionManager;
        private readonly DateTime _regionCreatedTime;
        private CancellationTokenSource? _timeoutCts;

        public LoadingRegionBehavior(
            string regionName,
            ILazyRegionBase region,
            RegionLoadingConfig config,
            ILazyRegionManagerBase regionManager)
        {
            _regionName = regionName;
            _region = region;
            _config = config;
            this._regionManager = regionManager;
            _regionCreatedTime = DateTime.UtcNow;
            if (!string.IsNullOrEmpty (_config.ErrorViewKey))
                _uiContext = SynchronizationContext.Current;
        }

        public void Attach()
        {
            // 1. 즉시 Loading View 표시
            if (!string.IsNullOrEmpty (_config.LoadingViewKey))
            {
                _ = ShowLoadingAsync ();
            }

            // 2. Timeout 타이머 시작
            StartTimeoutTimer ();

            // 3. Region의 View 변경 감지
            // (ILazyRegion 인터페이스에 따라 구현)
        }

        private async Task ShowLoadingAsync()
        {
            // LazyRegionManager를 통해 Loading View 표시
            await _regionManager.NavigateAsync(_regionName, _config.LoadingViewKey!);
        }

        private void StartTimeoutTimer()
        {
            _timeoutCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_config.Timeout, _timeoutCts.Token);

                    // UI 스레드로 전환
                    if (_uiContext != null)
                    {
                        _uiContext.Post(async _ =>
                        {
                            await _regionManager.NavigateAsync(_regionName, _config.ErrorViewKey);
                        }, null);
                    }
                }
                catch (TaskCanceledException)
                {
                    // 정상적으로 Cancel됨
                }
                finally
                {
                    StopTimeoutTimer ();
                }
            });
        }

        public void OnNavigationCompleted(string viewKey)
        {
            // Loading/Error View가 아니면 완료 처리
            if (viewKey != _config.LoadingViewKey &&
                viewKey != _config.ErrorViewKey)
            {
                _ = CompleteAsync ();
            }
        }

        private async Task CompleteAsync()
        {
            // MinDisplayTime 체크
            var elapsed = DateTime.UtcNow - _regionCreatedTime;
            var remaining = _config.MinDisplayTime - elapsed;

            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay (remaining);
            }

            StopTimeoutTimer ();
        }

        /// <summary>
        /// _timeoutCts를 원자적으로 취소 및 해제합니다.
        /// Interlocked.Exchange로 하나의 스레드만 정리를 수행하도록 보장합니다.
        /// </summary>
        private void StopTimeoutTimer()
        {
            var cts = Interlocked.Exchange (ref _timeoutCts, null);
            if (cts == null)
                return;

            cts.Cancel ();
            cts.Dispose ();
        }
    }
}
