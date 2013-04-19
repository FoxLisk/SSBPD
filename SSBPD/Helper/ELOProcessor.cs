using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;
using System.Diagnostics;

namespace SSBPD.Helper
{
    public class TournamentLockedException : Exception { }
    public class ELOProcessor
    {
        private IRatingsCalculator _calculator;
        public IRatingsCalculator calculator
        {
            get
            {
                if (_calculator == null)
                {
                    _calculator = new EloCalculator();
                }
                return _calculator;
            }
            set
            {
                _calculator = value;
            }
        }
        public const int STARTING_ELO = 1200;
        private void log(string message)
        {
            using (var sdb = new SSBPDContext())
            {
                sdb.LogMessages.Add(new LogMessage()
                {
                    Message = message
                });
                sdb.SaveChanges();
            }
        }

        private Tournament findAndLockTournament(int tournamentId)
        {
            Tournament tournament = null;
            using (var db = new SSBPDContext())
            {
                tournament = db.Tournaments.Find(tournamentId);
                if (tournament.locked)
                {
                    throw new TournamentLockedException();
                }
                tournament.locked = true;
                db.SaveChanges();
                db.Dispose();
            }
            return tournament;
        }

        public void adjustEloScoresForTournament(int tournamentId)
        {
            log("adjusting scores for tournament id = " + tournamentId);
            Tournament tournament = findAndLockTournament(tournamentId);
            //log("after tournament locked and disposal: " + GC.GetTotalMemory(false));

            var db = new SSBPDContext();
            IEnumerable<RatingsSet> sets;
            sets = from s in db.Sets
                   where s.TournamentID == tournamentId
                   select new RatingsSet() { WinnerID = s.WinnerID, LoserID = s.LoserID, isDraw = s.isDraw };

            HashSet<int> playerIDs = new HashSet<int>();
            foreach (RatingsSet set in sets)
            {
                playerIDs.Add(set.WinnerID);
                playerIDs.Add(set.LoserID);
            }
            var players = (from p in db.Players
                           where playerIDs.Contains(p.PlayerId)
                           select new RatingsPlayer() { PlayerId = p.PlayerId, ELO = p.ELO }).ToList();

            //  log("after getting players: " + GC.GetTotalMemory(false));
            playerIDs = null;
            GC.Collect();
            //  log("after cleaning up playerIds: " + GC.GetTotalMemory(false));
            var playerToEloChange = new Dictionary<int, double>();
            foreach (var player in players)
            {
                //    log("adding to playerToEloChange, memory: " + GC.GetTotalMemory(false));
                playerToEloChange[player.PlayerId] = calculator.calculateRatingChangeForPlayer(player, sets, players);
            }

            //   log("after calculating upcoming elo changes: " + GC.GetTotalMemory(false));
            int i = 0;
            using (var tdb = new SSBPDContext())
            {
               string updateSql = "";
                foreach (var player in players)
                {
                    var newElo = player.ELO + playerToEloChange[player.PlayerId];
                    if (newElo < 100)
                    {
                        newElo = 100;
                    }
                    //updateSql += String.Format("UPDATE Players SET ELO = {0} WHERE PlayerId = {1};", newElo, player.PlayerId);
                    tdb.Database.ExecuteSqlCommand("UPDATE Players SET ELO = {0} WHERE PlayerId = {1}", newElo, player.PlayerId);
                }
                //tdb.Database.ExecuteSqlCommand(updateSql);
            }
            //   log("after updating elos: " + GC.GetTotalMemory(false));

            tournament = db.Tournaments.Find(tournamentId);
            foreach (var player in players)
            {
                player.ELO += playerToEloChange[player.PlayerId];
            }
            playerToEloChange = null;
            saveEloScores(players, tournament);
      //      log("after saving elos: " + GC.GetTotalMemory(false));
            tournament.locked = false;
            tournament.eloProcessed = true;
            db.SaveChanges();
            db.Dispose();
        //    log("Exiting method for tournament id= " + tournament.TournamentID + ": " + GC.GetTotalMemory(false));
            GC.Collect();
        }

        private void saveEloScores(IEnumerable<RatingsPlayer> players, Tournament tournament)
        {
            int i = 0;
            using (var db = new SSBPDContext())
            {
                foreach (var player in players)
                {
                    if (i % 20 == 0)
                    {
                        GC.Collect();
                    }
              //      log("in save elo scores: " + GC.GetTotalMemory(false));

                    i++;
                    db.Database.ExecuteSqlCommand("INSERT INTO EloScores (ELO, Date, TournamentID, PlayerID) VALUES ({0}, {1}, {2}, {3})",
                        player.ELO, tournament.Date, tournament.TournamentID, player.PlayerId);
                }
            }
        }
        /**
         * this should only be used followed immediately by a call to "process all tournaments"
         */
        public void ResetAllEloScores()
        {
            using (var db = new SSBPDContext())
            {
                var allPlayers = from p in db.Players
                                 select p;
                foreach (Player player in allPlayers)
                {
                    player.ELO = STARTING_ELO;
                }
                var allTournaments = from t in db.Tournaments
                                     select t;
                foreach (Tournament tournament in allTournaments)
                {
                    tournament.eloProcessed = false;
                }
                db.Database.ExecuteSqlCommand("DELETE FROM EloScores");
                db.SaveChanges();
            }
        }
        /*
        internal void ProcessAllTournaments()
        {
            IEnumerable<int> allTournaments = null;
            using (var db = new SSBPDContext())
            {
                allTournaments = (from t in db.Tournaments
                                  orderby t.Date ascending
                                  where !t.eloProcessed
                                  select t.TournamentID).ToList();
            }
            foreach (int tid in allTournaments)
            {

                try
                {
                    adjustEloScoresForTournament(tid);
                    using (var blah = new SSBPDContext())
                    {
                        blah.LogMessages.Add(new LogMessage()
                        {
                            Message = "current memory = " + Process.GetCurrentProcess().PrivateMemorySize64 + "    gc size = " + GC.GetTotalMemory(false)
                        });
                        blah.SaveChanges();
                    }
                }
                catch (TournamentLockedException e)
                {
                    continue;
                }
            }
        }
         */
    }
}