using LazyRegion.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace LazyRegion.Core
{
    public interface IRegionConfigurationBuilder
    {
        IRegionBehaviorBuilder ForRegion(string regionName);
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
}
