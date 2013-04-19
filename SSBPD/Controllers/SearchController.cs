using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class SearchController : BaseController
    {
        public SearchController()
        {
            ViewBag.JavascriptIncludes.Add("~/Scripts/Search.js");
            ViewBag.CSSIncludes.Add("~/Content/jquery.fancybox-1.3.4.css");
            ViewBag.JavascriptIncludes.Add("~/Scripts/FancyboxUtils.js");
            ViewBag.JavascriptIncludes.Add("~/Scripts/jQuery/jquery.fancybox-1.3.4.js");
            ViewBag.CSSIncludes.Add("~/Content/Search.css");
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PlayerSearch()
        {
            string regionList = Request["regions"];
            string characterList = Request["characters"];
            int minRating, maxRating;
            bool hasMin = int.TryParse(Request["minRating"], out minRating);
            bool hasMax = int.TryParse(Request["maxRating"], out maxRating);
            if (!hasMin && !hasMax && string.IsNullOrWhiteSpace(regionList) && string.IsNullOrWhiteSpace(characterList))
            {
                return View("PlayerIndex");
            }
            List<Func<Player, bool>> predicates = new List<Func<Player, bool>>();
            predicates.Add(buildRegionPredicate(regionList));
            predicates.Add(buildCharacterPredicate(characterList));
            predicates.Add(buildRatingPredicate(minRating, hasMin, maxRating, hasMax));
            var filter = predicates.Aggregate(combinePredicates);
            var players = db.Players.Where(filter);
            return View(players);
        }

        public ActionResult SetSearch()
        {
            string winnerRatingType = Request["winnerRatingType"];
            RatingType winnerRating = winnerRatingType == "current" ? RatingType.Current : RatingType.SetTime;
            string winnerRegionList = Request["winnerRegions"];
            string winnerCharacterList = Request["winnerCharacters"];
            int winnerMinRating;
            int winnerMaxRating;
            bool winnerHasMin = int.TryParse(Request["winnerMinRating"], out winnerMinRating);
            bool winnerHasMax = int.TryParse(Request["winnerMaxRating"], out winnerMaxRating);
            DateTime? start = buildDate("start");
            DateTime? end = buildDate("end");
            string hasVideo = Request["hasVideo"];

            string loserRatingType = Request["loserRatingType"];
            RatingType loserRating = loserRatingType == "current" ? RatingType.Current : RatingType.SetTime;
            string loserRegionList = Request["loserRegions"];
            string loserCharacterList = Request["loserCharacters"];
            int loserMinRating;
            int loserMaxRating;
            bool loserHasMin = int.TryParse(Request["loserMinRating"], out loserMinRating);
            bool loserHasMax = int.TryParse(Request["loserMaxRating"], out loserMaxRating);

            if (!winnerHasMin && !winnerHasMax
                && string.IsNullOrWhiteSpace(winnerRegionList) && string.IsNullOrWhiteSpace(winnerCharacterList)
                && !loserHasMin && !loserHasMax
                && string.IsNullOrWhiteSpace(loserRegionList) && string.IsNullOrWhiteSpace(loserCharacterList)
                && !start.HasValue && !end.HasValue
                && (String.IsNullOrWhiteSpace(hasVideo) || hasVideo == "either"))
            {
                return View("SetIndex");
            }
            var datePredicate = buildDatePredicateSet(start, end);
            List<Func<SetInfo, bool>> winnerPredicates = new List<Func<SetInfo, bool>>();
            winnerPredicates.Add(buildRegionPredicateSet(winnerRegionList));
            winnerPredicates.Add(buildCharacterPredicateSet(winnerCharacterList));
            winnerPredicates.Add(buildRatingPredicateSet(winnerMinRating, winnerHasMin, winnerMaxRating, winnerHasMax, winnerRating));
            winnerPredicates.Add(datePredicate);
            if (hasVideo == "yes")
            {
                winnerPredicates.Add((s) => s.hasVideo);
            }
            else if (hasVideo == "no")
            {
                winnerPredicates.Add((s) => !s.hasVideo);
            }
            IEnumerable<SetInfo> winners;
            winners = from s in db.Sets
                      join e in db.EloScores on new { playerId = s.WinnerID, tournamentId = s.TournamentID } equals new { playerId = e.PlayerID, tournamentId = e.TournamentID }
                      join p in db.Players on e.PlayerID equals p.PlayerId
                      join sl in db.SetLinks on s.SetID equals sl.SetID
                      into gj
                      from setlink in gj.DefaultIfEmpty()
                      select new SetInfo
                      {
                          setId = s.SetID,
                          playerId = p.PlayerId,
                          tournamentId = s.TournamentID,
                          regionId = p.RegionValue,
                          characterId = s.WinnerCharacterID,
                          ELO = e.ELO,
                          playerELO = p.ELO,
                          date = s.DatePlayed,
                          hasVideo = (setlink != null && !setlink.Deleted)
                      };
            var winnerSets = winners.Where(winnerPredicates.Aggregate(combinePredicates)).Distinct();
            List<Func<SetInfo, bool>> loserPredicates = new List<Func<SetInfo, bool>>();
            loserPredicates.Add(buildRegionPredicateSet(loserRegionList));
            loserPredicates.Add(buildCharacterPredicateSet(loserCharacterList));
            loserPredicates.Add(buildRatingPredicateSet(loserMinRating, loserHasMin, loserMaxRating, loserHasMax, loserRating));
            loserPredicates.Add(datePredicate);
            if (hasVideo == "yes")
            {
                loserPredicates.Add((s) => s.hasVideo);
            }
            else if (hasVideo == "no")
            {
                loserPredicates.Add((s) => !s.hasVideo);
            }
            var losers = from s in db.Sets
                         join e in db.EloScores on new { playerId = s.LoserID, tournamentId = s.TournamentID } equals new { playerId = e.PlayerID, tournamentId = e.TournamentID }
                         join p in db.Players on e.PlayerID equals p.PlayerId
                         join sl in db.SetLinks on s.SetID equals sl.SetID
                         into gj
                         from setlink in gj.DefaultIfEmpty()
                         select new SetInfo
                         {
                             setId = s.SetID,
                             playerId = p.PlayerId,
                             tournamentId = s.TournamentID,
                             regionId = p.RegionValue,
                             characterId = s.LoserCharacterID,
                             ELO = e.ELO,
                             playerELO = p.ELO,
                             date = s.DatePlayed,
                             hasVideo = (setlink != null && !setlink.Deleted)
                         };
            var loserSets = losers.Where(loserPredicates.Aggregate(combinePredicates)).Distinct();

            var sets = from w in winnerSets
                       join l in loserSets on w.setId equals l.setId
                       select new { w, l };
            List<DisplaySetInfo> validSets = new List<DisplaySetInfo>();
            foreach (var set in sets)
            {
                validSets.Add(new DisplaySetInfo(set.w, set.l));
            }

            return View(validSets);
        }

        private DateTime? buildDate(string prefix)
        {

            int month, day, year;
            if (!int.TryParse(Request[prefix + "Month"], out month)) return null;
            if (!int.TryParse(Request[prefix + "Day"], out day)) return null;
            if (!int.TryParse(Request[prefix + "Year"], out year)) return null;
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        private Func<T, bool> combinePredicates<T>(Func<T, bool> f1, Func<T, bool> f2)
        {
            return (t) => f1(t) && f2(t);
        }

        private Func<SetInfo, bool> buildDatePredicateSet(DateTime? start, DateTime? end)
        {
            return (s) => ((!start.HasValue || s.date > start.Value) && (!end.HasValue || s.date < end.Value));
        }

        private Func<Player, bool> buildRatingPredicate(int minRating, bool hasMin, int maxRating, bool hasMax)
        {
            return (p) => ((!hasMin || p.ELO >= minRating) && (!hasMax || p.ELO <= maxRating));
        }
        private Func<SetInfo, bool> buildRatingPredicateSet(int minRating, bool hasMin, int maxRating, bool hasMax, RatingType ratingType)
        {
            if (ratingType == RatingType.SetTime)
            {
                return (s) => ((!hasMin || s.ELO >= minRating) && (!hasMax || s.ELO <= maxRating));
            }
            else
            {
                return (s) => ((!hasMin || s.playerELO >= minRating) && (!hasMax || s.playerELO <= maxRating));
            }
        }

        private Func<SetInfo, bool> buildCharacterPredicateSet(string characterList)
        {
            var characters = getEnumSet<Character>(characterList);
            if (characters.Count() == 0)
            {
                return (s) => true;
            }
            return (s) => characters.Contains(s.character);
        }

        private Func<Player, bool> buildCharacterPredicate(string characterList)
        {
            HashSet<Character> characterSet = getEnumSet<Character>(characterList);
            if (characterSet.Count == 0)
            {
                return (p) => true;
            }
            return (p) => characterSet.Contains(p.CharacterMain);
        }
        private HashSet<T> getEnumSet<T>(string members)
        {
            if (members == null)
            {
                return new HashSet<T>();
            }
            IEnumerable<T> characters = members.Split(',')
                .Where(c => Enum.IsDefined(typeof(T), Convert.ToInt32(c))).Select<string, T>(c => (T)Enum.Parse(typeof(T), c));
            return new HashSet<T>(characters);
        }

        private Func<Player, bool> buildRegionPredicate(string regionList)
        {
            HashSet<Region> regionSet = getEnumSet<Region>(regionList);
            if (regionSet.Count == 0)
            {
                return (p) => true;
            }
            return (p) => regionSet.Contains(p.Region);
        }
        private Func<SetInfo, bool> buildRegionPredicateSet(string regionList)
        {
            HashSet<Region> regionSet = getEnumSet<Region>(regionList);
            if (regionSet.Count == 0)
            {
                return (p) => true;
            }
            return (p) => regionSet.Contains(p.region);
        }


        public class SetInfo : IEquatable<SetInfo>
        {
            public int setId, playerId, tournamentId;
            public Region region
            {
                get
                {
                    if (regionId == null) { return Region.NoRegion; }
                    return (Region)regionId;
                }
            }
            public int? regionId;
            public Character character
            {
                get
                {
                    if (characterId == null) { return Character.NoCharacter; }
                    else
                    {
                        return (Character)characterId;
                    }
                }
            }
            public int? characterId;
            public double ELO;
            public double playerELO;
            public DateTime date;
            public bool hasVideo;

            public bool Equals(SetInfo other)
            {
                return (this.setId == other.setId);
            }
            public override int GetHashCode()
            {
                return this.setId.GetHashCode();
            }
        }
        public class DisplaySetInfo
        {
            private SSBPDContext db = new SSBPDContext();
            public int setId;
            public Player winner;
            public Player loser;
            public Tournament tournament;
            public int winnerElo;
            public int loserElo;
            public Character winnerCharacter;
            public Character loserCharacter;
            public DisplaySetInfo(SetInfo winners, SetInfo losers)
            {
                if (winners.setId != losers.setId)
                {
                    throw new Exception("Must construct a full set info from two set info objects for the same set.");
                }
                this.setId = winners.setId;
                this.winner = db.Players.Find(winners.playerId);
                this.loser = db.Players.Find(losers.playerId);
                this.tournament = db.Tournaments.Find(winners.tournamentId);
                this.winnerElo = (int)winners.ELO;
                this.loserElo = (int)losers.ELO;
                this.winnerCharacter = winners.characterId.HasValue && Enum.IsDefined(typeof(Character), winners.characterId) ? (Character)winners.characterId : Character.NoCharacter;
                this.loserCharacter = losers.characterId.HasValue && Enum.IsDefined(typeof(Character), losers.characterId) ? (Character)losers.characterId : Character.NoCharacter;
            }

        }

        public enum RatingType
        {
            Current, SetTime
        }

    }

}
