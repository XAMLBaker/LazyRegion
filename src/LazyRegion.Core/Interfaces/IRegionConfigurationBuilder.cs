using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public interface IRegionConfigurationBuilder
    {
        IRegionBehaviorBuilder ForRegion(string regionName);
    }

    public interface IRegionBehaviorBuilder
    {
        IRegionBehaviorBuilder WithLoadingBehavior(Action<ILoadingBehaviorBuilder> configure);
    }

    public interface ILoadingBehaviorBuilder
    {
        ILoadingBehaviorBuilder Loading(string viewKey);
        ILoadingBehaviorBuilder MinDisplayTime(TimeSpan time);
        IErrorBehaviorBuilder Error(string viewKey);
    }

    public interface IErrorBehaviorBuilder
    {
        IErrorBehaviorBuilder Timeout(TimeSpan time);
    }
    public class RegionConfigurationBuilder : IRegionConfigurationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<string, RegionLoadingConfig> _regionConfigs = new ();

        public RegionConfigurationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IRegionBehaviorBuilder ForRegion(string regionName)
        {
            if (!_regionConfigs.ContainsKey (regionName))
            {
                _regionConfigs[regionName] = new RegionLoadingConfig ();
            }

            return new RegionBehaviorBuilder (_regionConfigs[regionName]);
        }

        public Dictionary<string, RegionLoadingConfig> Build() => _regionConfigs;
    }

    public class RegionBehaviorBuilder : IRegionBehaviorBuilder
    {
        private readonly RegionLoadingConfig _config;

        public RegionBehaviorBuilder(RegionLoadingConfig config)
        {
            _config = config;
        }

        public IRegionBehaviorBuilder WithLoadingBehavior(Action<ILoadingBehaviorBuilder> configure)
        {
            var builder = new LoadingBehaviorBuilder (_config);
            configure (builder);
            return this;
        }
    }

    public class LoadingBehaviorBuilder : ILoadingBehaviorBuilder
    {
        private readonly RegionLoadingConfig _config;

        public LoadingBehaviorBuilder(RegionLoadingConfig config)
        {
            _config = config;
        }

        public ILoadingBehaviorBuilder Loading(string viewKey)
        {
            _config.LoadingViewKey = viewKey;
            return this;
        }

        public ILoadingBehaviorBuilder MinDisplayTime(TimeSpan time)
        {
            _config.MinDisplayTime = time;
            return this;
        }

        public IErrorBehaviorBuilder Error(string viewKey)
        {
            _config.ErrorViewKey = viewKey;
            return new ErrorBehaviorBuilder (_config);
        }
    }

    public class ErrorBehaviorBuilder : IErrorBehaviorBuilder
    {
        private readonly RegionLoadingConfig _config;

        public ErrorBehaviorBuilder(RegionLoadingConfig config)
        {
            _config = config;
        }

        public IErrorBehaviorBuilder Timeout(TimeSpan time)
        {
            _config.Timeout = time;
            return this;
        }
    }
}
