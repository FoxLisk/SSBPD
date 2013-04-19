using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.ViewModels;

namespace SSBPD.Controllers
{
    public class RegionController : BaseController
    {
        public RegionController()
        {
            ViewBag.CSSIncludes.Add("~/Content/Region.css");
            ViewBag.JavascriptIncludes.Add("~/Scripts/Region.js");
        }

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            List<CustomRegion> customRegions = new List<CustomRegion>();
            if (userId > 0)
            {
                customRegions = (from cr in db.CustomRegions
                                 where cr.UserID == userId
                                 select cr).ToList();
            }
            return View(new RegionIndexViewModel(customRegions));
        }

        public ActionResult RegionGroup()
        {
            IEnumerable<int> regionValues = null;
            try
            {
                regionValues = Request["regions"].Split(',').Select(r => Convert.ToInt32(r));
            }
            catch
            {
                return RedirectToAction("Index");
            }
            foreach (int regionValue in regionValues)
            {
                if (!Enum.IsDefined(typeof(Region), regionValue))
                {
                    return RedirectToAction("Index");
                }
            }
            var regionValuesSet = new HashSet<int>(regionValues);

            var playersInRegion = (from p in db.Players
                                   where p.RegionValue != null && regionValuesSet.Contains(p.RegionValue.Value)
                                   orderby p.ELO descending
                                   select p).ToList();
            var regions = regionValues.Select(r => (Region)r);
            var vm = new RegionGroupViewModel(regions, playersInRegion);
            return View(vm);

        }

        public ActionResult Versus()
        {
            IEnumerable<int> regionOneValues = null;
            IEnumerable<int> regionTwoValues = null;
            try
            {
                regionOneValues = Request["regionOne"].Split(',').Select(r => Convert.ToInt32(r));
                regionTwoValues = Request["regionTwo"].Split(',').Select(r => Convert.ToInt32(r));
            }
            catch
            {
                return RedirectToAction("Index");
            }
            foreach (int regionValue in regionOneValues.Union(regionTwoValues))
            {
                if (!Enum.IsDefined(typeof(Region), regionValue))
                {
                    return RedirectToAction("Index");
                }
            }
            return View(getViewModel(regionOneValues, regionTwoValues));
        }

        public ActionResult VersusSets()
        {
            IEnumerable<int> regionOneValues = null;
            IEnumerable<int> regionTwoValues = null;
            try
            {
                regionOneValues = Request["regionOne"].Split(',').Select(r => Convert.ToInt32(r));
                regionTwoValues = Request["regionTwo"].Split(',').Select(r => Convert.ToInt32(r));
            }
            catch
            {
                return RedirectToAction("Index");
            }
            foreach (int regionValue in regionOneValues.Union(regionTwoValues))
            {
                if (!Enum.IsDefined(typeof(Region), regionValue))
                {
                    return RedirectToAction("Index");
                }
            }
            return View(getViewModel(regionOneValues, regionTwoValues));
        }

        private VersusRegionViewModel getViewModel(IEnumerable<int> regionOneValues, IEnumerable<int> regionTwoValues) {
            

            var regionOneValuesSet = new HashSet<int>(regionOneValues);
            var regionTwoValuesSet = new HashSet<int>(regionTwoValues);
            var regionPlayers = from p in db.Players
                                where p.RegionValue != null && (regionOneValuesSet.Union(regionTwoValuesSet).Contains(p.RegionValue.Value))
                                orderby p.ELO descending
                                select p;
            var regionOnePlayerIds = new HashSet<int>(regionPlayers.Where(p => regionOneValuesSet.Contains(p.RegionValue.Value)).Select<Player, int>(p => p.PlayerId));
            var regionTwoPlayerIds = new HashSet<int>(regionPlayers.Where(p => regionTwoValuesSet.Contains(p.RegionValue.Value)).Select<Player, int>(p => p.PlayerId));
            
            var sets = from s in db.Sets
                       where (regionOnePlayerIds.Contains(s.WinnerID) && regionTwoPlayerIds.Contains(s.LoserID)) ||
                       (regionOnePlayerIds.Contains(s.LoserID) && regionTwoPlayerIds.Contains(s.WinnerID))
                       select s;

            int regionOneWins = sets.Where(s => !s.isDraw && regionOnePlayerIds.Contains(s.WinnerID)).Count();
            int regionTwoWins = sets.Where(s => !s.isDraw && regionOnePlayerIds.Contains(s.LoserID)).Count();
            int draws = sets.Where(s => s.isDraw).Count();
            var regionOnePlayers = from p in regionPlayers
                                   where regionOnePlayerIds.Contains(p.PlayerId)
                                   select p;

            var regionTwoPlayers = from p in regionPlayers
                                   where regionTwoPlayerIds.Contains(p.PlayerId)
                                   select p;
            var regionOneList = regionOneValues.Select(rv => (Region)rv);
            var regionTwoList = regionTwoValues.Select(rv => (Region)rv);
            return new VersusRegionViewModel(regionOneList, regionTwoList, regionOneWins, regionTwoWins, draws, regionOnePlayers, regionTwoPlayers, sets);

        }
    }
}
