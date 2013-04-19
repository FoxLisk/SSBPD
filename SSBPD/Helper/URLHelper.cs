using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace SSBPD.Helper
{
    /**
     * I really hate to do this but there doesn't appear to be a convenient library for checking
     * if a url has any totally illegal characters.
     */
    public class URLHelper
    {
        private static Regex URLRegex = new Regex("https?://.*");
        public static char[] illegalChars = ":%.+/#?".ToCharArray();

        public static bool validateURL(string URL){
            return URLRegex.IsMatch(URL);
        }
    }
}