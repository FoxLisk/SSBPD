using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class VersusSet
    {
        public bool regionOneWon;
        public bool isDraw;
        public Player regionOnePlayer;
        public Player regionTwoPlayer;
        public Tournament tournament;
        public DateTime datePlayed;


        public VersusSet(Set set, IEnumerable<Player> regionOnePlayers, IEnumerable<Player> regionTwoPlayers)
        {
            regionOnePlayer = regionOnePlayers.Where(p => p.PlayerId == set.WinnerID || p.PlayerId == set.LoserID).First();
            regionTwoPlayer = regionTwoPlayers.Where(p => p.PlayerId == set.WinnerID || p.PlayerId == set.LoserID).First();
            if (set.isDraw)
            {
                this.isDraw = true;
                regionOneWon = false;
            }
            else
            {
                regionOneWon = regionOnePlayer.PlayerId == set.WinnerID;
            }
            this.tournament = set.Tournament;
            this.datePlayed = set.DatePlayed;
        }
    }
    public class VersusRegionViewModel
    {
        public IEnumerable<Region> regionOneList;
        public IEnumerable<Region> regionTwoList;
        public int regionOneWins;
        public int regionTwoWins;
        public IEnumerable<Player> regionOnePlayers;
        public IEnumerable<Player> regionTwoPlayers;
        public int regionOneAverageELO;
        public int regionTwoAverageELO;
        public IEnumerable<VersusSet> sets;
        public int draws;

        public VersusRegionViewModel(IEnumerable<Region> regionOneList, IEnumerable<Region> regionTwoList, int regionOneWins, int regionTwoWins,
            int draws, IEnumerable<Player> regionOnePlayers, IEnumerable<Player> regionTwoPlayers, IEnumerable<Set> sets)
        {
            this.regionOneList = regionOneList;
            this.regionTwoList = regionTwoList;
            this.regionOneWins = regionOneWins;
            this.regionTwoWins = regionTwoWins;
            this.draws = draws;
            this.regionOnePlayers = regionOnePlayers;
            this.regionTwoPlayers = regionTwoPlayers;

            this.regionOneAverageELO = 0;
            this.regionTwoAverageELO = 0;
            if (regionOnePlayers.Count() > 0)
            {
                regionOneAverageELO = Convert.ToInt32(regionOnePlayers.Average(p => p.ELO));
            }
            if (regionTwoPlayers.Count() > 0)
            {
                regionTwoAverageELO = Convert.ToInt32(regionTwoPlayers.Average(p => p.ELO));
            }

            this.sets = sets.Select(s => new VersusSet(s, regionOnePlayers, regionTwoPlayers));
        }
    }
}