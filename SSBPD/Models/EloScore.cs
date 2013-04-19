using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class EloScore
    {
        public int EloScoreID { get; set; }
        [Required]
        public int PlayerID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public double ELO { get; set; }
        /**
         * <summary>The tournament at which the player's score moved to this score</summary>
         */
        [Required]
        public int TournamentID { get; set; }

    }
}