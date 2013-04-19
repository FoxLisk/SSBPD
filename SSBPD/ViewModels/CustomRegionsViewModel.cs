using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;
namespace SSBPD.ViewModels
{
    public class CustomRegionsViewModel
    {
        public List<CustomRegion> customRegions;
        public CustomRegionsViewModel(List<CustomRegion> regions)
        {
            this.customRegions = regions;
        }
    }
}