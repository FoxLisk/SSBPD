using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class User
    {
        [Required]
        public int UserID { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string salt { get; set; }
        [Required]
        public bool isAdmin { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public bool isModerator { get; set; }
        [Required]
        public Guid UserGuid { get; set; }
    }
}