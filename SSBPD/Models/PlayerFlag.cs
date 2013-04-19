using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class PlayerFlag
    {
        
        public int PlayerFlagID { get; set; }
        [Required]
        public int PlayerID { get; set; }
        public int? toPlayerID { get; set; }

        [Required]
        public int userID { get; set; }

        [Required]
        public string newTag { get; set; }
    }
}