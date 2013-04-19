using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;
using SSBPD.Helper;

namespace SSBPD.Controllers
{
    public abstract class BaseController : Controller
    {
        internal BaseController()
        {
            ViewBag.IsUserAdmin = false;
            ViewBag.JavascriptIncludes = new List<string>();
            ViewBag.JavascriptIncludes.Add("~/Scripts/ga.js");
            ViewBag.CSSIncludes = new List<string>();
            ViewBag.JavascriptFooterIncludes = new List<string>();
        }

        ~BaseController()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
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

        public System.Web.HttpApplicationState Application
        {

            get
            {
                return System.Web.HttpContext.Current.Application;
            }
        }

        protected void log(string message)
        {
            using (var db = new SSBPDContext())
            {
                db.LogMessages.Add(new LogMessage()
                {
                    Message = message
                });
                db.SaveChanges();
            }
        }

    }
}
