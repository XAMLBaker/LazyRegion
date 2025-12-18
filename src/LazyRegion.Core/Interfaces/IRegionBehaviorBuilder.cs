using System;

namespace LazyRegion.Core.Interfaces
{
    public interface IRegionBehaviorBuilder
    {
        IRegionBehaviorBuilder WithLoadingBehavior(Action<ILoadingBehaviorBuilder> state);
        IRegionBehaviorBuilder WithInitialFlow(Action<IInitialFlowBuilder> flow);
    }
}
