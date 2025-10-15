using System;

namespace LazyRegion.Core;

public class RegionLoadingConfig
{
    public string? LoadingViewKey { get; set; }
    public string? ErrorViewKey { get; set; }
    public TimeSpan MinDisplayTime { get; set; } = TimeSpan.Zero;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds (30);
}
