using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using SSBPD.Helper;

namespace SSBPD.Models
{
    public class Tournament
    {
        public int TournamentID { get; set; }
        [Required]
        public String Name { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public bool locked { get; set; }
        [Required]
        public bool eloProcessed { get; set; }
        [Required]
        public Guid TournamentGuid { get; set; }

        [NotMapped]
        public string URL
        {
            get
            {
                return Name.IndexOfAny(URLHelper.illegalChars) >= 0 ? TournamentID.ToString() : Name;
            }
        }
    }
}