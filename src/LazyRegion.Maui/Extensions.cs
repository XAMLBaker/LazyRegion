﻿using LazyRegion.Core;

namespace LazyRegion.Maui;

public static class Extensions
{
    public static MauiAppBuilder UseLazyRegion(this MauiAppBuilder builder,
                                               Action<IServiceCollection>? configure = null)
    {
        var configurator = builder.Services.UseLazyRegion ();
        configure (builder.Services);
        return builder;
    }
}
