using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class FlagController : BaseController
    {
        [HttpPost]
        public ActionResult AddPlayerFlag()
        {
            if (Convert.ToInt32(Session["userId"]) <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag players." };
                return Json(loggedOutJson);
            }
            string newTag = Request["newTag"];
            int? toPlayerID = Request["toPlayerId"].GetValueOrNull<int>();

            if (toPlayerID == null && string.IsNullOrEmpty(newTag))
            {
                var noNewNameJson = new { response = "Please specify what you think this player's correct name is." };
                return Json(noNewNameJson);
            }

            int playerId = Convert.ToInt32(Request["playerId"]);

            Player foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                var playerNotFoundJson = new { response = "Something went wrong. Please try again later." };
                return Json(playerNotFoundJson);
            }

            int userId = Convert.ToInt32(Session["userId"]);
            PlayerFlag foundFlag = (from pf in db.PlayerFlags
                                    where pf.userID == userId && pf.PlayerID == playerId
                                    select pf).FirstOrDefault();
            if (foundFlag != null)
            {
                var alreadyFlaggedJson = new { response = "You have already flagged this player." };
                return Json(alreadyFlaggedJson);
            }
            if (toPlayerID != null)
            {
                var toPlayer = db.Players.Find(toPlayerID);
                if (toPlayer != null)
                {
                    newTag = toPlayer.Tag;
                }
            }

            PlayerFlag newFlag = new PlayerFlag();
            newFlag.newTag = newTag;
            newFlag.PlayerID = playerId;
            newFlag.toPlayerID = toPlayerID;
            newFlag.userID = userId;
            db.PlayerFlags.Add(newFlag);
            db.SaveChanges();

            var savedJson = new { response = "Flag saved! Thank you." };
            return Json(savedJson);
        }

        [HttpPost]
        public ActionResult AddRegionFlag(int playerId)
        {

            if (Convert.ToInt32(Session["userId"]) <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag players." };
                return Json(loggedOutJson);
            }
            int regionValue;
            bool validRegion = int.TryParse(Request["regionValue"], out regionValue);
            validRegion = validRegion && Enum.IsDefined(typeof(Region), regionValue);
            if (!validRegion)
            {
                var noNewNameJson = new { response = "Please specify what region this player is from." };
                return Json(noNewNameJson);
            }

            Player foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                var playerNotFoundJson = new { response = "Something went wrong. Please try again later." };
                return Json(playerNotFoundJson);
            }
            if (regionValue == foundPlayer.RegionValue)
            {
                var alreadyRightJson = new { response = "This player is already in this region." };
                return Json(alreadyRightJson);
            }

            int userId = Convert.ToInt32(Session["userId"]);
            RegionFlag foundFlag = (from pf in db.RegionFlags
                                    where pf.userID == userId && pf.PlayerID == playerId
                                    select pf).FirstOrDefault();
            if (foundFlag != null)
            {
                var alreadyFlaggedJson = new { response = "You have already flagged this player." };
                return Json(alreadyFlaggedJson);
            }

            RegionFlag newFlag = new RegionFlag(playerId, userId, regionValue);

            db.RegionFlags.Add(newFlag);
            db.SaveChanges();

            var savedJson = new { response = "Suggestion taken! Thank you." };
            return Json(savedJson);
        }

        [HttpPost]
        public ActionResult AddCharacterFlagForSet(int setId)
        {
            if (Convert.ToInt32(Session["userId"]) <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag sets." };
                return Json(loggedOutJson);
            }
            int characterValue;
            bool validCharacter = int.TryParse(Request["characterValue"], out characterValue);
            validCharacter = validCharacter && Enum.IsDefined(typeof(Character), characterValue);
            if (!validCharacter)
            {
                var noCharacterJson = new { response = "Please specify what character was played in this set." };
                return Json(noCharacterJson);
            }
            var foundSet = db.Sets.Find(setId);
            if (foundSet == null)
            {
                var noSetJson = new { response = "Sorry, something went wrong. Please try again later." };
                return Json(noSetJson);
            }
            int userId = Convert.ToInt32(Session["userId"]);
            bool winnerFlag = Convert.ToBoolean(Request["winnerFlag"]);
            CharacterFlag foundFlag = (from cf in db.CharacterFlags
                                       where cf.UserID == userId && cf.SetID == setId && cf.WinnerFlag == winnerFlag
                                       select cf).FirstOrDefault();
            if (foundFlag != null)
            {
                var alreadyFlaggedJson = new { response = "You have already flagged this set." };
                return Json(alreadyFlaggedJson);
            }
            CharacterFlag newFlag = new CharacterFlag(userId, setId, winnerFlag, characterValue);
            db.CharacterFlags.Add(newFlag);
            db.SaveChanges();

            var savedJson = new { response = "Suggestion taken! Thank you." };
            return Json(savedJson);
        }
        [HttpPost]
        public ActionResult AddCharacterFlagForPlayer(int playerId)
        {

            if (Convert.ToInt32(Session["userId"]) <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag players." };
                return Json(loggedOutJson);
            }
            int characterValue;
            bool validCharacter = int.TryParse(Request["characterValue"], out characterValue);
            validCharacter = validCharacter && Enum.IsDefined(typeof(Character), characterValue);
            if (!validCharacter)
            {
                var noCharacterJson = new { response = "Please specify what character this player mains." };
                return Json(noCharacterJson);
            }
            var foundPlayer = db.Players.Find(playerId);
            if (foundPlayer == null)
            {
                var noPlayerJson = new { response = "Sorry, something went wrong. Please try again later." };
                return Json(noPlayerJson);
            }
            int userId = Convert.ToInt32(Session["userId"]);
            CharacterFlag foundFlag = (from cf in db.CharacterFlags
                                       where cf.UserID == userId && cf.PlayerID == playerId
                                       select cf).FirstOrDefault();
            if (foundFlag != null)
            {
                var alreadyFlaggedJson = new { response = "You have already flagged this player." };
                return Json(alreadyFlaggedJson);
            }
            CharacterFlag newFlag = new CharacterFlag(userId, playerId, characterValue);
            db.CharacterFlags.Add(newFlag);
            db.SaveChanges();

            var savedJson = new { response = "Suggestion taken! Thank you." };
            return Json(savedJson);

        }

        [HttpPost]
        public ActionResult AddSetLinkFlag(int setLinkId)
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                var loggedOutJson = new { response = "Must be logged in to flag players." };
                return Json(loggedOutJson);
            }
            var setLink = db.SetLinks.Find(setLinkId);
            if (setLink == null)
            {
                var errorJson = new { response = "Something went wrong. Please try again later." };
                return Json(errorJson);
            }

            var setLinkFlag = from slf in db.SetLinkFlags
                              where slf.SetLinkID == setLinkId && slf.userID == userId
                              select slf;
            if (setLinkFlag.Count() > 0)
            {
                var alreadyFlaggedJson = new { response = "Reported. Thank you." };
                return Json(alreadyFlaggedJson);
            }

            var newFlag = new SetLinkFlag();
            newFlag.SetLinkID = setLinkId;
            newFlag.userID = userId;

            db.SetLinkFlags.Add(newFlag);
            db.SaveChanges();
            var json = new { response = "Reported. Thank you." };
            return Json(json);
        }
    }
    public static class StringExtensions
    {

        public static T? GetValueOrNull<T>(this string valueAsString) where T : struct
        {
            if (string.IsNullOrEmpty(valueAsString))
                return null;
            return (T)Convert.ChangeType(valueAsString, typeof(T));
        }
    }

}
