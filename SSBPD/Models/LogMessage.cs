using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class LogMessage
    {
        [Required]
        public int LogMessageID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Message { get; set; }

        public LogMessage()
        {
            Date = DateTime.Now;
        }
    }
}