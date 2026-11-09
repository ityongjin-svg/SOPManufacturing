using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_API.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string PreviewPath { get; set; }


        public int MenuId { get; set; }
        public string Category { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public int CategoryId { get; set; }
    }
}