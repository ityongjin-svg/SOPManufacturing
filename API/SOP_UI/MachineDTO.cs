using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_UI
{
    public class MachineDTO
    {
    }
    public class MachineItemDTO
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public List<MachineTypeDTO> Types { get; set; }
        public string IteTypeNamemName { get; set; }
    }

    public class MachineTypeDTO
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public List<MachineFileDTO> Files { get; set; }
    }

    public class MachineFileDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
    public class MachineFlatDTO
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string ItemName { get; set; }
        public string TypeName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class Machine_Items
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string ItemName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
    public class Machine_Types
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string TypeName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class MachineFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileIcon { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}