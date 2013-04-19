using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using SSBPD.Helper;

namespace SSBPD.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        public int? RegionValue { get; set; }
        public int? CharacterMainID { get; set; }

        public Character CharacterMain
        {
            get
            {
                if (CharacterMainID.HasValue && Enum.IsDefined(typeof(Character), CharacterMainID))
                {
                    return (Character)CharacterMainID;
                }
                else
                {
                    return Character.NoCharacter;
                }
            }
            set { CharacterMainID = (int)value; }

        }
        public Region Region
        {
            get
            {
                if (RegionValue.HasValue && Enum.IsDefined(typeof(Region), RegionValue))
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

        public String infoHTML(bool includeChar, bool includeRegion)
        {
            var html = String.Format("<a href=\"/player/{0}\">{1}</a>", URL, Tag);
            if (includeChar)
            {
                html += " " + CharacterMain.ImgTag();
            }
            html += " - " + Convert.ToInt32(ELO);
            if (includeRegion)
            {
                html += " " + Region.ImgTag();
            }
            return html;
        }


        [Required]
        public String Tag { get; set; }

        [Required]
        public double ELO { get; set; }

        [NotMapped]
        public string URL
        {
            get
            {
                System.Console.WriteLine(Tag);
                if (Tag.IndexOfAny(URLHelper.illegalChars) >= 0)
                {
                    return PlayerId.ToString();
                }
                int dead;
                bool isNum = int.TryParse(Tag, out dead);
                if (isNum)
                {
                    return PlayerId.ToString();
                }
                return Tag;
            }
        }

    }
}