using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class MenuDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameKr { get; set; }
        public int? ParentId { get; set; }  
        public string Url { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; } // tambahkan
        public string ParentName { get; set; } 

    }
}