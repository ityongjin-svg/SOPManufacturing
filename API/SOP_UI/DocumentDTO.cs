using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class DocumentDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string PreviewPath { get; set; }
        public string FileType { get; set; }
        public int MenuId { get; set; }
        public int CategoryId { get; set; }

        public string Name { get; set; }
        public string MenuName { get; set; }
        public string CategoryName { get; set; }


    }
}