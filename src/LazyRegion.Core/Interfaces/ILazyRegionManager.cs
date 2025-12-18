using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyRegionManagerBase
{
    Task NavigateAsync(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
}
public interface ILazyRegionManager : ILazyRegionManagerBase
{
    void RegisterView(
        string viewKey,
        ServiceLifetime lifetime,
        LazyFactory factory);
}
