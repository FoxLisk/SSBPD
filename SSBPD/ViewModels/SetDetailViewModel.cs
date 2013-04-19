using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class SetDetailViewModel
    {
        public Set set;
        public Tournament tournament;
        public Player winner;
        public Player loser;
        public IEnumerable<SetLink> videos;
        public string message;
        public SetDetailViewModel(Set set, Player winner, Player loser, IEnumerable<SetLink> videos, string message)
        {
            this.set = set;
            this.tournament = set.Tournament;
            this.winner = winner;
            this.loser = loser;
            this.videos = videos;
            this.message = message;
        }
    }
}