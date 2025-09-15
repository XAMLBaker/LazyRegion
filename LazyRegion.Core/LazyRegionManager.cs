namespace LazyRegion.Core;

public static class LazyRegionManager
{
    private static LazyRegionService _instance;

    public static ILazyRegionManager Instance
    {
        get
        {
            if (_instance == null)
                throw new System.Exception ("BUg!!!!");
            return _instance;
        }
    }

    public static LazyRegionService Service => (LazyRegionService)Instance;

    /// <summary>
    /// DI Container로부터 LazyRegionService를 초기화합니다.
    /// </summary>
    public static void Initialize(LazyRegionService service)
    {
        _instance = service;
    }
}