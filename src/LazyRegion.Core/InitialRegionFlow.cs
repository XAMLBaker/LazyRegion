using System;
using System.Collections.Generic;

namespace LazyRegion.Core;

public sealed class InitialRegionFlow
{
    internal readonly List<FlowStep> Steps = new ();
}

public sealed class FlowStep
{
    public string ViewKey { get; }
    public Func<IServiceProvider, bool>? Condition { get; }

    public FlowStep(string viewKey,
                    Func<IServiceProvider, bool>? condition = null)
    {
        ViewKey = viewKey;
        Condition = condition;
    }
}
