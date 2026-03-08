namespace LazyRegion.Core;

public sealed class LazyNavigationContext
{
    public string RegionName { get; }
    public string ViewKey { get; }
    public LazyNavigationParameters Parameters { get; }

    public LazyNavigationContext(
        string regionName,
        string viewKey,
        LazyNavigationParameters? parameters = null)
    {
        RegionName = regionName;
        ViewKey = viewKey;
        Parameters = parameters ?? new LazyNavigationParameters();
    }
}
