using System;
using System.Collections.Generic;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistAlarm
    {
        public ulong HistAlarmId { get; set; }
        public string MaincomId { get; set; }
        public int? TaskId { get; set; }
        public int? AlarmLevel { get; set; }
        public string TaskName { get; set; }
        public int? TaskType { get; set; }
        public string TaskTypeDesc { get; set; }
        public int? CameraId { get; set; }
        public string CameraName { get; set; }
        public string ObjName { get; set; }
        public string ObjShortDesc { get; set; }
        public string ObjJsonData { get; set; }
        public long? OccurDatetime { get; set; }
        public DateTime? CaptureTime { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CapturePath { get; set; }
        public double Threshold { get; set; }
        public string Remark { get; set; }
    }
}
