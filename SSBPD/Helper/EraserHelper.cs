using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.Helper
{
    public class TournamentNotFoundException : Exception { }
    public class FileNotFoundException : Exception { }
    public class EraserHelper
    {
        private SSBPDContext _db;
        private SSBPDContext db
        {
            get
            {
                if (_db == null)
                {
                    _db = new SSBPDContext();
                }
                return _db;
            }
            set
            {
                _db = value;
            }
        }
        public void EraseTournament(int tournamentId)
        {
            var tournament = (from t in db.Tournaments
                              where t.TournamentID == tournamentId
                              select t).FirstOrDefault();
            if (tournament == null)
            {
                throw new TournamentNotFoundException();
            }
            if (tournament.locked)
            {
                throw new TournamentLockedException();
            }
            tournament.locked = true;
            db.SaveChanges();

            var sets = from s in db.Sets
                       select s;
            var playerIds = new HashSet<int>();

            foreach (Set set in sets)
            {
                if (set.TournamentID == tournamentId)
                {
                    playerIds.Add(set.WinnerID);
                    playerIds.Add(set.LoserID);
                    db.Sets.Remove(set);
                }
            }

            var players = (from p in db.Players
                           where playerIds.Contains(p.PlayerId)
                           select p).ToList();

            foreach (Player player in players)
            {
                var playerSets = (from s in sets
                                  where (s.TournamentID != tournamentId && (s.WinnerID == player.PlayerId || s.LoserID == player.PlayerId))
                                  select s).Count(s => true);
                if (playerSets == 0)
                {
                    db.Players.Remove(player);
                }
            }

            var tournamentFile = (from t in db.TournamentFiles
                                  where t.TournamentGuid.Equals(tournament.TournamentGuid)
                                  select t).FirstOrDefault();
            tournamentFile.Processed = false;
            tournamentFile.ProcessedAt = null;
            db.Tournaments.Remove(tournament);
            db.SaveChanges();
        }

        public void EraseTournamentFile(int tournamentFileId)
        {
            var tournamentFile = (from t in db.TournamentFiles
                                  where t.TournamentFileID == tournamentFileId
                                  select t).FirstOrDefault();
            if (tournamentFile == null)
            {
                throw new FileNotFoundException();
            }
            var tournament = (from t in db.Tournaments
                              where t.TournamentGuid.Equals(tournamentFile.TournamentGuid)
                              select t).FirstOrDefault();
            if (tournament != null)
            {
                throw new FileAlreadyParsedException();
            }

            db.TournamentFiles.Remove(tournamentFile);
            db.SaveChanges();
        }
    }
}