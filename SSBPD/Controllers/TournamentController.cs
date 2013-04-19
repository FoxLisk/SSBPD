using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.ViewModels;
using SSBPD.Helper;
using System.Diagnostics;

namespace SSBPD.Controllers
{
    public class Pool
    {
        public int PoolNum { get; set; }
        public string BracketName { get; set; }
        //wins[i][j] is the square in a standard pools representation representing row i, column j (because arrays have indices and that sux)
        public Dictionary<int, Dictionary<int, Tuple<string, Character, Character, int>>> wins = new Dictionary<int, Dictionary<int, Tuple<string, Character, Character, int>>>();
        public List<int> playerIDs;

        public Pool(List<Set> sets)
        {
            PoolNum = sets[0].PoolNum ?? 9999;
            BracketName = sets[0].BracketName;
            HashSet<int> playerIDs = new HashSet<int>();
            foreach (Set set in sets)
            {
                int setWins = set.Wins.Value;
                int losses = set.Losses.Value;
                if (!wins.ContainsKey(set.WinnerID))
                {
                    wins.Add(set.WinnerID, new Dictionary<int, Tuple<string, Character, Character, int>>());
                }
                if (!wins.ContainsKey(set.LoserID))
                {
                    wins.Add(set.LoserID, new Dictionary<int, Tuple<string, Character, Character, int>>());
                }

                wins[set.WinnerID].Add(set.LoserID, new Tuple<string, Character, Character, int>(String.Format("{0} - {1}", setWins, losses), set.WinnerCharacter, set.LoserCharacter, set.SetID));
                wins[set.LoserID].Add(set.WinnerID, new Tuple<string, Character, Character, int>(String.Format("{0} - {1}", losses, setWins), set.LoserCharacter, set.WinnerCharacter, set.SetID));
                playerIDs.Add(set.WinnerID);
                playerIDs.Add(set.LoserID);
            }
            this.playerIDs = playerIDs.ToList();
        }
    }

    public class EntrantsComparer : IComparer<int>
    {
        private Dictionary<int, int> tournamentToEntrants;
        public EntrantsComparer(Dictionary<int, int> tournamentToEntrants)
        {
            this.tournamentToEntrants = tournamentToEntrants;
        }
        public int Compare(int a, int b)
        {
            if (tournamentToEntrants[a] > tournamentToEntrants[b])
            {
                return -1;
            }
            else if (tournamentToEntrants[a] == tournamentToEntrants[b])
            {
                return 0;
            }
            return 1;
        }
    }

    public class TournamentController : BaseController
    {
        public TournamentController()
        {
            ViewBag.CSSIncludes.Add("~/Content/Tournament.css");
            ViewBag.CSSIncludes.Add("~/Content/jquery.fancybox-1.3.4.css");
            ViewBag.JavascriptIncludes.Add("~/Scripts/Tournament.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/FancyboxUtils.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/jQuery/jquery.fancybox-1.3.4.js");
        }
        public ActionResult Index()
        {
            IEnumerable<Tournament> tournaments = (from t in db.Tournaments
                                                  orderby t.Date descending
                                                  select t).ToList();
            var tournamentToEntrants = new Dictionary<int, int>();
            foreach (Tournament tournament in tournaments)
            {
                tournamentToEntrants[tournament.TournamentID] = getPlayerCount(tournament.TournamentID);
            }

            if (Request["sort"] != null && Request["sort"] == "name")
            {
                tournaments = tournaments.OrderBy(t => t.Name);
            }
            else if (Request["sort"] != null && Request["sort"] == "entrants")
            {
                tournaments = tournaments.OrderBy(t => t.TournamentID, new EntrantsComparer(tournamentToEntrants));
            }
            db.Dispose();
            var vm = new TournamentIndexViewModel(tournaments, tournamentToEntrants);
            return View(vm);
        }
        public ActionResult ViewBracket(int tournamentId, string bracketName)
        {
            Bracket bracket = null;
            try
            {
                bracket = new Bracket(tournamentId, bracketName); //this doesn't exist but building a bracketshould only get sets from one bracket
            } catch (SingleElimNotSupportedException e) {
                var errorVm = new BracketViewModel(null, null, bracketName, "Single elimination tournaments are not currently supported, sorry.");
                return PartialView(errorVm);
            }
            var winnersGrid = buildBracketGrid(bracket.winnersBracket, "right");
            var losersGrid = buildBracketGrid(bracket.losersBracket, "right");
            var vm = new BracketViewModel(winnersGrid, losersGrid, bracketName, null);
            return PartialView(vm);

        }
        public ActionResult Update(int tournamentId)
        {
            if (!Convert.ToBoolean(Session["userModerator"]))
            {
                var unauthorizedJson = new { response = "Unauthorized." };
                return Json(unauthorizedJson);
            }
            Tournament tournament = db.Tournaments.Find(tournamentId);
            if (tournament == null)
            {
                var notFoundJson = new { response = "Something went wrong, please try again later." };
                return Json(notFoundJson);
            }
            int month, day, year;
            int.TryParse(Request["tournamentMonth"], out month);
            int.TryParse(Request["tournamentDay"], out day);
            int.TryParse(Request["tournamentYear"], out year);
            DateTime? newDate = null;
            try
            {
                newDate = new DateTime(year, month, day);
                tournament.Date = newDate.Value;
                db.Database.ExecuteSqlCommand("UPDATE Sets SET DatePlayed = {0} WHERE TournamentID = {1}", newDate, tournament.TournamentID);
            }
            catch { }
            string newName = Request["name"];
            if (!String.IsNullOrWhiteSpace(newName))
            {
                tournament.Name = newName;
            }
            db.SaveChanges();
            db.Dispose();
            var json = new { response = "Tournament updated!", newDate = newDate == null ? "" : newDate.Value.ToString("MM/dd/yyyy"), newName = newName };
            return Json(json);
        }
        private int getPlayerCount(int tournamentId)
        {
            var tournamentSets = from s in db.Sets
                                 where s.TournamentID == tournamentId
                                 select new { s.WinnerID, s.LoserID };
            var players = new HashSet<int>();
            foreach (var s in tournamentSets)
            {
                players.Add(s.WinnerID);
                players.Add(s.LoserID);
            }
            return players.Count;
        }

        public ActionResult Search()
        {
            var tournamentName = Request["name"];
            return RedirectToAction("Detail", new { tournamentName = tournamentName });
        }

        public ActionResult Detail(string tournamentName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string logmessage = "";
            logmessage += stopwatch.ElapsedMilliseconds + " | Start Get tournament for " + tournamentName + "<br>";
            Tournament tournament;
            int? id = null;
            try
            {
                id = Convert.ToInt32(tournamentName);
                tournament = db.Tournaments.Find(id);
            }
            catch (FormatException e)
            {
                tournament = (from t in db.Tournaments
                              where t.Name.Equals(tournamentName)
                              select t).FirstOrDefault();
            }
            logmessage += stopwatch.ElapsedMilliseconds + " | End Get tournament<br>";
            if (tournament == null)
            {
                return RedirectToAction("Index");
            }
            logmessage += stopwatch.ElapsedMilliseconds + " | Start get sets<br>";
            var tournamentSets = from set in db.Sets
                                 where set.TournamentID == tournament.TournamentID
                                 select set;
            logmessage += stopwatch.ElapsedMilliseconds + " | End get sets<br>";
            var tournamentPlayers = from p in db.Players
                                    from s in tournamentSets
                                    where p.PlayerId == s.WinnerID || p.PlayerId == s.LoserID
                                    select p;
            logmessage += stopwatch.ElapsedMilliseconds + " | Start player dict<br>";
            Dictionary<int, Player> idToPlayer = new Dictionary<int, Player>();
            foreach (Player p in tournamentPlayers.Distinct())
            {
                idToPlayer.Add(p.PlayerId, p);
            }
            logmessage += stopwatch.ElapsedMilliseconds + " | End player dict<br>";
            logmessage += stopwatch.ElapsedMilliseconds + " | Start build pools<br>";
            var poolsBrackets = from set in tournamentSets
                                where set.isPool
                                group set by set.BracketName;
            //e.g. Melee Singles R1 -> [[Pool information]]
            Dictionary<string, List<Pool>> poolsEvents = new Dictionary<string, List<Pool>>();
            foreach (var poolsBracket in poolsBrackets)
            {
                string bracketName = poolsBracket.Key;
                poolsEvents[bracketName] = new List<Pool>();
                var pools = from set in poolsBracket
                            group set by set.PoolNum;
                foreach (var poolsSet in pools)
                {
                    poolsEvents[bracketName].Add(new Pool(poolsSet.ToList()));
                }
            }
            logmessage += stopwatch.ElapsedMilliseconds + " | End build pools<br>";
            logmessage += stopwatch.ElapsedMilliseconds + " | Start brackets<br>";
            var bracketSets = (from set in tournamentSets
                              where !set.isPool
                              orderby set.DatePlayed descending
                              group set by set.BracketName).ToList();
            logmessage += stopwatch.ElapsedMilliseconds + " | End brackets<br>";
            log(logmessage);
            db.Dispose();
            TournamentDetailViewModel vm = new TournamentDetailViewModel(bracketSets, poolsEvents, idToPlayer, tournament);
            return View(vm);

        }

        private List<List<T>> ToList<T>(BinaryTree<T> tree)
        {
            List<List<T>> outArray = new List<List<T>>();
            var currentNodes = new List<BinaryTree<T>>() { tree };
            var nextNodes = new List<BinaryTree<T>>();
            while (currentNodes.Count > 0)
            {
                var nextLevel = new List<T>();
                foreach (var node in currentNodes)
                {
                    nextLevel.Add(node.node);
                    if (node.leftChild != null)
                    {
                        nextNodes.Add(node.leftChild);
                    }
                    if (node.rightChild != null)
                    {
                        nextNodes.Add(node.rightChild);
                    }
                }
                currentNodes.Clear();
                currentNodes.AddRange(nextNodes);
                nextNodes.Clear();
                outArray.Add(nextLevel);
            }
            return outArray;
        }
        private BracketCell[,] buildBracketGrid(BinaryTree<Set> bracket, string sideClass)
        {
            var bracketInfo = ToList(bracket); //List<List<Set>>
            bracketInfo.Reverse();
            Set final = new Set();
            final.WinnerID = bracketInfo.First().First().WinnerID;
            final.LoserID = 0;
            bracketInfo.Add(new List<Set>() { final });
            int rounds = bracketInfo.Count;
            int players = 2 * bracketInfo[0].Count;
            int rows = 2 * players;
            var outputGrid = new BracketCell[rows, rounds];
            string bottomClass = "";
            for (int round = 0; round < rounds; round++)
            {
                int playerDataHeight = (int)Math.Pow(2, round + 1);
                int playerDataLoc = (int)Math.Pow(2, round) + 1;
                int set = 0;
                bool isWinner = true;
                bool isRightBorder = false;

                for (int row = 0; row < rows; row++)
                {
                    if ((row % playerDataHeight) == (playerDataLoc - 1))
                    {
                        if (isWinner)
                        {
                            if (bracketInfo[round][set].WinnerID > 0)
                            {
                                bottomClass = "bottom";
                                isRightBorder = true;
                            }
                            else
                            {
                                bottomClass = "";
                                isRightBorder = false;
                            }
                            var cell = new BracketCell(bracketInfo[round][set].WinnerID, bottomClass);
                            outputGrid[row, round] = cell;
                            isWinner = false;
                        }
                        else
                        {
                            string tmpSideClass;
                            if (bracketInfo[round][set].WinnerID > 0)
                            {
                                bottomClass = "bottom";
                                tmpSideClass = sideClass;
                            }
                            else
                            {
                                bottomClass = "";
                                tmpSideClass = "";
                            }
                            var cell = new BracketCell(bracketInfo[round][set].LoserID, bottomClass, tmpSideClass);
                            outputGrid[row, round] = cell;
                            isWinner = true;
                            isRightBorder = false;
                            set++;
                        }
                    }
                    else
                    {
                        string tdClass = (isRightBorder && round < rounds - 1) ? sideClass : "";
                        var cell = new BracketCell(tdClass);
                        outputGrid[row, round] = cell;
                    }
                }
            }
            return outputGrid;
        }

    }
}
