using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Helper;
using SSBPD.Models;

namespace SSBPD.Controllers
{
    public class LoginController : Controller
    {
        private UserAuthHelper _userAuthHelper;
        public UserAuthHelper userAuthHelper
        {
            get
            {
                if (_userAuthHelper == null)
                {
                    _userAuthHelper = new UserAuthHelper();
                }
                return _userAuthHelper;
            }
            set
            {
                _userAuthHelper = value;
            }
        }
        [HttpPost]
        public ActionResult LogIn()
        {
            string username = Request["username"];
            string password = Request["password"];
            User user = userAuthHelper.getUser(username, password);
            bool loginSuccessful = false;
            if (user != null)
            {
                Session["userId"] = user.UserID;
                loginSuccessful = true;

                if (user.isAdmin)
                {
                    Session["userAdmin"] = true;
                }
                if (user.isModerator)
                {
                    Session["userModerator"] = true;
                }
                bool createCookie = Convert.ToBoolean(Request["stayLoggedIn"]);
                if (createCookie)
                {
                    var userCookie = new HttpCookie("userID", user.UserGuid.ToString());
                    userCookie.Expires = DateTime.Now.AddYears(5);
                    Response.Cookies.Add(userCookie);
                }
            }


            if (!loginSuccessful)
            {
                ViewBag.LoginError = "Your username or password was incorrect.";
                return PartialView("LogIn");
            }

            return PartialView("LogOut");
        }
        
        [HttpPost]
        public ActionResult LogOut()
        {
            Session["UserAdmin"] = false;
            Session["userAuthorizedUploader"] = false;
            Session["userId"] = null;
            Session["userModerator"] = false;

            var json = new
            {
                logoutSuccessful = "true"
            };

            if (Request.Cookies["userID"] != null)
            {
                var nullCookie = new HttpCookie("userID");
                nullCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(nullCookie);
            }

            return PartialView("LogIn");
        }

        [HttpGet]
        public ActionResult CreateAccountDialog()
        {
            return PartialView("CreateAccount");
        }

        [HttpPost]
        public ActionResult CreateAccount()
        {
            string username = Request["username"];
            string password = Request["password"];
            string email = Request["emailAddress"];
            bool accountCreated = false;
            User newUser = null;
            try
            {
                newUser = userAuthHelper.createUser(username, password, email);
                accountCreated = true;
            }
            catch (InvalidEmailException e)
            {
                ViewBag.CreateAccountError = "This is not a valid email.";
            }
            catch (UserExistsException e)
            {
                ViewBag.CreateAccountError = "This username is already in use.";
            }
            catch (EmailExistsException e)
            {
                ViewBag.CreateAccountError = "This email is already in use.";
            }
            catch (PasswordTooWeakException e)
            {
                ViewBag.CreateAccountError = "This password is too weak. Please select a password of at least 8 characters.";
            }

            if (!accountCreated) {
                return PartialView("CreateAccount");
            }
            Session["userId"] = newUser.UserID;

            bool createCookie = Convert.ToBoolean(Request["stayLoggedIn"]);
            if (createCookie)
            {
                var userCookie = new HttpCookie("userID", newUser.UserGuid.ToString());
                userCookie.Expires = DateTime.Now.AddYears(5);
                Response.Cookies.Add(userCookie);
            }
            return PartialView("LogOut");
        }
    }
}
