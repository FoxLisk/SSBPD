using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class SetWithElo
    {
        public bool isDraw;
        public double winnerElo;
        public double loserElo;
        public int winnerChar;
        public int loserChar;

        public SetWithElo(double winnerElo, double loserElo, bool isDraw, int winnerChar, int loserChar)
        {
            this.winnerElo = winnerElo;
            this.loserElo = loserElo;
            this.isDraw = isDraw;
            this.winnerChar = winnerChar;
            this.loserChar = loserChar;
        }

    }
    public class StatsController : BaseController
    {

        public ActionResult Index()
        {/*
            var sets = from s in db.Sets
                       where s.WinnerCharacterID != null && s.LoserCharacterID != null
                       group s by s.TournamentID into g
                       select g;
            var scores = from e in db.EloScores
                         group e by e.TournamentID into g
                         select g;


            var displaySets = new List<SetWithElo>();
            foreach (var set in sets)
            {
                int tid = set.TournamentID;

                var winnerEloScore = scores.First(g => g.Key == tid).First(e => e.PlayerID == set.WinnerID).ELO;
                var loserEloScore = scores.First(g => g.Key == tid).First(e => e.PlayerID == set.LoserID).ELO;
                if (winnerEloScore > 1300 && loserEloScore > 1300)
                {
                    displaySets.Add(new SetWithElo(winnerEloScore, loserEloScore, set.isDraw, set.WinnerCharacterID.Value, set.LoserCharacterID.Value));
                }
            }
          * */

            return null;
        }

    }
}
