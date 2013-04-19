using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.ViewModels;
using SSBPD.Helper;

namespace SSBPD.Controllers
{
    public class SetController : BaseController
    {
        public ActionResult UpdateSet(int setId)
        {
            if (!Convert.ToBoolean(Session["userAdmin"]) && !Convert.ToBoolean(Session["userModerator"]))
            {
                var unauthorizedJson = new { response = "You are not authorized to change this." };
                return Json(unauthorizedJson);
            }
            Set set = db.Sets.Find(setId);
            if (set == null)
            {
                var fuckupJson = new { response = "Sorry, something went wrong. Please try again later." };
                return Json(fuckupJson);
            }
            Player winner = db.Players.Find(set.WinnerID);
            Player loser = db.Players.Find(set.LoserID);
            if (Convert.ToBoolean(Request["switchWinner"]))
            {
                int tmpWinner = set.WinnerID;
                var tmpWinnerChar = set.WinnerCharacter;
                set.WinnerID = set.LoserID;
                set.WinnerCharacter = set.LoserCharacter;
                set.LoserID = tmpWinner;
                set.LoserCharacter = tmpWinnerChar;
            }
            if (Convert.ToBoolean(Request["toDraw"]))
            {
                set.isDraw = true;
            }
            int winnerChar;
            bool isWinnerChar = int.TryParse(Request["winnerChar"], out winnerChar);
            if (isWinnerChar && Enum.IsDefined(typeof(Character), winnerChar))
            {
                set.WinnerCharacter = (Character)winnerChar;
            }
            int loserChar;
            bool isLoserChar = int.TryParse(Request["loserChar"], out loserChar);
            if (isLoserChar && Enum.IsDefined(typeof(Character), loserChar))
            {
                set.LoserCharacter = (Character)loserChar;
            }
            db.SaveChanges();
            deleteFlags(setId, isWinnerChar, isLoserChar);
            var json = new { response = "Changes saved. Thank you!" };
            return Json(json);
        }

        private void deleteFlags(int setId, bool winner, bool loser)
        {
            if (winner)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM CharacterFlags WHERE SetID = {0} AND WinnerFlag = 1", setId);
            }
            if (loser)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM CharacterFlags WHERE SetID = {0} AND WinnerFlag = 0", setId);
            }

        }

        public ActionResult AddVideoLink(int setId)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag sets." };
                return Json(loggedOutJson);
            }
            string videoURL = Request["url"];
            string videoTitle = Request["title"];
            if (String.IsNullOrWhiteSpace(videoURL))
            {
                var missingJson = new { response = "Please enter a complete url." };
                return Json(missingJson);
            }

            if (String.IsNullOrWhiteSpace(videoTitle))
            {
                var missingJson = new { response = "Please enter a short description." };
                return Json(missingJson);
            }

            if (!URLHelper.validateURL(videoURL))
            {
                var invalidLinkJson = new { response = "Please input a valid link." };
                return Json(invalidLinkJson);
            }
            if (!canUserAddLinks(userId))
            {
                var userErrorJson = new { response = "Sorry, something went wrong. Please try again later." };
                return Json(userErrorJson);
            }

            SetLink newLink = new SetLink();
            newLink.Deleted = false;
            newLink.SetID = setId;
            newLink.Title = videoTitle;
            newLink.URL = videoURL;
            newLink.UserID = userId;
            db.SetLinks.Add(newLink);
            db.SaveChanges();
            var json = new { response = "Successfully added! Thank you :D" };
            return Json(json);
        }

        public ActionResult Detail(int setId)
        {
            Set set = db.Sets.Find(setId);
            if (set == null)
            {
                return View("Error");
            }
            Player winner = db.Players.Find(set.WinnerID);
            Player loser = db.Players.Find(set.LoserID);
            var videos = from sl in db.SetLinks
                         where sl.SetID == setId && !sl.Deleted
                         select sl;
            
            var vm = new SetDetailViewModel(set, winner, loser, videos, "");
            if (Convert.ToBoolean(Session["userAdmin"]) || Convert.ToBoolean(Session["userModerator"]))
            {
                return View("Moderator",vm);
            }
            else
            {
                return View(vm);
            }
        }

        private bool canUserAddLinks(int userId)
        {
            var links = from sl in db.SetLinks
                        where sl.UserID == userId
                        select new { del = sl.Deleted };
            int deleted = links.Count(s => s.del);
            int undeleted = links.Count(s => !s.del);
            int total = deleted + undeleted;
            if (total >= 5 && ((double)deleted / (double)total) > .50)
            {
                return false;
            }
            return true;
        }
    }
}
