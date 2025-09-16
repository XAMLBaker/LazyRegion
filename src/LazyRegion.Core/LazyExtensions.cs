using Microsoft.Extensions.DependencyInjection;

namespace LazyRegion.Core;

public static class LazyExtensions
{
    public static IServiceCollection UseLazyRegion(this IServiceCollection services)
    {
        services.AddSingleton<LazyViewRegistry> ();
        services.AddSingleton<ILazyRegionManager, LazyRegionService> ();
        return services;
    }

    public static IServiceCollection AddLazyView<T>(this IServiceCollection services, 
                                                    string viewKey, 
                                                    ServiceLifetime lifetime = ServiceLifetime.Singleton)
                                           where T : class, new()
    {
        // DI Container에 뷰 등록
        services.Add (new ServiceDescriptor (typeof (T), typeof (T), lifetime));


        // 2. LazyViewRegistry에 기록
        services.AddSingleton<IStartupAction> (sp =>
        {
            var registry = sp.GetRequiredService<LazyViewRegistry> ();
            registry.Add<T> (viewKey, lifetime);
            return new StartupAction (() => { }); // 아무 것도 안 하는 더미
        });
        return services;
    }
}
