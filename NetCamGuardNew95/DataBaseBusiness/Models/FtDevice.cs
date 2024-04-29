using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtDevice
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int? DeviceType { get; set; }
        public string MaincomId { get; set; }
        public int SiteId { get; set; }
        public int? LibId { get; set; }
        public string LibName { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceConfig { get; set; }
        public ulong IsReverseHex { get; set; }
        public int DeviceEntryMode { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
    }
}
