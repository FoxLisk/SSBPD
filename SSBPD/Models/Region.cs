using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace SSBPD.Models
{
    public enum Region
    {
        Australia = 6,
        Canada,
        UK = 9,
        Sweden,
        Germany,
        France,
        Japan,
        Italy,
        Netherlands,
        Spain,
        Finland,
        Alabama,
        Alaska,
        Arizona,
        Arkansas,
        California,
        Colorado,
        Connecticut,
        Delaware,
        DistrictOfColumbia,
        Florida,
        Georgia,
        Guam,
        Hawaii,
        Idaho,
        Illinois,
        Indiana,
        Iowa,
        Kansas,
        Kentucky,
        Louisiana,
        Maine,
        Maryland,
        Massachusetts,
        Michigan,
        Minnesota,
        Mississippi,
        Missouri,
        Montana,
        Nebraska,
        Nevada,
        NewHampshire,
        NewJersey,
        NewMexico,
        NewYork,
        NorthCarolina,
        NorthDakota,
        Ohio,
        Oklahoma,
        Oregon,
        Pennsylvania,
        RhodeIsland,
        SouthCarolina,
        SouthDakota,
        Tennessee,
        Texas,
        Utah,
        Vermont,
        Virginia,
        VirginIslands,
        Washington,
        WestVirginia,
        Wisconsin,
        Wyoming,
        Austria,
        Belgium,
        Denmark,
        Israel,
        Norway,
        Switzerland,
        Luxembourg,
        Ecuador,
        Chile,
        Mexico,
        Brazil,
        Venezuela,
        Colombia,
        Bahamas,



        NoRegion = 999999
    }

    public static class RegionUtils
    {
        public static IEnumerable<Region> Regions
        {
            get
            {
                return Enum.GetValues(typeof(Region)).Cast<SSBPD.Models.Region>();
            }

        }
        public static string DisplayString(this Region region)
        {
            switch (region)
            {
                case Region.DistrictOfColumbia:
                    return "District of Columbia";
                case Region.NewHampshire:
                    return "New Hampshire";
                case Region.NewJersey:
                    return "New Jersey";
                case Region.NewMexico:
                    return "New Mexico";
                case Region.NewYork:
                    return "New York";
                case Region.NorthCarolina:
                    return "North Carolina";
                case Region.NorthDakota:
                    return "North Dakota";
                case Region.RhodeIsland:
                    return "Rhode Island";
                case Region.SouthCarolina:
                    return "South Carolina";
                case Region.SouthDakota:
                    return "South Dakota";
                case Region.WestVirginia:
                    return "West Virginia";
                case Region.NoRegion:
                    return "No region defined";
                default:
                    return region.ToString();

            }

        }
        public static string CountryCode(this Region region)
        {
            switch (region)
            {
                case Region.Australia:
                    return "au";
                case Region.Canada:
                    return "ca";
                case Region.Chile:
                    return "cl";
                case Region.UK:
                    return "uk";
                case Region.Sweden:
                    return "se";
                case Region.Germany:
                    return "de";
                case Region.France:
                    return "fr";
                case Region.Italy:
                    return "it";
                case Region.Japan:
                    return "jp";
                case Region.Netherlands:
                    return "nl";
                case Region.Spain:
                    return "es";
                case Region.Finland:
                    return "fi";
                case Region.Alabama:
                    return "us";
                case Region.Alaska:
                    return "us";
                case Region.Arizona:
                    return "us";
                case Region.Arkansas:
                    return "us";
                case Region.California:
                    return "us";
                case Region.Colorado:
                    return "us";
                case Region.Connecticut:
                    return "us";
                case Region.Delaware:
                    return "us";
                case Region.DistrictOfColumbia:
                    return "us";
                case Region.Florida:
                    return "us";
                case Region.Georgia:
                    return "us";
                case Region.Guam:
                    return "us";
                case Region.Hawaii:
                    return "us";
                case Region.Idaho:
                    return "us";
                case Region.Illinois:
                    return "us";
                case Region.Indiana:
                    return "us";
                case Region.Iowa:
                    return "us";
                case Region.Kansas:
                    return "us";
                case Region.Kentucky:
                    return "us";
                case Region.Louisiana:
                    return "us";
                case Region.Maine:
                    return "us";
                case Region.Maryland:
                    return "us";
                case Region.Massachusetts:
                    return "us";
                case Region.Michigan:
                    return "us";
                case Region.Minnesota:
                    return "us";
                case Region.Mississippi:
                    return "us";
                case Region.Missouri:
                    return "us";
                case Region.Montana:
                    return "us";
                case Region.Nebraska:
                    return "us";
                case Region.Nevada:
                    return "us";
                case Region.NewHampshire:
                    return "us";
                case Region.NewJersey:
                    return "us";
                case Region.NewMexico:
                    return "us";
                case Region.NewYork:
                    return "us";
                case Region.NorthCarolina:
                    return "us";
                case Region.NorthDakota:
                    return "us";
                case Region.Ohio:
                    return "us";
                case Region.Oklahoma:
                    return "us";
                case Region.Oregon:
                    return "us";
                case Region.Pennsylvania:
                    return "us";
                case Region.RhodeIsland:
                    return "us";
                case Region.SouthCarolina:
                    return "us";
                case Region.SouthDakota:
                    return "us";
                case Region.Tennessee:
                    return "us";
                case Region.Texas:
                    return "us";
                case Region.Utah:
                    return "us";
                case Region.Vermont:
                    return "us";
                case Region.Virginia:
                    return "us";
                case Region.Washington:
                    return "us";
                case Region.WestVirginia:
                    return "us";
                case Region.Wisconsin:
                    return "us";
                case Region.Wyoming:
                    return "us";
                case Region.Austria:
                    return "at";
                case Region.Belgium:
                    return "be";
                case Region.Denmark:
                    return "dk";
                case Region.Israel:
                    return "il";
                case Region.Norway:
                    return "no";
                case Region.Ecuador:
                    return "ec";
                case Region.Luxembourg:
                    return "lu";
                case Region.Switzerland:
                    return "ch";
                case Region.Mexico:
                    return "mx";
                case Region.Brazil:
                    return "br";
                case Region.Venezuela:
                    return "ve";
                case Region.Colombia:
                    return "co";
                case Region.Bahamas:
                    return "bs";
                default:
                    return "qu";
            }
        }
        public static string ImgTag(this Region region)
        {
            return String.Format("<img src=\"/Image/Flags/{0}.png\" title=\"{1}\" />", region.CountryCode(), region.DisplayString());

        }
    }
}