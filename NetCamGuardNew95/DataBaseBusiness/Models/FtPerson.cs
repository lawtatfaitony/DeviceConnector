using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtPerson
    {
        public long Id { get; set; }
        public string MaincomId { get; set; }
        public string OuterId { get; set; }
        public int LibId { get; set; }
        public string LibIdGroups { get; set; }
        public string Name { get; set; }
        public sbyte? Sex { get; set; }
        public string CardNo { get; set; } 
        public string PassKey { get; set; }
        public string Phone { get; set; }
        public sbyte Category { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
