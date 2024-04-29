using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtDoor
    {
        public int DoorId { get; set; }
        public string DoorName { get; set; }
        public string MaincomId { get; set; }
        public int? DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
