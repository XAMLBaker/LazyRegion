using Microsoft.Extensions.DependencyInjection;
using System;

namespace LazyRegion.Core
{
    // 새로운 빌더 클래스
    public class LazyRegionBuilder
    {
        private readonly IServiceCollection _services;

        public LazyRegionBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public LazyRegionBuilder Register<T>(
            string key,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : class, new()
        {
            _services.AddLazyView<T> (key, lifetime);
            return this;
        }

        public LazyRegionBuilder Register(
            string key,
            LazyFactory factory)
        {
            _services.AddLazyView (key, factory, ServiceLifetime.Singleton);
            return this;
        }

        public LazyRegionBuilder ConfigureRegions(
            Action<IRegionConfigurationBuilder> config)
        {
            _services.ConfigureRegions (config);
            return this;
        }
    }

}
