using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtDevicePerson
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public long PersonId { get; set; }
        public string PersonName { get; set; }
        public int LibId { get; set; }
        public string LibName { get; set; }
        public string MaincomId { get; set; }
        public int DownInsertStatus { get; set; }
        public DateTime DownInsertStatusDt { get; set; }
        public int DownUpdateStatus { get; set; }
        public DateTime DownUpdateStatusDt { get; set; }
        public int DownDelStatus { get; set; }
        public DateTime DownDelStatusDt { get; set; }
        public string SynchronizedStatusRemark { get; set; }
    }
}
