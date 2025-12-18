using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LazyRegion.Core;

public interface IInitialFlowBuilder
{
    IInitialFlowBuilder Show(string viewKey);

    IInitialFlowBuilder Then(string viewKey);

    IInitialFlowBuilder Then(string viewKey, Func<Task<bool>> when);

    IInitialFlowBuilder Then(
        string viewKey,
         Func<IServiceProvider, Task<bool>>? when = null);

    IInitialFlowBuilder Then<TService>(
        string viewKey,
         Func<TService, Task<bool>> when);
}
