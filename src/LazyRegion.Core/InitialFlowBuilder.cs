using Microsoft.Extensions.DependencyInjection;
using System;

namespace LazyRegion.Core;

internal sealed class InitialFlowBuilder : IInitialFlowBuilder
{
    private readonly InitialRegionFlow _flow;

    public InitialFlowBuilder(InitialRegionFlow flow)
    {
        _flow = flow;
    }

    public IInitialFlowBuilder Show(string viewKey)
    {
        _flow.Steps.Add (new FlowStep (viewKey));
        return this;
    }

    public IInitialFlowBuilder Then(string viewKey)
        => Show (viewKey);

    // ✅ Static 조건
    public IInitialFlowBuilder Then(string viewKey, Func<bool> when)
    {
        _flow.Steps.Add (
            new FlowStep (viewKey, _ => when ()));
        return this;
    }

    // ✅ IServiceProvider 직접
    public IInitialFlowBuilder Then(
        string viewKey,
        Func<IServiceProvider, bool> when)
    {
        _flow.Steps.Add (new FlowStep (viewKey, when));
        return this;
    }

    // ✅ DI 기반 (권장)
    public IInitialFlowBuilder Then<TService>(
        string viewKey,
        Func<TService, bool> when)
    {
        _flow.Steps.Add (
            new FlowStep (
                viewKey,
                sp =>
                {
                    var svc = sp.GetRequiredService<TService> ();
                    return when (svc);
                }));
        return this;
    }
}
