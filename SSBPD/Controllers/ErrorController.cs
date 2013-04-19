using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SSBPD.Helper;
using System.IO;

namespace SSBPD.Controllers
{
    public class ErrorController : BaseController
    {
        private const int MAX_DISTANCE = 2;

        public ActionResult Error(string path)
        {
            var routes = RouteTable.Routes.ToList();
            List<string> urls = new List<string>();
            foreach (Route route in routes)
            {
                urls.Add(route.Url);
            }
            string[] pathParts = path.Split(new char[] { '/' }, 2);
            string basePath = pathParts[0];
            string pathSuffix = null;
            if (pathParts.Length > 1)
            {
                pathSuffix = pathParts[1];
            }

            var possibleMatches = new HashSet<string>();

            foreach (string url in urls)
            {
                string routeBasePath = url.Split('/')[0];
                int distance = LevenshteinDistance.Compute(basePath, routeBasePath);
                if (distance <= MAX_DISTANCE)
                {
                    string urlToAdd = routeBasePath;
                    if (pathSuffix != null)
                    {
                        urlToAdd += '/' + pathSuffix;
                    }
                    possibleMatches.Add(urlToAdd);
                }
            }

            ViewBag.Title = path;
            ViewBag.Matches = possibleMatches.Count > 0 ? possibleMatches : null;
            return View();
        }
        
        [HandleError]
        public ActionResult InternalException()
        {
            return View();
        }
    }
}
