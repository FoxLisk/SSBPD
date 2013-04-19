using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.Helper;
using SSBPD.ViewModels;
using System.Net.Mail;
using System.Configuration;

namespace SSBPD.Controllers
{
    public class UserController : BaseController
    {
        private UserAuthHelper _userHelper;
        public UserAuthHelper userHelper
        {
            get
            {
                if (_userHelper == null)
                {
                    userHelper = new UserAuthHelper();
                } return _userHelper;
            }
            set { _userHelper = value; }
        }
        //
        // GET: /User/
        public UserController()
        {
            ViewBag.JavascriptIncludes.Add("~/Scripts/User.js");
        }
        public ActionResult Index()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            User user = db.Users.Find(Session["userId"]);
            return View(new UserViewModel(user, ""));
        }
        public ActionResult CustomRegions()
        {
            int userId = Convert.ToInt32(Session["userId"]);
            if (userId <= 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var customRegions = (from cr in db.CustomRegions
                                 where cr.UserID == userId
                                 select cr).ToList();
            return View(new CustomRegionsViewModel(customRegions));
        }
        public ActionResult Update()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (Request.HttpMethod.ToUpper().Equals("GET"))
            {
                return RedirectToAction("Index");
            }

            int userId = Convert.ToInt32(Session["userId"]);
            string oldPassword = Request["oldPassword"];
            string newPassword = Request["newPassword"];
            string msg;
            User user = null;
            user = userHelper.getUser(userId, oldPassword);
            if (user == null)
            {
                msg = "You entered the wrong password - please try again.";
                user = db.Users.Find(userId);
                return View("Index", new UserViewModel(user, msg));
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                msg = "Please enter a new password.";
                return View("Index", new UserViewModel(user, msg));
            }
            bool success;
            try
            {
                success = userHelper.updateUser(userId, oldPassword, newPassword);
            }
            catch (PasswordTooWeakException e)
            {
                msg = "This password is too weak. Please select a password of at least 8 characters.";
                return View("Index", new UserViewModel(user, msg));
            }
            msg = success ? "Your password has been changed!" : "Password change attempt failed. Please try again.";
            return View("Index", new UserViewModel(user, msg));

        }
        public ActionResult ResetPassword()
        {
            return View();
        }
        public ActionResult SendResetEmail(string email)
        {
            User user = (from u in db.Users
                         where u.email.Equals(email)
                         select u).FirstOrDefault();
            if (user == null)
            {
                var noUser = new { response = "Invalid email" };
                return Json(noUser);
            }
            
            var disabledJson = new { response = "I'm sorry, but due to technical issues, we can't send password reset emails right now. Please try again later." };
            return Json(disabledJson);
             
            string newPassword = getRandomWord();
            var message = new MailMessage("noreply@ssbpd.com", user.email);
            message.Subject = "Your password at SSBPD has been reset.";
            message.Body = String.Format("Your password for the account {0} has been reset to {1}. Please login and change your password back to something you'll remember.",
                user.username, newPassword);
            message.Sender = new MailAddress("noreply@ssbpd.com", "SSBPD Admin");
            SmtpClient client = new SmtpClient();
            client.Send(message);
            userHelper.updateUser(user.UserID, newPassword);
            var json = new { response = "Check your email for your new temporary password" };
            return Json(json);
        }

        private string getRandomWord()
        {
            string pw = "";
            var words = getWords();
            Random r = new Random();
            for (int i = 0; i < 4; i++)
            {
                pw += words[r.Next(0, words.Count)];
            }
            return pw;
        }

        private List<string> getWords()
        {
            List<string> words;
            if (Application["passwordNouns"] != null)
            {
                words = (List<string>)Application["passwordNouns"];
            }
            else
            {
                words = new List<string>();
                var path = Server.MapPath("~/Files/passwordnouns.txt");
                foreach (string word in System.IO.File.ReadLines(path))
                {

                    words.Add(word);
                }
                Application["passwordNouns"] = words;
            }
            return words;
        }
    }
}
