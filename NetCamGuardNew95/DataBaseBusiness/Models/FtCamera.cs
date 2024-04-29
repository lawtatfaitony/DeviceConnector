using System;
using System.Collections.Generic;

namespace DataBaseBusiness.Models
{
    public partial class FtCamera
    {
        public int Id { get; set; }
        public string MaincomId { get; set; }
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Rtsp { get; set; }
        public string Ip { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public sbyte? Type { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string DeviceSerialNo { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public sbyte? RecordStatus { get; set; }
        public ulong? OnLive { get; set; }
    }
}
