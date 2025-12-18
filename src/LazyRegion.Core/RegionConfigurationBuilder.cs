using LazyRegion.Core.Interfaces;

namespace LazyRegion.Core
{
    public class RegionConfigurationBuilder : IRegionConfigurationBuilder
    {
        private readonly RegionLoadingOptions _options;

        public RegionConfigurationBuilder(RegionLoadingOptions options)
        {
            _options = options;
        }

        public IRegionBehaviorBuilder ForRegion(string regionName)
        {
            if (!_options.Regions.ContainsKey (regionName))
                _options.Regions[regionName] = new RegionLoadingConfig ();

            return new RegionBehaviorBuilder (_options.Regions[regionName]);
        }
    }
}
