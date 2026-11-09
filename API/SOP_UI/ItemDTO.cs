using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class ItemDTO
    {
        public int Id { get; set; }
        public string ItemTitle { get; set; }
        public string FilePath { get; set; }
        public string GroupName { get; set; }
        public string SectionName { get; set; }
        public int MenuId { get; set; }
        public int SectionId { get; set; }
        public int GroupId { get; set; }
        public string CategoryId { get; set; }
    }
}