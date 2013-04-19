using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSBPD.Models
{
    public enum Character
    {
        Bowser = 1,
        CaptainFalcon,
        DonkeyKong,
        DrMario,
        Falco,
        Fox,
        Ganondorf,
        IceClimbers,
        Jigglypuff,
        Kirby,
        Link,
        Luigi,
        Mario,
        Marth,
        Mewtwo,
        MrGameAndWatch,
        Ness,
        Peach,
        Pichu,
        Pikachu,
        Roy,
        Samus,
        Sheik,
        Yoshi,
        YoungLink,
        Zelda,

        NoCharacter = 99999
    }

    public static class CharacterUtils
    {
        public static IEnumerable<Character> Characters
        {
            get
            {
                return Enum.GetValues(typeof(Character)).Cast<SSBPD.Models.Character>();
            }
        }
        public static string DisplayString(this Character character) {
            switch (character) {
                case Character.CaptainFalcon:
                    return "Captain Falcon";
                case Character.DonkeyKong:
                    return "Donkey Kong";
                case Character.DrMario:
                    return "Dr. Mario";
                case Character.IceClimbers:
                    return "Ice Climbers";
                case Character.MrGameAndWatch:
                    return "Mr. Game And Watch";
                case Character.NoCharacter:
                    return "Unknown character";
                case Character.YoungLink:
                    return "Young Link";
                default:
                    return character.ToString();
            
        }}
        public static string ImgTag(this Character character)
        {
            return String.Format("<img src=\"/Image/Icons/{0}.png\" title=\"{1}\" />", character.ToString(), character.DisplayString());
        }
    }
}