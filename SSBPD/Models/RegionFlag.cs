using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class RegionFlag
    {
        public RegionFlag(int playerId, int userId, int regionValue)
        {
            this.RegionValue = regionValue;
            this.PlayerID = playerId;
            this.userID = userId;
        }
        public RegionFlag() { }
        
        public int RegionFlagID { get; set; }
        [Required]
        public int PlayerID { get; set; }
        [Required]
        public int userID { get; set; }
        
        [Required]
        public int RegionValue { get; set; }

        public Region Region
        {
            get
            {
                if (Enum.IsDefined(typeof(Region), RegionValue))
                {
                    return (Region)RegionValue;
                }
                else
                {
                    return Region.NoRegion;
                }
            }
            set { RegionValue = (int)value; }
        }
    }
}