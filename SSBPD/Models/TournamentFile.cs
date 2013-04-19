using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class TournamentFile
    {
        public int TournamentFileID { get; set; }

        [Required]
        [MaxLength]
        public string XML { get; set; }

        [Required]
        public string OriginalFileName { get; set; }

        [Required]
        public bool Processed { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [Required]
        public DateTime Inserted { get; set; }

        [Required]
        public Guid TournamentGuid { get; set; }

        [Required]
        public int UserID { get; set; }

    }
}