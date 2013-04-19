using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class RegionIndexViewModel
    {
        public List<CustomRegion> customRegions;
        public RegionIndexViewModel(List<CustomRegion> customRegions)
        {
            this.customRegions = customRegions;
            this.customRegions.AddRange(CustomRegion.DefaultRegions);
        }
    }
}