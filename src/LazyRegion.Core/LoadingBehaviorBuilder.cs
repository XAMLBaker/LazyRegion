using System;

namespace LazyRegion.Core
{
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
}
