using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
        _flow.InitialViewKey = viewKey;
        return this;
    }

    public IInitialFlowBuilder Then(string viewKey)
    {
        _flow.Steps.Add (new FlowStep (viewKey, null));
        return this;
    }

    // ✅ Static 조건
    public IInitialFlowBuilder Then(string viewKey,
        Func<Task<bool>> when)
    {
        _flow.Steps.Add (
          new FlowStep (
              viewKey,
              _ => when ()
          ));
        return this;
    }

    // ✅ IServiceProvider 직접
    public IInitialFlowBuilder Then(
        string viewKey,
        Func<IServiceProvider, Task<bool>>? when = null)
    {
        _flow.Steps.Add (new FlowStep (viewKey, when));
        return this;
    }

    // ✅ DI 기반 (권장)
    public IInitialFlowBuilder Then<TService>(
        string viewKey,
            Func<TService, Task<bool>> when)
    {
        _flow.Steps.Add (
            new FlowStep (
                viewKey,
                async sp =>
                {
                    var svc = sp.GetRequiredService<TService> ();
                    return await when (svc);
                }
            ));
        return this;
    }
}
