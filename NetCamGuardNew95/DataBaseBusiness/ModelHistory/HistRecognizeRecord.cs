using System;
using System.Collections.Generic;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistRecognizeRecord
    {
        public long Id { get; set; }
        public sbyte Mode { get; set; }
        public string MaincomId { get; set; }
        public long OccurDatetime { get; set; }
        public int? DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int? TaskId { get; set; }
        public string TaskName { get; set; }
        public long? CameraId { get; set; }
        public string CameraName { get; set; }
        public long? PersonId { get; set; }
        public string PersonName { get; set; }
        public sbyte? Sex { get; set; }
        public string CardNo { get; set; }
        public sbyte? Category { get; set; }
        public int? LibId { get; set; }
        public string LibName { get; set; }
        public sbyte? Classify { get; set; }
        public string PicPath { get; set; }
        public string CapturePath { get; set; }
        public float? Similarity { get; set; }
        public string Remark { get; set; }
        public sbyte? Visible { get; set; }
        public DateTime? CaptureTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
