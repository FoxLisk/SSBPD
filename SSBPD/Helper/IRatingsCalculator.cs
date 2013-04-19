using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;

namespace SSBPD.Helper
{
    public interface IRatingsCalculator
    {

        double getExpectedScore(double ratingA, double ratingB);
        double calculateRatingChangeForPlayer(RatingsPlayer player, IEnumerable<RatingsSet> sets, IEnumerable<RatingsPlayer> players);
    }
    public class RatingsSet
    {
        public int WinnerID { get; set; }
        public int LoserID { get; set; }
        public bool isDraw { get; set; }
    }

    public class RatingsPlayer
    {
        public int PlayerId { get; set; }
        public double ELO { get; set; }
    }
}