using LazyRegion.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LazyRegion.Core
{
    public static class LazyExtensions
    {
        private static readonly List<Action<LazyViewRegistry>> _pending = new ();

        public static IServiceCollection AddLazyView<T>(
            this IServiceCollection services,
            string key,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : class, new()
        {
            services.Add (new ServiceDescriptor (typeof (T), typeof (T), lifetime));
            _pending.Add (r => r.Add<T> (key, lifetime));
            return services;
        }

        public static IServiceCollection AddLazyView(
            this IServiceCollection services,
            string key,
            LazyFactory factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _pending.Add (r => r.Add (key, factory, lifetime));
            return services;
        }

        public static IServiceCollection ConfigureRegions(
            this IServiceCollection services,
            Action<IRegionConfigurationBuilder> configure)
        {
            var options = new RegionLoadingOptions ();
            configure (new RegionConfigurationBuilder (options));
            services.AddSingleton (options);
            return services;
        }

        public static IServiceCollection UseLazyRegionCore(
            this IServiceCollection services)
        {
            services.AddSingleton (sp =>
            {
                var reg = new LazyViewRegistry ();
                foreach (var a in _pending)
                    a (reg);
                _pending.Clear ();
                return reg;
            });

            services.AddSingleton<ILazyRegionManager> (sp =>
                new LazyRegionManager (
                    sp,
                    sp.GetRequiredService<LazyViewRegistry> (),
                    sp.GetService<RegionLoadingOptions> ()));

            return services;
        }
    }

}
