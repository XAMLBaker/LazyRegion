using LazyRegion.Core.Interfaces;
using System;

namespace LazyRegion.Core
{
    public class RegionBehaviorBuilder : IRegionBehaviorBuilder
    {
        private readonly RegionLoadingConfig _config;

        public RegionBehaviorBuilder(RegionLoadingConfig config)
        {
            _config = config;
        }

        public IRegionBehaviorBuilder WithLoadingBehavior(Action<ILoadingBehaviorBuilder> state)
        {
            var builder = new LoadingBehaviorBuilder (_config);
            state (builder);
            return this;
        }
        public IRegionBehaviorBuilder WithInitialFlow(Action<IInitialFlowBuilder> flow)
        {
            var _flow = new InitialRegionFlow ();
            var builder = new InitialFlowBuilder (_flow);

            flow (builder);

            _config.InitialFlow = _flow;
            return this;
        }
    }
}
