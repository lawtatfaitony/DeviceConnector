using Common;
using EnumCode;
using System;

namespace VideoGuard.Business
{
    public class HistoryRecordsDefault
    {
        public HistoryRecordsDefault()
        {
            string unkown = "UNKNOWN";
            long occur = DateTimeHelp.ToUnixTimeMilliseconds(DateTime.Now); //默認以發生時間的unix time作為id
            Id = occur;
            Mode = (int)EnumBusiness.AttendanceMode.CAM_GUARD;
            MaincomId = string.Empty; 
            DeviceId = 0;
            DeviceName = unkown;
            TaskId = 0;
            TaskName = unkown ;
            CameraId = 0;
            CameraName = unkown;
            PersonId = 0;
            PersonName = unkown;
            Sex = (sbyte)EnumBusiness.Genders.Unkown;
            CardNo = string.Empty;
            Category = 0;
            PicPath = string.Empty;
            CapturePath = string.Empty;
            Similarity = 0;
            Visible = (sbyte)GeneralVisible.VISIBLE;
            LibId = 0;
            LibName = string.Empty;
            Classify = 0; //stranger 陌生人
            Remark = string.Empty;
            CaptureTime = DateTime.Now;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            OccurDatetime = occur;
        }
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
        public DateTime CaptureTime { get; set; } 
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
