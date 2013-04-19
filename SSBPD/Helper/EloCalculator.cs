using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.Helper
{
    public class EloCalculator : IRatingsCalculator
    {
        public double calculateRatingChangeForPlayer(RatingsPlayer player, IEnumerable<RatingsSet> sets, IEnumerable<RatingsPlayer> players)
        {
            var playerSets = (from s in sets
                              where s.WinnerID == player.PlayerId || s.LoserID == player.PlayerId
                              select s).Distinct();
            HashSet<int> playerOpponentIDs = new HashSet<int>();
            foreach (RatingsSet set in playerSets)
            {
                playerOpponentIDs.Add(set.WinnerID);
                playerOpponentIDs.Add(set.LoserID);
            }
            playerOpponentIDs.Remove(player.PlayerId);
            var playerOpponents = from p in players
                                  where playerOpponentIDs.Contains(p.PlayerId)
                                  select p;

            double expectedScore = getExpectedScoreForTournament(player, playerOpponents, playerSets);
            double actualScore = getActualScore(playerSets, player.PlayerId);
            var ratingChange = getRatingChange(getKFactor(player.ELO), actualScore, expectedScore);
            playerSets = null;
            GC.Collect();
            return ratingChange;
        }

        public double getExpectedScore(double EloA, double EloB) //that player with EloA beats EloB
        {

            double exponent = (EloB - EloA) / 400.0;
            return 1.0 / (1 + Math.Pow(10.0, exponent));
        }


        private double getRatingChange(double kFactor, double actualScore, double expectedScore)
        {
            return kFactor * (actualScore - expectedScore);
        }

        private double getExpectedScoreForTournament(RatingsPlayer player, IEnumerable<RatingsPlayer> playerOpponents, IEnumerable<RatingsSet> playerSets)
        {
            double expectedScore = 0;
            foreach (RatingsSet set in playerSets)
            {
                var opponent = set.WinnerID == player.PlayerId ? playerOpponents.First(p => p.PlayerId == set.LoserID) :
                                                                    playerOpponents.First(p => p.PlayerId == set.WinnerID);
                expectedScore += getExpectedScore(player.ELO, opponent.ELO);
            }
            return expectedScore;
        }

        private double getKFactor(double playerELO)
        {
            if (playerELO <= 2100.0)
            {
                return 32;
            }
            else if (playerELO <= 2400.0)
            {
                return 24;
            }
            else
            {
                return 16;
            }
        }

        private double getActualScore(IEnumerable<RatingsSet> playerSets, int playerId)
        {
            double actualScore = playerSets.Where(s => !s.isDraw).Count(s => s.WinnerID == playerId);
            actualScore += playerSets.Where(s => s.isDraw).Count() * 1.0 / 2;
            return actualScore;
        }
    }
}