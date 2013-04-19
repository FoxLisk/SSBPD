using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using SSBPD.Models;

namespace SSBPD.Helper
{
    internal abstract class Match
    {
        public Player Winner { get; set; }
        public Player Loser { get; set; }
        public int Number { get; set; }
        public DateTime datePlayed { get; set; }
        public string BracketName { get; set; }

        public Match(XElement matchXML, Dictionary<string, Player> idToPlayer, DateTime tournamentDate, string bracketName)
        {
            string player1ID = matchXML.Element("Player1").Value;
            string player2ID = matchXML.Element("Player2").Value;
            string winnerID = matchXML.Element("Winner").Value;

            //If either player is null, we don't actually have a set, so we can just set everything null and be done with it.
            if (player1ID == TioParser.BYE_PLAYER_ID || player1ID == TioParser.UNPLAYED_PLAYER_ID ||
                player2ID == TioParser.BYE_PLAYER_ID || player2ID == TioParser.UNPLAYED_PLAYER_ID ||
                winnerID == TioParser.BYE_PLAYER_ID || winnerID == TioParser.UNPLAYED_PLAYER_ID)
            {
                Winner = null;
                Loser = null;
                return;
            }
            
            Winner = idToPlayer[winnerID];
            if (player1ID == winnerID)
            {
                Loser = idToPlayer[player2ID];
            }
            else
            {
                Loser = idToPlayer[player1ID];
            }

            BracketName = bracketName;
        }

        internal abstract Set toSet();
    }

    internal class PoolsMatch : Match {
        public int poolNum { get; set; }
        public int losses { get; set; }
        public int wins { get; set; }
        public static int offset = 0;

        public PoolsMatch(XElement matchXML, Dictionary<string, Player> idToPlayer, DateTime tournamentDate, int poolNum, string bracketName)
            : base(matchXML, idToPlayer, tournamentDate, bracketName)
        {
            this.poolNum = poolNum;
            datePlayed = tournamentDate.AddSeconds(offset);
            this.losses = Convert.ToInt32(matchXML.Element("Losses").Value);
            
            int games = Convert.ToInt32(matchXML.Element("Games").Value);
            bool bestOf = matchXML.Element("SetType").Value.Equals("BestOf");
            if (bestOf)
            {
                wins = (games + 1) / 2;
            }
            else
            {
                wins = games - losses;
            }
        }

        internal override Set toSet()
        {
            Set set = new Set();
            set.WinnerID = Winner.PlayerId;
            set.LoserID = Loser.PlayerId;
            set.DatePlayed = datePlayed;
            set.BracketName = BracketName;
            set.isPool = true;
            set.Losses = losses;
            set.Wins = wins;
            set.isDraw = (wins == losses);
            set.PoolNum = poolNum;
            set.WinnerCharacter = Winner.CharacterMain;
            set.LoserCharacter = Loser.CharacterMain;
            return set;
        }

    }

    internal class BracketMatch : Match
    {
        public int Round { get; set; }
        public int? WinnerGoesTo;
        public int? LoserGoesTo;
        public bool IsWinners { get; set; }
        public Match winnerGoesToMatch;
        public Match loserGoesToMatch;

        public BracketMatch(XElement matchXML, Dictionary<string, Player> idToPlayer, DateTime tournamentDate, string bracketName) 
            : base ( matchXML, idToPlayer, tournamentDate, bracketName)
        {
            Number = Convert.ToInt32(matchXML.Element("Number").Value);
            Round = matchXML.Element("Round") == null ? -1 : Convert.ToInt32(matchXML.Element("Round").Value);
            WinnerGoesTo = Convert.ToInt32(matchXML.Element("WinnerNextMatch").Value);
            if (WinnerGoesTo == -1) WinnerGoesTo = null;
            if (matchXML.Element("LoserNextMatch") != null)
            {
                LoserGoesTo = Convert.ToInt32(matchXML.Element("LoserNextMatch").Value);
                if (LoserGoesTo == -1) LoserGoesTo = null;
            }
            else
            {
                LoserGoesTo = null;
            }
            if (matchXML.Element("IsWinners") == null)
            {
                IsWinners = true; //single-elim; there is no losers
            }
            else
            {
                IsWinners = Convert.ToBoolean(matchXML.Element("IsWinners").Value);
            }
            datePlayed = tournamentDate.AddSeconds(Round);
        }

        internal override Set toSet()
        {
            Set set = new Set();
            set.WinnerID = Winner.PlayerId;
            set.LoserID = Loser.PlayerId;
            set.DatePlayed = datePlayed;
            set.BracketName = BracketName;
            set.isPool = false;
            set.Round = this.Round;
            set.IsWinners = this.IsWinners;
            set.WinnerCharacter = Winner.CharacterMain;
            set.LoserCharacter = Loser.CharacterMain;
            return set;
        }
    }
}