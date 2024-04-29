using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtSite
    {
        public int SiteId { get; set; }
        public string MaincomId { get; set; }
        public int? ParentsId { get; set; }
        public string SiteName { get; set; }
        public int CameraCount { get; set; }
        public int PersonCount { get; set; }
        public string Address { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
