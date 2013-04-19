using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class CustomRegion
    {
        [Required]
        public int CustomRegionID { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int UserID { get; set; }

        public CustomRegion(string name, List<Region> regions, int userId)
        {
            this.Name = name;
            if (userId <= 0)
            {
                throw new Exception("Cannot create a custom region disassociated from a user");
            }
            this.UserID = userId;
            if (regions.Count == 0)
            {
                Value = "";
                return;
            }
            Value = "" + (int)regions[0];
            foreach (Region region in regions.Skip(1))
            {
                Value += "," + (int)region;
            }
        }
        public CustomRegion() { }

        public List<Region> getRegions()
        {
            var regions = new List<Region>();
            var regionIds = Value.Split(',').Select(s => int.Parse(s));
            foreach (var regionId in regionIds)
            {
                regions.Add((Region)regionId);
            }
            return regions;
        }

        public static List<CustomRegion> DefaultRegions
        {
            get
            {
                List<CustomRegion> defaultRegions = new List<CustomRegion>();
                var USARegions = new List<Region>() {
                    Region.Alaska, Region.Arizona, Region.Arkansas, Region.California, Region.Colorado,
                    Region.Connecticut, Region.Delaware, Region.DistrictOfColumbia, Region.Florida, Region.Georgia,
                    Region.Guam, Region.Hawaii, Region.Idaho, Region.Illinois, Region.Indiana,
                    Region.Iowa, Region.Kansas, Region.Kentucky, Region.Louisiana, Region.Maine,
                    Region.Maryland, Region.Massachusetts, Region.Michigan, Region.Minnesota, Region.Mississippi,
                    Region.Missouri, Region.Montana, Region.Nebraska, Region.Nevada, Region.NewHampshire,
                    Region.NewJersey, Region.NewMexico, Region.NewYork, Region.NorthCarolina, Region.NorthDakota,
                    Region.Ohio, Region.Oklahoma, Region.Oregon, Region.Pennsylvania, Region.RhodeIsland,
                    Region.SouthCarolina, Region.SouthDakota, Region.Tennessee, Region.Texas, Region.Utah,
                    Region.Vermont, Region.Virginia, Region.VirginIslands, Region.Washington, Region.WestVirginia,
                    Region.Wisconsin, Region.Wyoming
                };
                var EuropeRegions = new List<Region>() {
                    Region.UK, Region.Sweden, Region.Germany, Region.France, Region.Japan,
                    Region.Italy, Region.Netherlands, Region.Spain, Region.Finland, Region.Austria,
                    Region.Belgium, Region.Denmark, Region.Norway, Region.Switzerland, Region.Luxembourg
                };
                defaultRegions.Add(new CustomRegion("United States", USARegions,1 ));
                defaultRegions.Add(new CustomRegion("Europe", EuropeRegions,1));
                return defaultRegions;
            }
        }
    }
}