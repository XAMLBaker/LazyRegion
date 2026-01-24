using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public sealed class LazyViewRegistry
    {
        private readonly List<Action<ILazyRegionManager>> _registrations = new ();

        public void Add<T>(string key, ServiceLifetime lifetime)
            where T : class, new()
        {
            _registrations.Add (rm =>
                rm.RegisterView (key, lifetime, sp =>
                    sp.GetService<T> () ??
                    ActivatorUtilities.CreateInstance<T> (sp)));
        }

        public void Add(string key, LazyFactory factory,
                        ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _registrations.Add (rm =>
                rm.RegisterView (key, lifetime, factory));
        }

        internal void Initialize(ILazyRegionManager manager)
        {
            foreach (var r in _registrations)
                r (manager);
        }
    }
}
