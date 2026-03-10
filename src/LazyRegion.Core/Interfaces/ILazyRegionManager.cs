using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface ILazyRegionManagerBase
{
    Task NavigateAsync(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, TimeSpan? regionWaitTimeout = null);

    Task NavigateAsync(string regionName, string viewKey, LazyNavigationParameters parameters, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, LazyNavigationParameters parameters, TimeSpan? regionWaitTimeout = null);

    // Animation override overloads
    Task NavigateAsync(string regionName, string viewKey, TransitionAnimation animation, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, TransitionAnimation animation, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync(string regionName, string viewKey, LazyNavigationParameters parameters, TransitionAnimation animation, TimeSpan? regionWaitTimeout = null);
    Task NavigateAsync<T>(string regionName, string viewKey, LazyNavigationParameters parameters, TransitionAnimation animation, TimeSpan? regionWaitTimeout = null);

    // GoBack
    bool CanGoBack(string regionName);
    Task<bool> GoBackAsync(string regionName, TimeSpan? timeout = null);

    // Region group navigation
    Task NavigateGroupAsync(params (string regionName, string viewKey, TransitionAnimation? animation)[] navigations);

    Task AddItemAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null);
    Task AddItemsAsync(
            string regionName,
            string[] viewKeys,
            TimeSpan? timeout = null);
    Task RemoveItemAsync(
            string regionName,
            string viewKey,
            TimeSpan? timeout = null);
    Task ClearItemsAsync(
            string regionName,
            TimeSpan? timeout = null);
    Task InsertItemAsync(
            string regionName,
            int index,
            string viewKey,
            TimeSpan? timeout = null);
}
public interface ILazyRegionManager : ILazyRegionManagerBase
{
    void RegisterView(
        string viewKey,
        ServiceLifetime lifetime,
        LazyFactory factory);

    object GetView(string viewKey);
}
