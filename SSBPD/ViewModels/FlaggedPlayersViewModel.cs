using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class FlaggedPlayersViewModel
    {
        public Dictionary<int, Player> IdToPlayer;

        // PlayerID => ( tag1 => count1, tag2 => count2 )
        public Dictionary<int, Dictionary<string, int>> TagFlags;

        // PlayerID => ( Region1 => count1 ...)
        public Dictionary<int, Dictionary<Region, int>> RegionFlags;

        public Dictionary<int, Dictionary<Character, int>> CharacterFlags;

        public FlaggedPlayersViewModel(Dictionary<int, Player> idToPlayer, Dictionary<int, Dictionary<string, int>> tagFlags, Dictionary<int, Dictionary<Region, int>> regionFlags, Dictionary<int, Dictionary<Character, int>> characterFlags)
        {
            this.IdToPlayer = idToPlayer;
            this.TagFlags = tagFlags;
            this.RegionFlags = regionFlags;
            this.CharacterFlags = characterFlags;
        }
    }
}
