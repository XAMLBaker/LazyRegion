using LazyRegion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegionManager_InitialFlow_Then.ViewModels
{
    internal class MainViewModel
    {
        private readonly ILazyRegionManager lazyRegionManager;

        public MainViewModel(ILazyRegionManager lazyRegionManager)
        {
            this.lazyRegionManager = lazyRegionManager;
        }
    }
}
