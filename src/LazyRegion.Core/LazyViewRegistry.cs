using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public class LazyViewRegistry
    {
        private readonly List<(Type ViewType, string Key, ServiceLifetime Lifetime)> _views = new ();

        public void Add<T>(string key, ServiceLifetime lifetime) where T : class, new()
        {
            _views.Add ((typeof (T), key, lifetime));
            _registrations.Add ((rm) => rm.RegisterView<T> (key, lifetime)); // ✅ 미리 delegate 저장
        }

        public void Initialize(ILazyRegionManager regionManager)
        {
            foreach (var action in _registrations)
            {
                action (regionManager); // ✅ 리플렉션 안 씀
            }
        }

        private readonly List<Action<ILazyRegionManager>> _registrations = new ();
    }
}
