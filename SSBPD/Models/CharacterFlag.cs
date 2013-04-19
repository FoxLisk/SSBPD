using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class CharacterFlag
    {
        public int CharacterFlagID { get; set; }
        public int? PlayerID { get; set; }
        public int? SetID { get; set; }
        public bool? WinnerFlag { get; set; }

        [Required]
        public int UserID { get; set; }
        [Required]
        public int CharacterID { get; set; }
        [NotMapped]
        public Character Character
        {
            get
            {
                if (Enum.IsDefined(typeof(Character), CharacterID))
                {
                    return (Character)CharacterID;
                }
                else
                {
                    return Character.NoCharacter;
                }
            }
        }


        public CharacterFlag() { }

        public CharacterFlag(int userId, int setId, bool winnerFlag, int characterId)
        {
            this.UserID = userId;
            this.CharacterID = characterId;
            this.SetID = setId;
            this.WinnerFlag = winnerFlag;
        }

        public CharacterFlag(int userId, int playerId, int characterValue)
        {
            this.UserID = userId;
            this.PlayerID = playerId;
            this.CharacterID = characterValue;
        }
    }
}