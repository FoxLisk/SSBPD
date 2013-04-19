using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.Helper;
using System.Web.Script.Serialization;
using SSBPD.ViewModels;
using System.Diagnostics;

namespace SSBPD.Controllers
{
    public class BiasedSet : IEquatable<BiasedSet>
    {
        public string setMessage;
        public int setID;
        public string tournamentName;
        public int tournamentId;
        public DateTime date;
        public string tournamentURL;
        public Player player;
        public Player opponent;
        public bool isDraw;
        public bool won = false;
        public bool hasVideo;

        public BiasedSet(Set set, int playerID, IEnumerable<Player> allPlayers, Tournament tournament, bool hasVideo)
        {
            if (set.WinnerID != playerID && set.LoserID != playerID)
            {
                return;
            }
            if (set.isDraw)
            {
                setMessage = "Draw with";
                isDraw = true;
            }
            else
            {
                won = (set.WinnerID == playerID);
                isDraw = false;
                setMessage = won ? "Win over" : "Loss to";
            }
            int opponentID = set.WinnerID == playerID ? set.LoserID : set.WinnerID;
            opponent = allPlayers.First(p => p.PlayerId == opponentID);
            setID = set.SetID;
            tournamentName = tournament.Name;
            this.tournamentId = set.TournamentID;
            this.date = set.DatePlayed;
            this.hasVideo = hasVideo;
            tournamentURL = tournament.URL;
        }
        public bool Equals(BiasedSet other)
        {
            return (this.setID == other.setID);
        }
        public override int GetHashCode()
        {
            return this.setID.GetHashCode();
        }
    }
    public class PlayerController : BaseController
    {
        private HashSet<Player> allPlayersSet;
        public PlayerController()
            : base()
        {
            ViewBag.JavascriptIncludes.Add("~/Scripts/Player.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/highcharts/highcharts.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/Player-Highcharts.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/FancyboxUtils.js");
            ViewBag.CSSIncludes.Add("~/Content/Player.css");
            ViewBag.CSSIncludes.Add("~/Content/jquery.fancybox-1.3.4.css");
            ViewBag.JavascriptIncludes.Add("~/Scripts/jQuery/jquery.fancybox-1.3.4.js");
        }

        public ActionResult Detail(string tag)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string logmessage = stopwatch.ElapsedMilliseconds + "| Starting detail for player " + tag + "\n";
            //var allPlayers = from p in db.Players orderby p.Tag select p;
            tag = HttpUtility.UrlDecode(tag);
            bool noRewrite = Convert.ToBoolean(Request["noRewrite"]);
            int? id = null;
            try
            {
                id = Convert.ToInt32(tag);
            }
            catch (FormatException e)
            {
            }
            Player player;
            if (id == null)
            {
                logmessage += stopwatch.ElapsedMilliseconds + "| Looking up player\n";
                var players = from p in db.Players
                              where p.Tag.Equals(tag)
                              select p;
                logmessage += stopwatch.ElapsedMilliseconds + "| LINQ call complete\n";
                if (players.Count(c => true) > 1)
                {
                    return View("SearchFoundMultiple", players);
                }
                player = players.FirstOrDefault();
                logmessage += stopwatch.ElapsedMilliseconds + "| Player selected\n";
            }
            else
            {
                player = db.Players.Find(id);
                if (!noRewrite && player.Tag.Equals(player.URL))
                {
                    return RedirectToAction("Detail", new { tag = player.Tag });
                }
            }
            if (player == null)
            {
                return RedirectToAction("Index");
            }
            logmessage += stopwatch.ElapsedMilliseconds + "| Finding opponent sets\n";
            var opponentSets = (from s in db.Sets
                                where s.LoserID == player.PlayerId || s.WinnerID == player.PlayerId
                                select new List<int>() { s.WinnerID, s.LoserID });
            var opponentIDs = new HashSet<int>(opponentSets.SelectMany(s => s));
            logmessage += stopwatch.ElapsedMilliseconds + "| Opponent sets finished\n";
            logmessage += stopwatch.ElapsedMilliseconds + "| Players to dict start\n";
            Dictionary<int, string> IdToTag = (from p in db.Players
                                               where opponentIDs.Contains(p.PlayerId)
                                               select new { id = p.PlayerId, tag = p.Tag }).ToDictionary(p => p.id, p => p.tag);
            logmessage += stopwatch.ElapsedMilliseconds + "| Players to dict end\n";
            var tournaments = from t in db.Tournaments select t;
            logmessage += stopwatch.ElapsedMilliseconds + "| Opponents start\n";
            var opponentLinq = from p in db.Players
                               where opponentIDs.Contains(p.PlayerId)
                               select p;
            string query = String.Format("SELECT * FROM Players WHERE PlayerId IN ({0})", String.Join(",", opponentIDs));
            var opponents = db.Database.SqlQuery<Player>(query).ToList();
            logmessage += stopwatch.ElapsedMilliseconds + "| Opponents end\n";
            logmessage += stopwatch.ElapsedMilliseconds + "| Get recent sets start\n";
            List<BiasedSet> sets = getRecentSets(player.PlayerId, IdToTag, tournaments, opponents);
            logmessage += stopwatch.ElapsedMilliseconds + "| Get recent sets end\n";
            logmessage += stopwatch.ElapsedMilliseconds + "| Set counts start\n";
            int wins = sets.Count(s => s.won);
            int draws = sets.Count(s => s.isDraw);
            int losses = sets.Count(s => !s.won && !s.isDraw);
            logmessage += stopwatch.ElapsedMilliseconds + "| Set counts end\n";

            if (Convert.ToBoolean(Session["userModerator"]))
            {
                ViewBag.JavascriptIncludes.Add("~/Scripts/Player-Admin.js");
            }

            logmessage += stopwatch.ElapsedMilliseconds + "| Elo scores start\n";
            var eloScores = from es in db.EloScores
                            where es.PlayerID == player.PlayerId
                            orderby es.Date ascending
                            select new { Month = es.Date.Month - 1, Year = es.Date.Year, Day = es.Date.Year, ELO = es.ELO };
            logmessage += stopwatch.ElapsedMilliseconds + "| Elo scores end\n";

            List<object[]> data = new List<object[]>();
            foreach (var score in eloScores)
            {
                object[] tmp = new object[2];
                //month - 1 because javascript thinks january is 0 but C# thinks january is 1
                tmp[0] = new { month = score.Month - 1, year = score.Year, day = score.Day };
                tmp[1] = Convert.ToInt32(score.ELO);
                data.Add(tmp);
            }
            var eloData = new { data = data.ToArray() };
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            logmessage += stopwatch.ElapsedMilliseconds + "| All players start\n";
            var allPlayers = db.Players.Select(p => p).ToList();
            logmessage += stopwatch.ElapsedMilliseconds + "| All players end\n";
            log(logmessage.Replace("\n", "<br>"));
            var vm = new PlayerDetailViewModel(player, wins, losses, draws, sets, serializer.Serialize(eloData), allPlayers);
            db.Dispose();
            return View(vm);
        }

        public ActionResult Index()
        {
            IEnumerable<Player> players;
            if (Request["sort"] != null && Request["sort"] == "rating")
            {
                players = from p in db.Players
                          orderby p.ELO descending
                          select p;
            }
            else
            {
                players = from p in db.Players
                          orderby p.Tag ascending
                          select p;
            }
            players = players.ToList();
            db.Dispose();
            return View(players);
        }

        public ActionResult Search()
        {
            string tag = Request["tag"];
            if (string.IsNullOrWhiteSpace(tag))
            {
                return RedirectToAction("Index");
            }
            tag = tag.Trim();
            var onePlayer = db.Players.Where(p => p.Tag == tag).FirstOrDefault();
            if (onePlayer != null)
            {
                return RedirectToAction("Detail", new { tag = onePlayer.URL });
            }
            tag = tag.ToUpperInvariant();
            char[] tagArray = tag.ToCharArray();
            var allPlayers = from p in db.Players
                             select p;
            int maxDistance = Math.Max((int)((double)tagArray.Length / 5.0), 1); //fool with this if it sux
            List<Player> foundPlayers = new List<Player>();
            foreach (Player player in allPlayers)
            {
                string playerTag = player.Tag.ToUpperInvariant();
                int distance = LevenshteinDistance.Compute(playerTag, tag);
                if (distance <= maxDistance)
                {
                    var computeArray = playerTag.ToCharArray();
                    int union = tagArray.Union(computeArray).Count(); //total chars
                    int intersect = tagArray.Intersect(computeArray).Count(); //shared chars
                    if ((union * 1.0 - intersect) / union <= .5)
                    { //non-shared chars. should be close to 0
                        foundPlayers.Add(player);
                    }
                }
            }
            int players = foundPlayers.Count();
            if (players == 0)
            {
                db.Dispose();
                return RedirectToAction("Index");
            }
            else if (players == 1)
            {
                String playerURL = foundPlayers.First().URL;
                db.Dispose();
                return RedirectToAction("Detail", new { tag = playerURL });
            }
            else
            {
                var viewPlayers = foundPlayers.AsQueryable<Player>();
                db.Dispose();
                return View("SearchFoundMultiple", viewPlayers);
            }
        }


        public ActionResult Versus(string playerOneTag, string playerTwoTag)
        {
            int playerOneId = -1;
            int playerTwoId = -1;
            try
            {
                playerOneId = Convert.ToInt32(playerOneTag);
            }
            catch (FormatException e)
            {
            }
            try
            {
                playerTwoId = Convert.ToInt32(playerTwoTag);
            }
            catch (FormatException e) { }

            var players = from p in db.Players
                          where p.Tag.Equals(playerOneTag) || p.Tag.Equals(playerTwoTag)
                          || p.PlayerId == playerOneId || p.PlayerId == playerTwoId
                          select p;
            if (players.Count() != 2)
            {
                return RedirectToAction("Index");
            }

            Player player1 = players.Where(p => p.Tag.Equals(playerOneTag) || p.PlayerId == playerOneId).First();
            Player player2 = players.Where(p => p.Tag.Equals(playerTwoTag) || p.PlayerId == playerTwoId).First();

            var sets = (from s in db.Sets
                        where (s.WinnerID == player1.PlayerId && s.LoserID == player2.PlayerId)
                             || (s.WinnerID == player2.PlayerId && s.LoserID == player1.PlayerId)
                        select s).Distinct();
            ViewBag.Sets = sets.OrderByDescending(s => s.DatePlayed);
            ViewBag.Player1 = player1;
            ViewBag.Player2 = player2;
            return View();
        }

        public ActionResult UpdatePlayer(int playerId)
        {
            if (!Convert.ToBoolean(Session["userAdmin"]) && !Convert.ToBoolean(Session["userModerator"]))
            {
                var notAdminJson = new { response = "Unauthorized" };
                return Json(notAdminJson);
            }
            Player player = db.Players.Find(playerId);
            if (player == null)
            {
                db.Dispose();
                var notFoundJson = new { response = "Player not found." };
                return Json(notFoundJson);
            }
            string newTag = Request["newTag"];
            if (!String.IsNullOrWhiteSpace(newTag))
            {
                if (db.Players.Count(p => p.Tag.ToUpper().Equals(newTag.ToUpper())) > 0)
                {
                    var tagInUseJson = new { response = "This tag is already in use." };
                    return Json(tagInUseJson);
                }
                else
                {
                    player.Tag = newTag;
                }
            }
            int newRegion;
            bool isRegion = int.TryParse(Request["newRegion"], out newRegion);
            if (isRegion && Enum.IsDefined(typeof(Region), newRegion))
            {
                player.Region = (Region)newRegion;
                db.Database.ExecuteSqlCommand("DELETE FROM RegionFlags WHERE PlayerId = {0} AND RegionValue = {1}", playerId, newRegion);

            }
            int newChar;
            bool isChar = int.TryParse(Request["newChar"], out newChar);
            if (isChar && Enum.IsDefined(typeof(Character), newChar))
            {
                player.CharacterMain = (Character)newChar;
                bool overwrite = Convert.ToBoolean(Request["overwrite"]);
                if (overwrite)
                {
                    db.Database.ExecuteSqlCommand("UPDATE Sets SET WinnerCharacterId = {0} WHERE WinnerId = {1}", newChar, player.PlayerId);
                    db.Database.ExecuteSqlCommand("UPDATE Sets SET LoserCharacterId = {0} WHERE LoserId = {1}", newChar, player.PlayerId);

                }
                else
                {
                    db.Database.ExecuteSqlCommand("UPDATE Sets SET WinnerCharacterId = {0} WHERE WinnerId = {1} AND (WinnerCharacterID IS NULL OR WinnerCharacterID = {2})", newChar, player.PlayerId, (int)Character.NoCharacter);
                    db.Database.ExecuteSqlCommand("UPDATE Sets SET LoserCharacterId = {0} WHERE LoserId = {1} AND (LoserCharacterID IS NULL OR LoserCharacterID = {2})", newChar, player.PlayerId, (int)Character.NoCharacter);
                }
                db.Database.ExecuteSqlCommand("DELETE FROM CharacterFlags WHERE PlayerId = {0} AND CharacterID = {1}", player.PlayerId, newChar);
            }
            db.SaveChanges();
            db.Dispose();
            var json = new { response = "Player updated!" };
            return Json(json);

        }


        private List<BiasedSet> getRecentSets(int playerId, Dictionary<int, string> IdToTag, IEnumerable<Tournament> allTournaments, IEnumerable<Player> opponents)
        {
            var sets = (from s in db.Sets
                        join sl in db.SetLinks on s.SetID equals sl.SetID into slg
                        where s.WinnerID == playerId || s.LoserID == playerId
                        from links in slg.DefaultIfEmpty()
                        select new { set = s, link = links });
            //log(sets.ToString());
            var biasedSets = new List<BiasedSet>();
            foreach (var set in sets)
            {
                Tournament tournament = allTournaments.Where(t => t.TournamentID == set.set.TournamentID).FirstOrDefault();
                biasedSets.Add(new BiasedSet(set.set, playerId, opponents, tournament, set.link != null));
            }

            return biasedSets.OrderBy(s => s.date).Reverse().Distinct().ToList();
        }

    }
}
