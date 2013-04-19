using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class SetLink
    {
        [Required]
        public int SetLinkID { get; set; }
        [Required]
        public int SetID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string URL { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public bool Deleted { get; set; }
    }
}