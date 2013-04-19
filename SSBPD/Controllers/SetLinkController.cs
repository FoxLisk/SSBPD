using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class LeaderBoardUser
    {
        public string username { get; set; }
        public int taggedVids { get; set; }
    }
    public class SetLinkController : BaseController
    {

        public ActionResult LeaderBoard()
        {
            if (!Convert.ToBoolean(Session["userAdmin"]))
            {
                return RedirectToAction("Index", "Home");
            }
            Dictionary<string, int> results = new Dictionary<string, int>();
            var userScores = from sl in db.SetLinks
                        group sl by sl.UserID into slg
                        select slg;
            var userIds = new HashSet<int>(userScores.Select(u => u.Key));
            var users = (from u in db.Users
                        where userIds.Contains(u.UserID)
                        select u).ToDictionary(u => u.UserID, u => u.username);

            results = userScores.ToDictionary(g => users[g.Key], g => g.Count());
            return View(results);
        }

        [HttpPost]
        public ActionResult DeleteSetLink(int setLinkId)
        {
            if (!Convert.ToBoolean(Session["userModerator"]))
            {
                var unauthorizedJson = new {response = "You are not authorized to delete links."};
                return Json(unauthorizedJson);
            }
            var setLink = db.SetLinks.Find(setLinkId);
            if (setLink == null){
                var errorJson = new {response = "Something went wrong. Please try again later."};
                return Json(errorJson);
            }
            setLink.Deleted = true;
            db.SaveChanges();
            db.Database.ExecuteSqlCommand("DELETE FROM SetLinkFlags WHERE SetLinkID = {0}", setLinkId);
            var json = new { response = "Deleted!" };
            return Json(json);
        }

        [HttpPost]
        public ActionResult RenameSetLink(int setLinkId)
        {

            if (!Convert.ToBoolean(Session["userModerator"]))
            {
                var unauthorizedJson = new { error = "You are not authorized to delete links." };
                return Json(unauthorizedJson);
            }

            var setLink = db.SetLinks.Find(setLinkId);
            if (setLink == null)
            {
                var errorJson = new { error = "Something went wrong. Please try again later." };
                return Json(errorJson);
            }
            string newTitle = Request["newTitle"];
            if (String.IsNullOrWhiteSpace(newTitle))
            {
                var errorJson = new { error = "Please specify a new link title." };
                return Json(errorJson);
            }
            setLink.Title = newTitle;
            db.SaveChanges();
            var json = new { response = "Renamed." };
            return Json(json);
        }
    }
}
