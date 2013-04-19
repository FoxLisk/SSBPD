using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class Set
    {
       private SSBPDContext db = new SSBPDContext();
        public int SetID { get; set; }

        [Required]
        public int WinnerID { get; set; }

        [Required]
        public int LoserID { get; set; }

        [Required]
        public int TournamentID { get; set; }

        public virtual Tournament Tournament { get; set; }
        [Required]
        public string BracketName { get; set; }

        [Required]
        public bool isDraw { get; set; }

        public int? WinnerCharacterID { get; set; }
        public int? LoserCharacterID { get; set; }

        public Character WinnerCharacter
        {
            get
            {
                if (WinnerCharacterID.HasValue && Enum.IsDefined(typeof(Character), WinnerCharacterID))
                {
                    return (Character)WinnerCharacterID;
                }
                else
                {
                    return Character.NoCharacter;
                }
            }
            set { WinnerCharacterID = (int)value; }

        }

        public Character LoserCharacter
        {
            get
            {
                if (LoserCharacterID.HasValue && Enum.IsDefined(typeof(Character), LoserCharacterID))
                {
                    return (Character)LoserCharacterID;
                }
                else
                {
                    return Character.NoCharacter;
                }
            }
            set { LoserCharacterID = (int)value; }

        }


        //for bracket matches
        public int? Round { get; set; }
        public bool? IsWinners { get; set; }
        
        [Required]
        public bool isPool { get; set; }
        //for pools matches
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? PoolNum { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime DatePlayed { get; set; }

    }
}