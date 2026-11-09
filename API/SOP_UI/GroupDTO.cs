using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class GroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SectionName { get; set; }
        public int MenuId { get; set; }
        public int SectionId { get; set; }
    }
}