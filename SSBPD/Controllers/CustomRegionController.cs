using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class CustomRegionController : BaseController
    {
        public ActionResult Delete(int customRegionId)
        {
            var userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                var loggedOutJson = new { response = "You have been logged out. Please log back in to delete this.", success = false };
                return Json(loggedOutJson);
            }
            CustomRegion cr = db.CustomRegions.Find(customRegionId);
            if (cr == null)
            {
                var missingJson = new { response = "Something went wrong. Please try again later.", success = false };
                return Json(missingJson);
            }
            if (cr.UserID != userId)
            {
                var illegalJson = new { response = "You're trying to delete someone else's custom region. Don't be a douche.", success = false };
                return Json(illegalJson);
            }
            db.CustomRegions.Remove(cr);
            db.SaveChanges();
            var successJson = new { response = "Successfully deleted!", success = true };
            return Json(successJson);
        }

        public ActionResult Create()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                var notLoggedInJson = new { response = "Please log in", success = false };
                return Json(notLoggedInJson);
            }
            List<Region> regionValues = null;
            try
            {
                regionValues = Request["regions"].Split(',').Select(r => (Region)Convert.ToInt32(r)).ToList();
            }
            catch
            {
                var errorJson = new { response = "Something went wrong, sorry. Please try again later.", success = false };
                return Json(errorJson);
            }
            if (regionValues.Count == 0)
            {
                var emptyJson = new { response = "Please assign some regions to this region group!", success = false };
                return Json(emptyJson);
            }
            string name = Request["name"];
            if (string.IsNullOrWhiteSpace(name))
            {
                var noNameJson = new { response = "Please name this Region group", success = false };
                return Json(noNameJson);
            }

            CustomRegion customRegion = new CustomRegion(name, regionValues, userId);
            db.CustomRegions.Add(customRegion);
            db.SaveChanges();
            var successJson = new { success = true };
            return Json(successJson);
        }

    }
}
