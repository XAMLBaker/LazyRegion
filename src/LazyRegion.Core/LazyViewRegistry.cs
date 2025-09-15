using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public class LazyViewRegistry
    {
        private readonly List<(Type ViewType, string Key, ServiceLifetime Lifetime)> _views = new ();

        public void Add<T>(string key, ServiceLifetime lifetime) where T : class, new()
            => _views.Add ((typeof (T), key, lifetime));
        public LazyViewRegistry()
        {
            
        }
        public void Initialize(ILazyRegionManager regionManager)
        {
            foreach (var v in _views)
            {
                var method = typeof (ILazyRegionManager)
                    .GetMethod (nameof (ILazyRegionManager.RegisterView))
                    ?.MakeGenericMethod (v.ViewType);

                method?.Invoke (regionManager, new object[] { v.Key, v.Lifetime });
            }
        }
    }
}
