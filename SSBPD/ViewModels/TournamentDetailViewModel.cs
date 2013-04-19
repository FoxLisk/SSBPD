using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;
using SSBPD.Controllers;

namespace SSBPD.ViewModels
{
    public class TournamentDetailViewModel
    {

        public IEnumerable<IGrouping<string, Set>> Brackets;
        public Dictionary<string, List<Pool>> PoolsEvents;
        public Dictionary<int, Player> IdToPlayer;
        public Tournament tournament;
        public int entrants;

        public TournamentDetailViewModel(IEnumerable<IGrouping<string, Set>> brackets, Dictionary<string, List<Pool>> poolsEvents, Dictionary<int, Player> idToPlayer, Tournament tournament)
        {
            this.Brackets = brackets;
            this.PoolsEvents = poolsEvents;
            this.IdToPlayer = idToPlayer;
            this.tournament = tournament;
            var bracketPlayers = brackets.SelectMany(s => s.ToList()).SelectMany(s => new List<int>() {s.WinnerID, s.LoserID});
            var poolsPlayers = poolsEvents.Values.SelectMany(pool => pool.SelectMany(p => p.playerIDs));
            entrants = bracketPlayers.Union(poolsPlayers).Count();
        }
    }
}