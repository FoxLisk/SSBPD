using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSBPD.Models;

namespace SSBPD.ViewModels
{
    public class UserViewModel
    {
        public User user;
        public string msg;
        public UserViewModel(User user, string msg)
        {
            this.user = user;
            this.msg = msg;
        }
    }
}
