using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class TournamentIndexViewModel
    {
        public IEnumerable<Tournament> tournaments;
        public Dictionary<int, int> tournamentToEntrants;
        public TournamentIndexViewModel(IEnumerable<Tournament> tournaments, Dictionary<int, int> tournamentToEntrants)
        {
            this.tournaments = tournaments;
            this.tournamentToEntrants = tournamentToEntrants;
        }
    }
}