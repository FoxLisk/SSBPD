using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using SSBPD.Models;
using System.Globalization;

namespace SSBPD.Helper
{
    public class FileAlreadyParsedException : Exception { }
    public enum TioVersion
    {
        ONE_TWO_ZERO,
        ONE_ONE_THREE,
        UNKNOWN
    }
    public class TioParser
    {
        public static string BYE_PLAYER_ID = "00000001-0001-0001-0101-010101010101";
        public static string UNPLAYED_PLAYER_ID = "00000000-0000-0000-0000-000000000000";

        /**
         * Using ID in the TIO sense, not in the primary key in my tables sense
         */
        private Dictionary<string, Player> idToPlayer = new Dictionary<string, Player>();
        private Dictionary<string, Player> idToFoundPlayer = new Dictionary<string, Player>();
        /**
         * each list is the full, linked set of matches for a single or double elim bracket (assuming people might have more than one)
         */
        List<List<Match>> bracketsMatches = new List<List<Match>>();
        /**
         * each list is the set of all pools matches for a "bracket" in tio land; what we might think of as 1st round pools, 2nd round pools, etc,
         * correspondes to poolsMatches(1).
         */
        List<List<Match>> poolsMatches = new List<List<Match>>();

        HashSet<string> meleePlayerIDs = new HashSet<string>();

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
        private XDocument tournamentXML;
        private Tournament tournament;
        private TournamentFile tournamentFile;

        public TioParser(int tournamentFileId)
        {
            tournamentFile = db.TournamentFiles.Find(tournamentFileId);
            if (tournamentFile.Processed)
            {
                throw new FileAlreadyParsedException();
            }
            Tournament existingTournament = (from t in db.Tournaments
                                             where t.TournamentGuid.Equals(tournamentFile.TournamentGuid)
                                             select t).FirstOrDefault();
            if (existingTournament != null)
            {
                throw new FileAlreadyParsedException();
            }

            tournament = new Tournament();
            tournamentXML = XDocument.Parse(tournamentFile.XML);
            CultureInfo ci = new CultureInfo(tournamentXML.Descendants("Culture").First().Value, false);
            tournament.Date = DateTime.Parse(tournamentXML.Descendants("StartDate").First().Value, ci).AddHours(12);
            tournament.Name = tournamentXML.Descendants("Event").First().Element("Name").Value;
            tournament.locked = false;
            tournament.TournamentGuid = tournamentFile.TournamentGuid;
            db.Tournaments.Add(tournament);
            db.SaveChanges();
        }
        public TioParser(string tournamentXML)
        {
            var xml = XDocument.Parse(tournamentXML);
            this.tournamentXML = xml;
        }
        public XDocument SeedTournamentForBracket()
        {
            var events = from e in tournamentXML.Descendants("Event").Elements("Games").Elements("Game")
                         where e.Element("GameName").Value.IndexOf("Melee", StringComparison.OrdinalIgnoreCase) >= 0
                                && e.Element("GameType").Value == "Singles"
                         select e;

            foreach (XElement match in events.Descendants("Match"))
            {
                meleePlayerIDs.Add(match.Element("Player1").Value);
                meleePlayerIDs.Add(match.Element("Player2").Value);
            }

            var players = from e in tournamentXML.Element("AppData").Elements("PlayerList").Elements("Players").Elements("Player")
                          where e.Element("IsBye").Value == "False"
                          select e;

            initializePlayerList(players);
            double minElo = 9999;
            double maxElo = 0;
            var topPlayers = idToFoundPlayer.Values.OrderByDescending(p => p.ELO).Select(p => p.Tag).Take(4);
            foreach (var player in idToFoundPlayer.Values)
            {
                if (player.ELO < minElo)
                {
                    minElo = player.ELO;
                }
                if (player.ELO > maxElo && !topPlayers.Contains(player.Tag))
                {
                    maxElo = player.ELO;
                }
            }
            foreach (var player in players)
            {
                var id = player.Element("ID").Value;
                if (idToFoundPlayer.ContainsKey(id))
                {
                    var elo = idToFoundPlayer[id].ELO;
                    var skill = 7 * (elo - minElo) / (maxElo - minElo);
                    if (Math.Abs((int)(skill + 1) - skill) < .0005)
                    {
                        skill += .0005;
                    }
                    skill = (int)(skill + 1);
                    player.Element("Skill").SetValue(skill);
                }
                else
                {
                    player.Element("Skill").SetValue(1);
                }
                if (topPlayers.Take(2).Contains(player.Element("Nickname").Value)){
                    player.Element("Skill").SetValue(10);
                } else if (topPlayers.Skip(2).Contains(player.Element("Nickname").Value)){
                    player.Element("Skill").SetValue(9);
                }

            }
            return tournamentXML;
        }
        public XDocument SeedTournamentForPools()
        {
            var events = from e in tournamentXML.Descendants("Event").Elements("Games").Elements("Game")
                         where e.Element("GameName").Value.IndexOf("Melee", StringComparison.OrdinalIgnoreCase) >= 0
                                && e.Element("GameType").Value == "Singles"
                         select e;

            foreach (XElement match in events.Descendants("Match"))
            {
                meleePlayerIDs.Add(match.Element("Player1").Value);
                meleePlayerIDs.Add(match.Element("Player2").Value);
            }

            var players = from e in tournamentXML.Element("AppData").Elements("PlayerList").Elements("Players").Elements("Player")
                          where e.Element("IsBye").Value == "False"
                          select e;

            initializePlayerList(players);
            var playerList = idToFoundPlayer.Values.OrderBy(p => p.ELO).ToList();
            double numPlayers = playerList.Count;
            int bucketSize = (int)numPlayers / 10;
            foreach (var player in players)
            {
                var id = player.Element("ID").Value;
                Player playerModel = null;
                if (idToFoundPlayer.ContainsKey(id))
                {
                    playerModel = idToFoundPlayer[id];
                }
                if (playerModel == null)
                {
                    player.Element("Skill").SetValue(1);
                }
                else
                {
                    double index = playerList.IndexOf(playerModel);
                    var skill = 10 * (index / numPlayers) + 1;
                    player.Element("Skill").SetValue((int)skill);
                }
            }

            return tournamentXML;
        }

        /**
         * Parses the tournament file associated with this TioParser
         * Returns the ID of the resulting tournament object.
         */
        public int ParseTournament()
        {
            tournament.locked = true;
            db.SaveChanges();
            var events = from e in tournamentXML.Descendants("Event").Elements("Games").Elements("Game")
                         where e.Element("GameName").Value.IndexOf("Melee", StringComparison.OrdinalIgnoreCase) >= 0
                                && e.Element("GameType").Value == "Singles"
                         select e;

            foreach (XElement match in events.Descendants("Match"))
            {
                meleePlayerIDs.Add(match.Element("Player1").Value);
                meleePlayerIDs.Add(match.Element("Player2").Value);
            }

            var players = from e in tournamentXML.Element("AppData").Elements("PlayerList").Elements("Players").Elements("Player")
                          where e.Element("IsBye").Value == "False"
                          select e;

            initializePlayerList(players);
            db.SaveChanges();

            foreach (var myEvent in events)
            {
                if (myEvent.Element("BracketType").Value.EndsWith("Elim"))
                {
                    bracketsMatches.Add(getBracketMatchesFromXML(myEvent));
                }
                else if (myEvent.Element("BracketType").Value.Equals("RoundRobin"))
                {
                    poolsMatches.Add(getPoolsMatchesFromXML(myEvent));
                }
            }

            foreach (List<Match> matches in bracketsMatches.Union(poolsMatches))
            {
                foreach (Match match in matches)
                {
                    if (match.Winner == null || match.Loser == null) //byes or unplayed
                    {
                        continue;
                    }
                    Set set = match.toSet();
                    set.TournamentID = tournament.TournamentID;
                    db.Sets.Add(set);
                }
            }
            tournament.locked = false;
            tournamentFile.Processed = true;
            tournamentFile.ProcessedAt = DateTime.Now;
            db.SaveChanges();
            return tournament.TournamentID;
        }

        private List<Match> getPoolsMatchesFromXML(XElement myEvent)
        {
            List<Match> poolMatches = new List<Match>();
            string bracketName = myEvent.Element("Name").Value;
            foreach (XElement pool in myEvent.Element("Bracket").Element("Pools").Elements("Pool"))
            {
                int poolNum = Convert.ToInt32(pool.Element("Number").Value);
                foreach (XElement match in pool.Descendants("Match"))
                {
                    poolMatches.Add(new PoolsMatch(match, idToPlayer, tournament.Date, poolNum, bracketName));
                }
            }
            return poolMatches;
        }
        private List<Match> getBracketMatchesFromXML(XElement myEvent)
        {
            string bracketName = myEvent.Element("Name").Value;
            List<Match> bracketMatches = new List<Match>();
            Dictionary<int?, BracketMatch> matchNumToMatch = new Dictionary<int?, BracketMatch>();

            foreach (XElement match in myEvent.Element("Bracket").Element("Matches").Elements("Match"))
            {
                BracketMatch currMatch = new BracketMatch(match, idToPlayer, tournament.Date, bracketName);
                bracketMatches.Add(currMatch);
                matchNumToMatch.Add(currMatch.Number, currMatch);
            }
            foreach (BracketMatch match in bracketMatches)
            {
                if (match.WinnerGoesTo != null)
                {
                    match.winnerGoesToMatch = matchNumToMatch[match.WinnerGoesTo];
                }
                if (match.LoserGoesTo != null)
                {
                    match.loserGoesToMatch = matchNumToMatch[match.LoserGoesTo];
                }
            }

            return bracketMatches;
        }
        private void initializePlayerList(IEnumerable<XElement> players)
        {
            Dictionary<string, string> playerTagToID = new Dictionary<string, string>();

            foreach (var player in players)
            {
                string id = player.Element("ID").Value;
                if (!meleePlayerIDs.Contains(id))
                {
                    continue;
                }
                playerTagToID.Add(player.Element("Nickname").Value, id);
            }

            var allPlayerTags = playerTagToID.Keys.ToList();

            var foundPlayers = (from existingPlayer in db.Players
                                where allPlayerTags.Contains(existingPlayer.Tag)
                                select existingPlayer).ToList();

            var foundPlayerTags = (from f in foundPlayers
                                   select f.Tag.ToUpperInvariant()).ToList();

            var newPlayerTags = new List<string>(allPlayerTags);
            foreach (string tag in allPlayerTags)
            {
                if (foundPlayerTags.Contains(tag.ToUpperInvariant()))
                {
                    newPlayerTags.Remove(tag);
                    Player player = foundPlayers.Where(p => p.Tag.ToUpperInvariant().Equals(tag.ToUpperInvariant())).First();
                    idToPlayer.Add(playerTagToID[tag], player);
                    idToFoundPlayer.Add(playerTagToID[tag], player);
                }
            }

            foreach (var newTag in newPlayerTags)
            {
                if (newTag.Equals(BYE_PLAYER_ID) || newTag.Equals(UNPLAYED_PLAYER_ID))
                {
                    continue;
                }
                if (!meleePlayerIDs.Contains(playerTagToID[newTag]))
                {
                    continue;
                }
                Player newPlayer = new Player();
                newPlayer.Tag = newTag;
                newPlayer.ELO = ELOProcessor.STARTING_ELO;
                db.Players.Add(newPlayer);
                idToPlayer.Add(playerTagToID[newTag], newPlayer);
            }
        }

        public static XDocument getReadableXML(string XML)
        {
            XDocument xDoc = XDocument.Parse(XML);
            string version = xDoc.Element("AppData").Element("Version").Value;
            TioVersion tioVersion;
            if (version == "1.2.0")
            {
                tioVersion = TioVersion.ONE_TWO_ZERO;
            }
            else if (version == "1.1.3")
            {
                tioVersion = TioVersion.ONE_ONE_THREE;
            }
            else
            {
                tioVersion = TioVersion.UNKNOWN;
            }
            XDocument doc = new XDocument(
                new XElement("AppData",
                    new XElement("Culture", xDoc.Element("AppData").Element("Culture").Value),
                    new XElement("Version", xDoc.Element("AppData").Element("Version").Value),
                    getEventList(xDoc, tioVersion),
                    getPlayerList(xDoc)
                        )

                );

            return doc;
        }

        private static XElement getEventList(XDocument xDoc, TioVersion version)
        {
            var eventElements = new List<XElement>();
            XElement eventList = xDoc.Descendants("EventList").First();
            foreach (var _event in eventList.Elements("Event"))
            {
                XElement eventToAdd =
                    new XElement("Event",
                        new XElement("ID", _event.Element("ID").Value),
                        new XElement("Name", _event.Element("Name").Value),
                        new XElement("StartDate", _event.Element("StartDate").Value),
                        new XElement("Organizer", _event.Element("Organizer").Value),
                        new XElement("Location", _event.Element("Location").Value),
                        new XElement("InProgress", "False"),
                        getEventGames(_event.Element("Games"), version),
                        new XElement("Stations")
                        );
                eventElements.Add(eventToAdd);
            }
            XElement toReturn = new XElement("EventList", eventElements.Select(c => c));
            return toReturn;
        }

        private static XElement getEventGames(XElement _event, TioVersion version)
        {
            List<XElement> games = new List<XElement>();

            foreach (var game in _event.Elements("Game"))
            {
                XElement gameToAdd =
                    new XElement("Game",
                        new XElement("Name", game.Element("Name").Value),
                        new XElement("ID", game.Element("ID").Value),
                        new XElement("Date", game.Element("Date").Value),
                        new XElement("BracketType", game.Element("BracketType").Value),
                        new XElement("EntryFee", game.Element("EntryFee").Value),
                        new XElement("BasePotSize", 0),
                        new XElement(getEntrants(game)),
                        new XElement("GameType", game.Element("GameType").Value),
                        new XElement("HouseCut", game.Element("HouseCut").Value),
                        new XElement("HouseCutType", game.Element("HouseCutType").Value),
                        new XElement(game.Element("Payouts")),
                        new XElement("InProgress", "False"),
                        new XElement("SeparateByLocation", "False"),
                        new XElement("SeedType", game.Element("SeedType").Value),
                        new XElement(game.Element("Seeds")),
                        new XElement("GameName", game.Element("GameName").Value),
                        //new XElement(game.Element("Metrics")),
                        new XElement(game.Element("Bracket"))
                        );
                if (version == TioVersion.ONE_TWO_ZERO || version == TioVersion.UNKNOWN)
                {
                    gameToAdd.Add(game.Element("Metrics"));
                }
                games.Add(gameToAdd);
            }

            XElement toReturn = new XElement("Games", games.Select(c => c));
            return toReturn;
        }

        private static XElement getEntrants(XElement game)
        {
            XElement entrantsElement = game.Element("Entrants");
            if (entrantsElement.Elements("Entrant").Count() == 0)
            {
                return entrantsElement;
            }
            List<XElement> entrantElements = new List<XElement>();
            foreach (var entrant in entrantsElement.Elements("Entrant"))
            {
                string playerId = entrant.Element("PlayerID").Value;
                string seed = entrant.Element("Seed").Value;
                string amountPaid = entrant.Element("AmountPaid").Value;
                entrantElements.Add(
                    new XElement("PlayerID", new XAttribute("seed", seed), new XAttribute("AmountPaid", amountPaid), playerId)
                    );
            }
            return new XElement("Entrants", entrantElements.Select(el => el));
        }

        private static XElement getPlayerList(XDocument xDoc)
        {
            XElement players = xDoc.Descendants("PlayerList").First();
            return new XElement("PlayerList",
                players.Element("Players"),
                players.Element("Teams"));
        }




    }
}