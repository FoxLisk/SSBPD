using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SSBPD.Models
{
    public class Image
    {
        [Required]
        public int ImageID { get; set; }
        [Required]
        [MaxLength]
        public byte[] ImageBytes { get; set; }
        [Required]
        public string FileName { get; set; }

        public string MimeType { get; set; }
    }
}