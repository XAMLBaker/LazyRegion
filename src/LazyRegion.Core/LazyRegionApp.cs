using LazyRegion.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public sealed class LazyRegionApp
    {
        private static readonly Lazy<LazyRegionApp> _instance =
            new Lazy<LazyRegionApp> (() => new LazyRegionApp ());

        public static LazyRegionApp Default => _instance.Value;

        private readonly object _lock = new object ();
        private readonly List<Action<IServiceCollection>> _viewRegistrations = new ();
        private Action<IRegionConfigurationBuilder>? _regionConfigure;
        private ILazyRegionManager? _manager;
        private bool _isInitialized;

        private LazyRegionApp() { }

        public LazyRegionApp Register<T>(
            string key,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : class, new()
        {
            lock (_lock)
            {
                ThrowIfInitialized ();
                _viewRegistrations.Add (s => s.AddLazyView<T> (key, lifetime));
            }
            return this;
        }

        public LazyRegionApp Register(
            string key,
            LazyFactory factory,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            lock (_lock)
            {
                ThrowIfInitialized ();
                _viewRegistrations.Add (s => s.AddLazyView (key, factory, lifetime));
            }
            return this;
        }

        public LazyRegionApp Configure(
            Action<IRegionConfigurationBuilder> configure)
        {
            lock (_lock)
            {
                ThrowIfInitialized ();
                _regionConfigure = configure;
            }
            return this;
        }

        public ILazyRegionManager RegionManager
        {
            get
            {
                EnsureInitialized ();
                return _manager!;
            }
        }

        internal static void SetManager(ILazyRegionManager manager)
        {
            var app = Default;
            lock (app._lock)
            {
                app._manager = manager;
                app._isInitialized = true;
            }
        }

        internal static bool HasPendingRegistrations
        {
            get
            {
                var app = Default;
                lock (app._lock)
                {
                    return app._viewRegistrations.Count > 0 && !app._isInitialized;
                }
            }
        }

        internal static void ApplyTo(IServiceCollection services)
        {
            var app = Default;
            lock (app._lock)
            {
                foreach (var reg in app._viewRegistrations)
                    reg (services);

                if (app._regionConfigure != null)
                    services.ConfigureRegions (app._regionConfigure);
            }
        }

        private void EnsureInitialized()
        {
            if (_isInitialized && _manager != null)
                return;

            lock (_lock)
            {
                if (_isInitialized && _manager != null)
                    return;

                if (_viewRegistrations.Count == 0)
                    throw new InvalidOperationException (
                        "LazyRegionApp is not initialized. " +
                        "Call LazyRegionApp.Default.Register<T>() first, " +
                        "or use the DI path with UseLazyRegion().");

                var services = new ServiceCollection ();

                foreach (var reg in _viewRegistrations)
                    reg (services);

                if (_regionConfigure != null)
                    services.ConfigureRegions (_regionConfigure);

                services.UseLazyRegionCore ();

                var sp = services.BuildServiceProvider ();
                _manager = sp.GetRequiredService<ILazyRegionManager> ();
                _isInitialized = true;
            }
        }

        private void ThrowIfInitialized()
        {
            if (_isInitialized)
                throw new InvalidOperationException (
                    "Cannot register views after LazyRegionApp has been initialized. " +
                    "Call Register() before accessing RegionManager.");
        }
    }
}
