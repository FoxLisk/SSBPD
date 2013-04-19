using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.Helper;
using System.Diagnostics;
using SSBPD.ViewModels;

namespace SSBPD.Controllers
{
    public class CharacterStats
    {
        public class Stats
        {
            public int wins;
            public int losses;
            public int draws;
            public Stats()
            {
                wins = 0;
                losses = 0;
                draws = 0;
            }
            public double ratio
            {
                get
                {
                    if (wins + draws + losses == 0) { return 0; }
                    return 100 * ((wins * 1.0 + draws * 0.5) / (wins + losses + draws) * 1.0);
                }
            }
        }
        public Character character;
        public int totalWins = 0;
        public int totalLosses = 0;
        public int totalDraws = 0;
        public double totalRatio;
        public double expectedWinRatio;
        public Dictionary<Character, Stats> stats = new Dictionary<Character, Stats>();
        public Dictionary<Character, double> opponentToExpectedScore = new Dictionary<Character, double>();
        public CharacterStats(Character thisChar, IEnumerable<Set> sets, List<EloScore> scores, IRatingsCalculator calculator)
        {

            this.character = thisChar;
            Dictionary<Character, List<Tuple<double, double>>> opponentToBothElos = new Dictionary<Character, List<Tuple<double, double>>>();
            foreach (Character character in CharacterUtils.Characters)
            {
                stats[character] = new Stats();
                opponentToBothElos[character] = new List<Tuple<double, double>>();
            }

            foreach (Set set in sets.Distinct())
            {
                if (set.WinnerCharacter == set.LoserCharacter) { continue; }
                if (!set.isDraw)
                {
                    if (set.WinnerCharacter == character)
                    {
                        stats[set.LoserCharacter].wins += 1;
                        totalWins += 1;
                    }
                    else
                    {
                        stats[set.WinnerCharacter].losses += 1;
                        totalLosses += 1;
                    }
                }
                else
                {
                    totalDraws += 1;
                    if (set.WinnerCharacter == character)
                    {
                        stats[set.LoserCharacter].draws += 1;
                    }
                    else
                    {
                        stats[set.WinnerCharacter].draws += 1;
                    }
                }
                    double? winnerRating = null;
                    double? loserRating = null;
                    var winnerScore = (from e in scores
                                       where e.PlayerID == set.WinnerID && e.TournamentID == set.TournamentID
                                       select e).FirstOrDefault();
                    if (winnerScore != null)
                    {
                        winnerRating = winnerScore.ELO;
                    }
                    var loserScore = (from e in scores
                                      where e.PlayerID == set.LoserID && e.TournamentID == set.TournamentID
                                      select e).FirstOrDefault();
                    if (loserScore != null)
                    {
                        loserRating = loserScore.ELO;
                    }
                    if (set.WinnerCharacter == thisChar)
                    {
                        if (winnerRating != null && loserRating != null)
                        {
                            opponentToBothElos[set.LoserCharacter].Add(new Tuple<double, double>(winnerRating.Value, loserRating.Value));
                        }
                    }
                    else // (set.LoserCharacter == thisChar)
                    {

                        if (winnerRating != null && loserRating != null)
                        {
                            opponentToBothElos[set.WinnerCharacter].Add(new Tuple<double, double>(loserRating.Value, winnerRating.Value));
                        }
                    }
            }

            foreach (Character character in CharacterUtils.Characters)
            {
                double averageWinner, averageLoser;
                averageWinner = opponentToBothElos[character].Count > 0 ? opponentToBothElos[character].Average(tup => tup.Item1) : 1200;
                averageLoser = opponentToBothElos[character].Count > 0 ? opponentToBothElos[character].Average(tup => tup.Item2) : 1200;
                if (averageWinner != 1200 && averageLoser != 1200)
                {
                    int i = 0;
                    int j = 1;
                    i *= j;
                }
                opponentToExpectedScore[character] = 100 * calculator.getExpectedScore(averageWinner, averageLoser);
            }
            if (totalWins + totalLosses + totalDraws == 0) { totalRatio = 0; }
            else
            {
                totalRatio = 100 * ((totalWins * 1.0 + totalDraws * 0.5) / (totalWins + totalLosses + totalDraws) * 1.0);
                double totalAverageWinner, totalAverageLoser;
                var l = opponentToBothElos.SelectMany(f => f.Value);
                totalAverageWinner = l.Average(tup => tup.Item1);
                totalAverageLoser = l.Average(tup => tup.Item2);
                expectedWinRatio = 100 * calculator.getExpectedScore(totalAverageWinner, totalAverageLoser);
            }

        }

    }
    public class CharacterController : BaseController
    {
        public CharacterController()
        {
            ViewBag.JavascriptIncludes.Add("~/Scripts/Character.js");
            ViewBag.CSSIncludes.Add("~/Content/Character.css");
        }

        public ActionResult Index()
        {
            Dictionary<Character, IEnumerable<Player>> playersByCharacter = new Dictionary<Character, IEnumerable<Player>>();
            foreach (Character character in CharacterUtils.Characters)
            {
                int charId = (int)character;
                var players = from p in db.Players
                              where p.CharacterMainID == charId
                              orderby p.ELO descending
                              select p;
                playersByCharacter[character] = players.Take(10);
            }
            return View(playersByCharacter);
        }

        public ActionResult Detail(string characterName)
        {
            Character? character = null;
            if (Enum.IsDefined(typeof(Character), characterName)) {
                character = (Character) Enum.Parse(typeof(Character), characterName);
            }
            if (character == null){
                return RedirectToAction("Index");
            }
            int charId = (int) character;
            var players = from p in db.Players
                          where p.CharacterMainID == charId
                          orderby p.ELO descending
                          select p;
            var vm = new CharacterDetailViewModel(players, character.Value);
            return View(vm);
        }

        public ActionResult Stats()
        {
            List<CharacterStats> stats = new List<CharacterStats>();
            var calculator = new EloCalculator();
            var scores = (from e in db.EloScores
                          select e).ToList();
            
            foreach (Character character in CharacterUtils.Characters)
            {
                if (Application[character.ToString() + "Stats"] != null)
                {
                    stats.Add((CharacterStats)Application[character.ToString() + "Stats"]);
                    Debug.WriteLine("Hit cache for " + character.ToString());
                }
                else
                {
                    int charId = (int)character;
                    var sets = from s in db.Sets
                               where s.WinnerCharacterID == charId || s.LoserCharacterID == charId
                               select s;
                    var charStats = new CharacterStats(character, sets, scores, calculator);
                    stats.Add(charStats);
                    Application[character.ToString() + "Stats"] = charStats;
                    Debug.WriteLine("Missed cache for " + character.ToString());
                    Application["lastProcessedStats"] = DateTime.Now;
                }
            }
            ViewBag.LastProcessed = Application["lastProcessedStats"];
            return View(stats);
        }

    }
}
