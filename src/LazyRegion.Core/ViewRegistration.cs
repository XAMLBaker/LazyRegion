using Microsoft.Extensions.DependencyInjection;
using System;

namespace LazyRegion.Core
{
    internal sealed class ViewRegistration
    {
        public string ViewKey { get; set; } = null!;
        public Type? ViewType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public LazyFactory Factory { get; set; } = null!;
    }
}
