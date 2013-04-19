using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Controllers;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class PlayerDetailViewModel
    {
        public int wins;
        public int losses;
        public int draws;
        public double ratio;
        public List<BiasedSet> sets;
        public string eloData;
        public Player player;
        public IEnumerable<Player> allPlayers;

        public PlayerDetailViewModel(Player player, int wins, int losses, int draws, List<BiasedSet> sets, string eloData, IEnumerable<Player> allPlayers)
        {
            this.player = player;
            this.wins = wins;
            this.losses = losses;
            this.draws = draws;
            this.ratio = 100 * ((wins * 1.0 + draws * 0.5) / (wins + losses + draws) * 1.0);
            this.sets = sets;
            this.eloData = eloData;
            this.allPlayers = allPlayers;
        }
    }
}