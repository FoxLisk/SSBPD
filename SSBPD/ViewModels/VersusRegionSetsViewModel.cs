using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{

    public class VersusRegionSetsViewModel
    {
        public Region regionOne;
        public Region regionTwo;
        public int regionOneWins;
        public int regionTwoWins;
        public int regionOneAverageELO;
        public int regionTwoAverageELO;
        public IEnumerable<VersusSet> sets;


        public VersusRegionSetsViewModel(Region regionOne, Region regionTwo, int regionOneWins, int regionTwoWins,
            IEnumerable<Player> regionOnePlayers, IEnumerable<Player> regionTwoPlayers, IEnumerable<Set> sets)
        {
            this.regionOne = regionOne;
            this.regionTwo = regionTwo;
            this.regionOneWins = regionOneWins;
            this.regionTwoWins = regionTwoWins;
            this.regionOneAverageELO = Convert.ToInt32(regionOnePlayers.Average(p => p.ELO));
            this.regionTwoAverageELO = Convert.ToInt32(regionTwoPlayers.Average(p => p.ELO));
        }
    }
}