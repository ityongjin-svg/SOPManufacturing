using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class SectionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int MenuId { get; set; }
        public int SortOrder { get; set; }

    }
}