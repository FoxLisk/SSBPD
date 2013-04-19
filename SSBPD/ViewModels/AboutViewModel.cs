using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class AboutViewModel
    {
        public Player player;
        public Player opponent;
        public Tournament tournament;

        public AboutViewModel(Player player, Player opponent, Tournament tournament)
        {
            this.tournament = tournament;
            this.player = player;
            this.opponent = opponent;
        }

    }
}
