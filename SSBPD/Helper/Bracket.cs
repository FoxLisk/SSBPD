using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.Helper
{
    public enum BracketType
    {
        SINGLE_ELIM
    }
    public class SingleElimNotSupportedException : Exception { }
    public class Bracket
    {
        public BinaryTree<Set> winnersBracket;
        public BinaryTree<Set> losersBracket;
        private int winnersRounds;
        private int losersRounds;
        private SSBPDContext _db;
        public SSBPDContext db
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
        private int tournamentId;
        public Dictionary<int, List<Set>> rounds = new Dictionary<int, List<Set>>();
        public Bracket(int tournamentId, string bracketName)
        {
            this.tournamentId = tournamentId;
            var sets = from s in db.Sets
                       where s.TournamentID == tournamentId && s.isPool == false
                       && s.BracketName.Equals(bracketName)
                       group s by s.Round into g
                       select g;
            foreach (var round in sets)
            {
                rounds[round.Key.Value] = round.Select(s => s).ToList();
            }
            if (rounds.Keys.Count <= 1)
            {
                throw new SingleElimNotSupportedException();
            }
            winnersRounds = rounds.Keys.Max();
            losersRounds = rounds.Keys.Min();
            winnersBracket = new BinaryTree<Set>(rounds[winnersRounds].First());
            losersBracket = new BinaryTree<Set>(rounds[losersRounds].First());
            initializeBracket(winnersBracket, winnersRounds);
            initializeBracket(losersBracket, losersRounds);
            var winnersFinals = solveChampionships(winnersBracket, winnersRounds);
            buildWinnersBracket(winnersFinals);
            buildLosersBracket(losersBracket);
        }

        private void initializeBracket(BinaryTree<Set> bracket, int round)
        {
            if (round == 0) return;
            bool isWinners = Math.Sign(round) > 0;
            Set left = new Set() { WinnerID = -1, LoserID = -1, IsWinners = isWinners, Round = round };
            Set right = new Set() { WinnerID = -1, LoserID = -1, IsWinners = isWinners, Round = round };
            bracket.AddLeftChild(left);
            bracket.AddRightChild(right);
            initializeBracket(bracket.leftChild, round - Math.Sign(round));
            initializeBracket(bracket.rightChild, round - Math.Sign(round));
        }
        private BinaryTree<Set> solveChampionships(BinaryTree<Set> winnersBracket, int round)
        {
            if (rounds[round].Count > 1)
            {
                return winnersBracket;
            }
            winnersBracket.SetLeftValue(rounds[round].First());
            return solveChampionships(winnersBracket.leftChild, round - 1);

        }
        private void buildWinnersBracket(BinaryTree<Set> winnersBracket)
        {
            if (winnersBracket == null || winnersBracket.node == null || winnersBracket.node.Round == 1)
            {
                return;
            }
            int round = winnersBracket.node.Round.Value;
            int winnerId = winnersBracket.node.WinnerID;
            int loserId = winnersBracket.node.LoserID;
            Set leftSet = null;
            Set rightSet = null;
            bool buildLeftBracket = true;
            bool buildRightBracket = true;

            leftSet = rounds[round - 1].Where(s => s.WinnerID == winnerId).FirstOrDefault();
            rightSet = rounds[round - 1].Where(s => s.WinnerID == loserId).FirstOrDefault();
            if (leftSet == null)
            {
                leftSet = rounds[losersRounds].First();
                if (leftSet.WinnerID != winnerId)
                {
                    leftSet = null;
                }
                else
                {
                    buildLeftBracket = false;
                }
            }
            if (rightSet == null)
            {
                rightSet = rounds[losersRounds].First();
                if (rightSet.WinnerID != loserId)
                {
                    rightSet = null;
                }
                else
                {
                    buildRightBracket = false;
                }
            }

            if (leftSet == null && buildLeftBracket)
            {
                leftSet = new Set() { WinnerID = winnerId, LoserID = 0, Round = round - 1 };
            }
            if (rightSet == null && buildRightBracket)
            {
                rightSet = new Set() { WinnerID = loserId, LoserID = 0, Round = round - 1 };
            }

            if (buildLeftBracket)
            {
                winnersBracket.SetLeftValue(leftSet);
                buildWinnersBracket(winnersBracket.leftChild);
            }
            if (buildRightBracket)
            {
                winnersBracket.SetRightValue(rightSet);
                buildWinnersBracket(winnersBracket.rightChild);
            }
        }
        private void buildLosersBracket(BinaryTree<Set> bracket)
        {
            if (bracket == null || bracket.node == null || bracket.node.Round == -1)
            {
                return;
            }

            int round = bracket.node.Round.Value;
            int winnerId = bracket.node.WinnerID;
            int loserId = bracket.node.LoserID;
            bool buildLeftBracket = true;
            bool buildRightBracket = true;
            Set leftSet = null;
            Set rightSet = null;
            leftSet = rounds.ContainsKey(round + 1) ? rounds[round + 1].Where(s => s.WinnerID == winnerId).FirstOrDefault() : null;
            rightSet = rounds.ContainsKey(round + 1) ? rounds[round + 1].Where(s => s.WinnerID == loserId).FirstOrDefault() : null;
            if ((-1 * round) % 2 == 0) //even rounds have one child from winners, one from losers
            {
                if (leftSet == null && rightSet == null)
                {
                    leftSet = new Set() { WinnerID = winnerId, LoserID = 0, IsWinners = false, Round = round + 1 };
                }
                else if (leftSet == null && rightSet != null)
                {
                    buildLeftBracket = false;
                }
                else if (leftSet != null && rightSet == null)
                {
                    buildRightBracket = false;
                }
                else //leftSEt != null && rightSet != null
                {
                    //???
                }
                if (leftSet != null) bracket.SetLeftValue(leftSet);
                if (rightSet != null) bracket.SetRightValue(rightSet);
            }
            else //odd rounds have both children sets from losers
            {
                if (leftSet == null && rightSet == null)
                {
                    leftSet = new Set() { WinnerID = winnerId, LoserID = 0, Round = round + 1, IsWinners = false };
                    rightSet = new Set() { WinnerID = winnerId, LoserID = 0, Round = round + 1, IsWinners = false };
                }
                else if (leftSet == null && rightSet != null)
                {
                    leftSet = new Set() { WinnerID = winnerId, LoserID = 0, Round = round + 1, IsWinners = false };
                }
                else if (leftSet != null && rightSet == null)
                {

                    rightSet = new Set() { WinnerID = winnerId, LoserID = 0, Round = round + 1, IsWinners = false };
                }
                else //leftSEt != null && rightSet != null
                {
                }
                bracket.SetLeftValue(leftSet);
                bracket.SetRightValue(rightSet);
            }
            if (buildLeftBracket) buildLosersBracket(bracket.leftChild);
            if (buildRightBracket) buildLosersBracket(bracket.rightChild);
        }
    }
}