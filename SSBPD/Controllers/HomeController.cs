using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using System.Data.Entity.Migrations;
using SSBPD.Helper;
using System.Diagnostics;
using System.Configuration;
using SSBPD.ViewModels;
using System.Text.RegularExpressions;

namespace SSBPD.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController()
        {
            ViewBag.CSSIncludes.Add("~/Content/Home.css");
        }

        public ActionResult Index()
        {
            IEnumerable<Tournament> tournaments;
            using (var db = new SSBPDContext())
            {
                tournaments = (from t in db.Tournaments
                                  orderby t.Date descending
                                  select t).Take(10).ToList();
                var players = from p in db.Players
                              orderby p.ELO descending
                              select p;
                ViewBag.Players = players.Take(10).ToList();
            }
            return View(tournaments.Take(10));
        }

        public ActionResult About()
        {
            Player player = (from p in db.Players
                             orderby p.ELO descending
                             select p).First();
            int opponentId = db.Database.SqlQuery<IdHolder>("SELECT LoserID AS ID, COUNT(LoserID) AS cnt FROM Sets WHERE WinnerID = {0} GROUP BY LoserID  ORDER BY cnt DESC", player.PlayerId).First().ID;
            Player opponent = db.Players.Find(opponentId);
            int tournamentId = db.Database.SqlQuery<IdHolder>("SELECT TournamentID AS ID, COUNT(TournamentID) AS cnt FROM Sets GROUP BY TournamentID ORDER BY cnt DESC").First().ID;
            Tournament tournament = db.Tournaments.Find(tournamentId);
            var vm = new AboutViewModel(player, opponent, tournament);
            return View(vm);
        }

        class IdHolder
        {
            public int ID { get; set; }
            public int cnt { get; set; }
        }

    }
}
