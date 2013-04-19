using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class RegionViewPlayer
    {
        SSBPDContext db = new SSBPDContext();
        public Player player;
        public bool Active { get; set; }
        

        public RegionViewPlayer(Player p)
        {
            player = p;
            var sets = from s in db.Sets
                          where s.WinnerID == p.PlayerId || s.LoserID == p.PlayerId
                          orderby s.DatePlayed descending
                          select s;
            var lastset = sets.FirstOrDefault();
            if (lastset == null)
            {
                Active = false;
            }
            else
            {
                Active = lastset.DatePlayed > DateTime.Now.AddYears(-1);
            }
        }
    }

    public class RegionGroupViewModel
    {
        public IEnumerable<Region> Regions { get; set; }
        public int AverageElo { get; set; }
        public int AverageActiveELO { get; set; }
        public List<RegionViewPlayer> Players { get; set; }
        public string QueryString
        {
            get
            {
                var regionStrs = Regions.Select(r => ((int)r).ToString());
                return string.Join(",", regionStrs);
            }
        }

        public RegionGroupViewModel(IEnumerable<Region> regions, IEnumerable<Player> players)
        {
            Regions = regions;
            Players = new List<RegionViewPlayer>();
            foreach (Player player in players)
            {
                Players.Add(new RegionViewPlayer(player));
            }
            if (players.Count() > 0)
            {
                AverageElo = Convert.ToInt32(Players.Average(p => p.player.ELO));
                AverageActiveELO = Convert.ToInt32(Players.Where(p => p.Active).Average(p => p.player.ELO));
            }
            else
            {
                AverageActiveELO = 0;
                AverageElo = 0;
            }
        }
    }
}