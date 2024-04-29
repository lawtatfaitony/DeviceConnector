using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtTask
    {
        public int Id { get; set; }
        public string MaincomId { get; set; }
        public string Name { get; set; }
        public sbyte? Type { get; set; }
        public string CameraList1 { get; set; }
        public string CameraList2 { get; set; }
        public string LibList { get; set; }
        public int? Interval { get; set; }
        public float? Threshold { get; set; }
        public sbyte? State { get; set; }
        public string Plan { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
