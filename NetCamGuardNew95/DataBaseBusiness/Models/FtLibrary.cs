using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtLibrary
    {
        public int Id { get; set; }
        public string MaincomId { get; set; }
        public int LibId { get; set; }
        public string Name { get; set; }
        public int? PersonCount { get; set; }
        public sbyte? Type { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
