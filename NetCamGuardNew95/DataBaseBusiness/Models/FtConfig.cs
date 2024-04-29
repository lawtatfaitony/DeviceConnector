using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Config { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
