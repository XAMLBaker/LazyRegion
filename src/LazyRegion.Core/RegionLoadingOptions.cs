using System.Collections.Generic;

namespace LazyRegion.Core
{
    public sealed class RegionLoadingOptions
    {
        internal Dictionary<string, RegionLoadingConfig> Regions { get; } = new ();

        public bool TryGet(string name, out RegionLoadingConfig cfg)
            => Regions.TryGetValue (name, out cfg);
    }
}
