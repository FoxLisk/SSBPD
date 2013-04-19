using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class SetLinkFlag
    {
        public int SetLinkFlagID { get; set; }
        [Required]
        public int SetLinkID { get; set; }
        [Required]
        public int userID { get; set; }
    }
}