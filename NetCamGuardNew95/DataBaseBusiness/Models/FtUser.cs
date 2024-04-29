using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtUser
    {
        public int Id { get; set; }
        public string MaincomId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
