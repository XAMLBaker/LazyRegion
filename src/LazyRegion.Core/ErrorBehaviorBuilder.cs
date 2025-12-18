using System;

namespace LazyRegion.Core
{
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
