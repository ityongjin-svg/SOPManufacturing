using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_API.Models
{
    public class QCLayout
    {
    }
    public class QCStyleDTO
    {
        public int Id { get; set; }
        public string Buyer { get; set; }
        public string StyleName { get; set; }
        public string Item { get; set; }
        public string Sort { get; set; }
        public string Description { get; set; }
        public bool HasVideo { get; set; }
        public bool HasThumbnail { get; set; }
        public bool HasFile { get; set; }
        public string FileFileName { get; set; }
        public string Status { get; set; }
    }

    public class QCStyleDetailDTO
    {
        public int Id { get; set; }
        public string Buyer { get; set; }
        public string StyleName { get; set; }
        public string Item { get; set; }
        public string Sort { get; set; }
        public string Description { get; set; }
        public string VideoFileName { get; set; }
        public string FileFileName { get; set; }
    }
}