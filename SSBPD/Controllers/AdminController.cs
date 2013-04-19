using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.Helper;
using System.Globalization;
using SSBPD.ViewModels;
using System.Net;
namespace SSBPD.Controllers
{
    public class IllegalMergeException : Exception
    {
        public IllegalMergeException(string message)
            : base(message)
        {
        }
    }

    public class AdminController : BaseController
    {
        public class TournamentFileDummy
        {
            public bool Processed { get; set; }
            public DateTime? ProcessedAt { get; set; }
            public DateTime Inserted { get; set; }
            public string OriginalFileName { get; set; }
            public int TournamentFileId { get; set; }
            public Guid TournamentGUID { get; set; }
            public SSBPD.Models.User User { get; set; }

            public TournamentFileDummy(bool processed, DateTime? processedAt, DateTime inserted, string originalFileName, int tournamentFileId, Guid tournamentGUID, User user)
            {
                this.Processed = processed;
                this.ProcessedAt = processedAt;
                this.OriginalFileName = originalFileName;
                this.Inserted = inserted;
                this.TournamentFileId = tournamentFileId;
                this.TournamentGUID = tournamentGUID;
                this.User = user;
                if (User == null)
                {
                    User = new User()
                    {
                        email = "",
                        isAdmin = false,
                        isModerator = false,
                        username = "Unknown",
                        UserID = 0
                    };
                }
            }
        }

        private UserAuthHelper _userAuthHelper;
        public UserAuthHelper userAuthHelper
        {
            get
            {
                if (_userAuthHelper == null)
                {
                    _userAuthHelper = new UserAuthHelper();
                }
                return _userAuthHelper;
            }
            set
            {
                _userAuthHelper = value;
            }
        }
        private ELOProcessor _eloProcessor;
        public ELOProcessor eloProcessor
        {
            get
            {
                if (_eloProcessor == null)
                {
                    _eloProcessor = new ELOProcessor();
                }
                return _eloProcessor;
            }
            set
            {
                _eloProcessor = value;
            }
        }
        private EraserHelper _eraserHelper;
        public EraserHelper eraserHelper
        {
            get
            {
                if (_eraserHelper == null)
                {
                    _eraserHelper = new EraserHelper();
                } return _eraserHelper;
            }
            set
            {
                _eraserHelper = value;
            }
        }
        private bool isAdmin
        {
            get
            {
                return Convert.ToBoolean(Session["userAdmin"]);
            }
        }
        private bool isMod
        {
            get
            {
                return Convert.ToBoolean(Session["userModerator"]);
            }
        }


        public AdminController()
        {
            ViewBag.JavascriptIncludes.Add("~/Scripts/Admin.js");
            ViewBag.CSSIncludes.Add("~/Content/Admin.css");
        }
        [HttpPost]
        public ActionResult ResetAllEloScores()
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }

            eloProcessor.ResetAllEloScores();

            var json = new { response = "The damage is done." };
            return Json(json);
        }
        [HttpPost]
        public ActionResult ResetCharacterStatsCache()
        {
            foreach (Character character in CharacterUtils.Characters)
            {
                Application[character.ToString() + "Stats"] = null;
            }
            var json = new { response = "Cache cleared" };
            return Json(json);
        }

        [HttpGet]
        public ActionResult AssignSets(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }
            Player foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.AllPlayers = from p in db.Players
                                 orderby p.Tag ascending
                                 select p;
            return View(foundPlayer);
        }
        [HttpPost]
        public ActionResult DoAssignSets(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }
            Player foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                return RedirectToAction("Index");
            }
            int fromPlayerId = Convert.ToInt32(Request["fromPlayer"]);
            int fromTournamentId = Convert.ToInt32(Request["fromTournament"]);
            var sets = from s in db.Sets
                       where s.TournamentID == fromTournamentId
                       && (s.WinnerID == fromPlayerId || s.LoserID == fromPlayerId)
                       select s;
            foreach (Set set in sets)
            {
                if (set.WinnerID == fromPlayerId)
                {
                    set.WinnerID = playerId;
                }
                else if (set.LoserID == fromPlayerId)
                {
                    set.LoserID = playerId;
                }
            }
            db.SaveChanges();
            db.Dispose();
            return RedirectToAction("Detail", "Player", new { tag = foundPlayer.Tag });
        }

        [HttpPost]
        public ActionResult DeletePlayer(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                var notAuthorizedJson = new { response = "You are not authorized to delete this player." };
                return Json(notAuthorizedJson);
            }
            Player foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                var redirectJson = new { url = "/player" };
                return Json(redirectJson);
            }
            var sets = from s in db.Sets
                       where s.WinnerID == playerId || s.LoserID == playerId
                       select s;
            if (sets.Count() > 0)
            {
                var hasSetsJson = new { response = "Cannot delete a player with sets" };
                return Json(hasSetsJson);
            }
            db.Players.Remove(foundPlayer);
            db.Database.ExecuteSqlCommand("DELETE FROM PlayerFlags WHERE PlayerId = {0}", playerId);
            db.SaveChanges();
            db.Dispose();
            var json = new { url = "/player" };
            return Json(json);
        }

        [HttpGet]
        public ActionResult CreatePlayer()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DoCreatePlayer()
        {
            if (!(isAdmin || isMod))
            {

                return RedirectToAction("Index");
            }
            string tag = Request["tag"];
            if (string.IsNullOrWhiteSpace(tag))
            {
                ViewBag.Message = "Please specify a tag.";
                return View("CreatePlayer");
            }
            Player foundPlayer = (from p in db.Players where p.Tag.Equals(tag) select p).FirstOrDefault();
            if (foundPlayer != null)
            {
                ViewBag.Message = "Tag already in use.";
                return View("CreatePlayer");
            }
            Player newPlayer = new Player();
            newPlayer.Tag = tag;
            newPlayer.ELO = SSBPD.Helper.ELOProcessor.STARTING_ELO;
            db.Players.Add(newPlayer);
            db.SaveChanges();
            return RedirectToAction("AssignSets", new { playerId = newPlayer.PlayerId });
        }

        public ActionResult GetTournamentSelectForPlayer(int playerId)
        {
            var playerTournamentIds = from s in db.Sets
                                      where s.WinnerID == playerId || s.LoserID == playerId
                                      select s.TournamentID;
            var playerTournaments = from t in db.Tournaments
                                    join i in playerTournamentIds
                                    on t.TournamentID equals i
                                    select t;
            var tournamentsOrdered = playerTournaments.Distinct().OrderBy(t => t.Name).ToList();
            db.Dispose();
            return PartialView(tournamentsOrdered);
        }

        [HttpPost]
        public ActionResult ProcessAllTournaments()
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            log("Start processing all tournaments");
            var tids = (from t in db.Tournaments
                       where t.eloProcessed == false
                       orderby t.Date ascending
                       select t.TournamentID).ToList();
            foreach (var tid in tids)
            {
                try
                {

                    eloProcessor.adjustEloScoresForTournament(tid);
                }
                catch (TournamentLockedException e)
                {
                    log("tournament " + tid + " is locked");
                }
            }
            log("Finished processing all tournaments");
            setLastProcessedDate(DateTime.Now);
            return null;
        }

        [HttpPost]
        public ActionResult ProcessTournamentFile(int tournamentFileId)
        {
            if (!(isAdmin || isMod))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            TioParser tioParser;
            try
            {
                tioParser = new TioParser(tournamentFileId);
            }
            catch (FileAlreadyParsedException e)
            {
                var errorJson = new
                {
                    response = "Already parsed"
                };
                return Json(errorJson);
            }
            catch (Exception)
            {
                var errorJson = new { response = "Something went wrong. Please try again later." };
                return Json(errorJson);
            }
            tioParser.ParseTournament();
            var json = new
            {
                response = "Success!"
            };
            return Json(json);
        }

        [HttpPost]
        public ActionResult ReleaseLock(int tournamentId)
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Tournament tournament = db.Tournaments.Find(tournamentId);
            if (tournament == null)
            {
                db.Dispose();
                var notFoundJson = new { response = "Tournament not found" };
                return Json(notFoundJson);
            }
            tournament.locked = false;
            db.SaveChanges();
            db.Dispose();
            var json = new { response = "tournament unlocked" };
            return Json(json);
        }

        [HttpPost]
        public ActionResult ErasePlayerFlags(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Player player = db.Players.Find(playerId);
            string tag = "[player not found]";
            if (player != null)
            {
                tag = player.Tag;
            }
            db.Database.ExecuteSqlCommand("DELETE FROM PlayerFlags WHERE PlayerID = {0}", playerId);
            var json = new { response = String.Format("All flags for {0} have been deleted", tag) };
            return Json(json);

        }

        [HttpPost]
        public ActionResult ErasePlayerRegionFlags(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Player player = db.Players.Find(playerId);
            string tag = "[player not found]";
            if (player != null)
            {
                tag = player.Tag;
            }
            db.Database.ExecuteSqlCommand("DELETE FROM RegionFlags WHERE PlayerID = {0}", playerId);
            db.Dispose();
            var json = new { response = String.Format("All flags for {0} have been deleted", tag) };
            return Json(json);
        }

        [HttpPost]
        public ActionResult ErasePlayerCharacterFlags(int playerId)
        {
            if (!(isAdmin || isMod))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Player player = db.Players.Find(playerId);
            string tag = "[player not found]";
            if (player != null)
            {
                tag = player.Tag;
            }
            db.Database.ExecuteSqlCommand("DELETE FROM CharacterFlags WHERE PlayerID = {0}", playerId);
            db.Dispose();
            var json = new { response = String.Format("All flags for {0} have been deleted", tag) };
            return Json(json);
        }


        [HttpPost]
        public ActionResult EraseTournament(int tournamentId)
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }

            try
            {
                eraserHelper.EraseTournament(tournamentId);
            }
            catch (TournamentNotFoundException)
            {
                var notFoundJson = new { response = "Tournament not found." };
                return Json(notFoundJson);
            }
            catch (TournamentLockedException)
            {
                var lockedJson = new { response = "Tournament locked." };
                return Json(lockedJson);
            }
            var json = new { response = "Tournament erased." };
            return Json(json);
        }

        [HttpPost]
        public ActionResult EraseTournamentFile(int tournamentFileId)
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }

            var tournamentFile = (from t in db.TournamentFiles
                                  where t.TournamentFileID == tournamentFileId
                                  select t).FirstOrDefault();
            if (tournamentFile == null)
            {
                db.Dispose();
                var notFoundJson = new { response = "File not found." };
                return Json(notFoundJson);
            }

            var tournament = (from t in db.Tournaments
                              where t.TournamentGuid.Equals(tournamentFile.TournamentGuid)
                              select t).FirstOrDefault();

            if (tournament != null)
            {
                var alreadyProcessedJson = new { response = "You cannot delete the file for a tournament that has been processed. Try deleting " + tournament.Name + " first." };
                return Json(alreadyProcessedJson);
            }

            db.TournamentFiles.Remove(tournamentFile);
            db.SaveChanges();
            db.Dispose();

            var json = new { response = "Tournament File removed." };
            return Json(json);
        }

        public ActionResult ProcessEloForTournament(int tournamentId)
        {
            log("trying to process elo for tournament " + tournamentId);
            if (!(isAdmin || isMod))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Tournament tournament = db.Tournaments.Find(tournamentId);
            db.Dispose();
            if (tournament == null)
            {
                log("error finding tournament " + tournamentId);
                var wtfJson = new { response = "Something went wrong. Please try again later." };
                return Json(wtfJson);
            }
            if (tournament.Date < DateTime.Now.AddDays(-7) && !(Request["user"] == "processall"))
            {
                log("Cannot process tournaments more than seven days old");
                var tooOldJson = new { response = "Sorry, this tournament is more than a week old and will have to wait for a full reprocessing." };
                return Json(tooOldJson);
            }
            try
            {
                eloProcessor.adjustEloScoresForTournament(tournamentId);
            }
            catch (TournamentLockedException e)
            {
                var errorJson = new { response = "Tournament locked." };
                return Json(errorJson);
            }
            var json = new { response = "Success!" };
            return Json(json);
        }
        [HttpPost]
        public ActionResult ChangeAdminStatus(int userID)
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            User user = (from u in db.Users
                         where u.UserID == userID
                         select u).FirstOrDefault();
            if (user == null)
            {
                var notfoundJson = new { response = "User not found" };
                return Json(notfoundJson);
            }
            if (user.UserID == Convert.ToInt32(Session["userId"]))
            {
                var wtfJson = new { response = "Are you retarded?" };
                return Json(wtfJson);
            }

            user.isAdmin = Convert.ToBoolean(Request["newStatus"]);
            db.SaveChanges();
            db.Dispose();
            var json = new { response = "Change effected" };
            return Json(json);
        }
        [HttpPost]
        public ActionResult ChangeModeratorStatus(int userID)
        {
            if (!isAdmin)
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            User user = (from u in db.Users
                         where u.UserID == userID
                         select u).FirstOrDefault();
            if (user == null)
            {
                var notfoundJson = new { response = "User not found" };
                return Json(notfoundJson);
            }

            user.isModerator = Convert.ToBoolean(Request["newStatus"]);
            db.SaveChanges();
            db.Dispose();
            var json = new { response = "Change effected" };
            return Json(json);
        }
        public ActionResult Index()
        {
            if (!(isAdmin || isMod))
            {
                return View();
            }
            var files = from tf in db.TournamentFiles
                        select new
                        {
                            FileName = tf.OriginalFileName,
                            Processed = tf.Processed,
                            ProcessedAt = tf.ProcessedAt,
                            Inserted = tf.Inserted,
                            TournamentFileId = tf.TournamentFileID,
                            TournamentGUID = tf.TournamentGuid,
                            UserID = tf.UserID
                        };
            var displayFiles = new List<TournamentFileDummy>();
            foreach (var file in files)
            {
                User user = null;
                if (file.UserID > 0)
                {
                    user = db.Users.Find(file.UserID);
                }
                displayFiles.Add(new TournamentFileDummy(file.Processed, file.ProcessedAt, file.Inserted, file.FileName, file.TournamentFileId, file.TournamentGUID, user));
            }
            ViewBag.Files = displayFiles;
            ViewBag.Users = (from u in db.Users
                             select u).ToList();
            ViewBag.Tournaments = (from t in db.Tournaments
                                   orderby t.Date descending
                                   select t).ToList();
            ViewBag.LastProcessedDate = getLastProcessedDate();
            ViewBag.NumberProcessedTournaments = getNumberProcessedTournaments();

            List<CultureInfo> cultures = new List<CultureInfo>();
            cultures.Add(new CultureInfo("en-us"));
            cultures.Add(new CultureInfo("fr-FR"));
            cultures.Add(new CultureInfo("fi-FI"));
            cultures.Add(new CultureInfo("sv-SE"));
            ViewBag.Cultures = cultures;
            db.Dispose();
            return View();
        }

        public ActionResult MergePlayers()
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }

            var players = (from p in db.Players
                           orderby p.Tag ascending
                           select p).ToList();
            db.Dispose();
            return View(players);
        }

        public ActionResult ViewLogs()
        {
            if (!isAdmin)
            {
                return RedirectToAction("Index");
            }
            var logs = (from l in db.LogMessages
                       orderby l.LogMessageID descending
                       select l).Take(500).ToList();
            db.Dispose();
            return View(logs);
        }

        public ActionResult FlaggedPlayers()
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }

            var playerFlagGroups = from pf in db.PlayerFlags
                                   group pf by pf.PlayerID into grouping
                                   where grouping.Count() > 0
                                   select grouping;
            // PlayerID => ( tag1 => count1, tag2 => count2 )
            var TagFlags = new Dictionary<int, Dictionary<string, int>>();
            foreach (var group in playerFlagGroups)
            {
                TagFlags[group.Key] = new Dictionary<string, int>();
                foreach (var pf in group)
                {
                    TagFlags[group.Key][pf.newTag] = TagFlags[group.Key].ContainsKey(pf.newTag) ? TagFlags[group.Key][pf.newTag] + 1 : 1;
                }
            }

            var playerCharacterGroups = from cf in db.CharacterFlags
                                        where cf.PlayerID != null
                                        group cf by cf.PlayerID into grouping
                                        where grouping.Count() > 0
                                        select grouping;
            var CharacterFlags = new Dictionary<int, Dictionary<Character, int>>();
            foreach (var group in playerCharacterGroups)
            {
                CharacterFlags[group.Key.Value] = new Dictionary<Character, int>();
                foreach (var cf in group)
                {
                    CharacterFlags[group.Key.Value][(Character)cf.CharacterID] = CharacterFlags[group.Key.Value].ContainsKey((Character)cf.CharacterID) ? CharacterFlags[group.Key.Value][(Character)cf.CharacterID] + 1 : 1;
                }
            }

            var RegionFlags = new Dictionary<int, Dictionary<Region, int>>();
            var regionFlagGroups = from rf in db.RegionFlags
                                   group rf by rf.PlayerID into grouping
                                   where grouping.Count() > 0
                                   select grouping;
            foreach (var group in regionFlagGroups)
            {
                RegionFlags[group.Key] = new Dictionary<Region, int>();
                foreach (var pf in group)
                {
                    RegionFlags[group.Key][pf.Region] = RegionFlags[group.Key].ContainsKey(pf.Region) ? RegionFlags[group.Key][pf.Region] + 1 : 1;
                }
            }

            var players = from p in db.Players
                          where TagFlags.Keys.Contains(p.PlayerId) || RegionFlags.Keys.Contains(p.PlayerId) || CharacterFlags.Keys.Contains(p.PlayerId)
                          select p;
            var IdToPlayer = new Dictionary<int, Player>();
            foreach (var player in players)
            {
                IdToPlayer[player.PlayerId] = player;
            }
            foreach (int id in RegionFlags.Keys.Union(TagFlags.Keys).Union(CharacterFlags.Keys).Except(IdToPlayer.Keys))
            {
                IdToPlayer[id] = new Player()
                {
                    Tag = "player not found",
                    PlayerId = id
                };
            }
            db.Dispose();
            FlaggedPlayersViewModel vm = new FlaggedPlayersViewModel(IdToPlayer, TagFlags, RegionFlags, CharacterFlags);
            return View(vm);
        }

        public ActionResult FlaggedSets()
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }


            var flaggedSets = getCharacterFlaggedSets();
            var linkFlags = (from slf in db.SetLinkFlags
                             group slf by slf.SetLinkID into g
                             select g);
            var linkFlaggedSets = new Dictionary<Set, List<FlaggedSetLink>>();
            foreach (var lf in linkFlags)
            {
                int setLinkId = lf.Key;
                SetLink setLink = db.SetLinks.Find(setLinkId);
                Set set = db.Sets.Find(setLink.SetID);
                Player winner = db.Players.Find(set.WinnerID);
                Player loser = db.Players.Find(set.LoserID);
                int count = lf.Count();
                if (!linkFlaggedSets.ContainsKey(set))
                {
                    linkFlaggedSets[set] = new List<FlaggedSetLink>();
                }
                linkFlaggedSets[set].Add(new FlaggedSetLink(setLink, count, winner, loser));
            }
            db.Dispose();
            return View(new FlaggedSetsViewModel(flaggedSets, linkFlaggedSets));
        }

        private List<FlaggedSet> getCharacterFlaggedSets()
        {
            var setFlags = from cf in db.CharacterFlags
                           where cf.SetID.HasValue
                           group cf by cf.SetID.Value into cfGroup
                           select cfGroup;

            var flaggedSets = new List<FlaggedSet>();
            foreach (var cfGroup in setFlags)
            {
                var set = db.Sets.Find(cfGroup.Key);
                var tournamentName = set.Tournament;
                Player winner = db.Players.Find(set.WinnerID);
                Player loser = db.Players.Find(set.LoserID);
                var winnerFlags = new Dictionary<Character, int>();
                var loserFlags = new Dictionary<Character, int>();
                foreach (var flag in cfGroup)
                {
                    if (flag.WinnerFlag.Value)
                    {
                        if (winnerFlags.ContainsKey(flag.Character))
                        {
                            winnerFlags[flag.Character]++;
                        }
                        else
                        {
                            winnerFlags[flag.Character] = 1;
                        }
                    }
                    else
                    {
                        if (loserFlags.ContainsKey(flag.Character))
                        {
                            loserFlags[flag.Character]++;
                        }
                        else
                        {
                            loserFlags[flag.Character] = 1;
                        }
                    }

                }
                flaggedSets.Add(new FlaggedSet(winner, loser, winnerFlags, loserFlags, tournamentName, set.SetID));
            }
            return flaggedSets;
        }


        public ActionResult UploadImages()
        {
            if (!isAdmin)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult DoMergePlayers()
        {
            if (!(isAdmin || isMod))
            {
                return RedirectToAction("Index");
            }

            int fromPlayerId = Convert.ToInt32(Request["fromPlayerId"]);
            int toPlayerId = Convert.ToInt32(Request["toPlayerId"]);

            var players = from p in db.Players
                          orderby p.Tag ascending
                          select p;
            if (fromPlayerId == toPlayerId)
            {
                ViewBag.MergeMessage = "Cannot merge a player into himself.";
                return View("MergePlayers", players);
            }
            var fromPlayer = players.Where(p => p.PlayerId == fromPlayerId).First();
            var toPlayer = players.Where(p => p.PlayerId == toPlayerId).First();

            var sets = from s in db.Sets
                       where s.WinnerID == fromPlayerId || s.LoserID == fromPlayerId
                       select s;

            foreach (Set set in sets)
            {
                if ((set.WinnerID == fromPlayerId && set.LoserID == toPlayerId) || (set.WinnerID == toPlayerId && set.LoserID == fromPlayerId))
                {
                    ViewBag.MergeMessage = "These two players have played each other, cannot merge";
                    db.Dispose();
                    return View("MergePlayers", players);
                }
            }

            foreach (Set set in sets)
            {
                if (set.WinnerID == fromPlayerId)
                {
                    set.WinnerID = toPlayerId;
                }
                else if (set.LoserID == fromPlayerId)
                {
                    set.LoserID = toPlayerId;
                }
            }
            db.Database.ExecuteSqlCommand("DELETE FROM PlayerFlags WHERE PlayerId = " + fromPlayerId);

            db.Players.Remove(fromPlayer);
            db.SaveChanges();
            var viewPlayers = players.Where(p => p.PlayerId != fromPlayerId).ToList();
            db.Dispose();
            ViewBag.MergeMessage = "Merge complete!";
            return View("MergePlayers", viewPlayers);
        }

        public ActionResult Login()
        {
            string username = Request["username"];
            string password = Request["password"];
            User user = userAuthHelper.getUser(username, password);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            Session["userId"] = user.UserID;
            if (user.isAdmin)
            {
                Session["userAdmin"] = true;
            }
            if (user.isModerator)
            {
                Session["userModerator"] = true;
            }
            db.Dispose();
            return RedirectToAction("Index");
        }

        private DateTime getLastProcessedDate()
        {
            if (Application["lastProcessedDate"] == null)
            {
                return (DateTime)Application["startup"];
            }
            return (DateTime)Application["lastProcessedDate"];
        }

        private void setLastProcessedDate(DateTime time)
        {
            Application["lastProcessedDate"] = time;
        }


        private int getNumberProcessedTournaments()
        {
            DateTime lastProcessed = getLastProcessedDate();
            return db.TournamentFiles.Where(t => t.ProcessedAt > lastProcessed).Count();
        }

    }
}
