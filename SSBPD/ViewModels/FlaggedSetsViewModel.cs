using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class FlaggedSet
    {
        public int setId;
        public Player winner;
        public Player loser;
        public Dictionary<Character, int> winnerFlags;
        public Dictionary<Character, int> loserFlags;
        public Tournament tournament;

        public FlaggedSet(Player winner, Player loser, Dictionary<Character, int> winnerFlags, Dictionary<Character, int> loserFlags, Tournament tournament, int setId)
        {
            this.winner = winner;
            this.loser = loser;
            this.winnerFlags = winnerFlags;
            this.loserFlags = loserFlags;
            this.tournament = tournament;
            this.setId = setId;
        }
    }
    public class FlaggedSetLink
    {
        public SetLink setLink;
        public int numFlags;
        public Player winner;
        public Player loser;
        public FlaggedSetLink(SetLink setLink, int numFlags, Player winner, Player loser)
        {
            this.setLink = setLink;
            this.numFlags = numFlags;
            this.winner = winner;
            this.loser = loser;
        }
    }

    public class FlaggedSetsViewModel
    {
        public IEnumerable<FlaggedSet> FlaggedSets;
        public Dictionary<Set, List<FlaggedSetLink>> linkFlaggedSets;

        public FlaggedSetsViewModel(IEnumerable<FlaggedSet> sets, Dictionary<Set, List<FlaggedSetLink>> linkFlaggedSets)
        {
            this.FlaggedSets = sets;
            this.linkFlaggedSets = linkFlaggedSets;
        }

    }
}