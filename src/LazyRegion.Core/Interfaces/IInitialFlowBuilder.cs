using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace LazyRegion.Core;

public interface IInitialFlowBuilder
{
    IInitialFlowBuilder Show(string viewKey);

    IInitialFlowBuilder Then(string viewKey);

    IInitialFlowBuilder Then(string viewKey, Func<bool> when);

    IInitialFlowBuilder Then(
        string viewKey,
        Func<IServiceProvider, bool> when);

    IInitialFlowBuilder Then<TService>(
        string viewKey,
        Func<TService, bool> when);
}
