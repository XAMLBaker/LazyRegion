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
        private static readonly Dictionary<string, TaskCompletionSource<ILazyRegion>> _regionWaiters = new ();

        public static void RegisterRegion(string regionName, ILazyRegion region)
        {
            _regions[regionName] = region;

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
    }
}
