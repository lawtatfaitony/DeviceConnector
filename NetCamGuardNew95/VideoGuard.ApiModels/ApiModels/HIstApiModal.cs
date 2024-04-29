using EnumCode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VideoGuard.ApiModels
{
    public class LiveRoomInitialize
    {
        //----------------------------------------------------
        [JsonProperty("currentIndexId")]
        public long CurrentIndexId { get; set; }

        [JsonProperty("currentIndexTimeStamp")]
        public long CurrentIndexTimeStamp { get; set; }

        [JsonProperty("currentIndexItem")]
        public HistoryLiveRoom CcurrentIndexItem { get; set; }

        [JsonProperty("listTop25")]
        public List<HistoryLiveRoom> ListTop25 { get; set; }
    }

    public class HistoryLiveRoom : HistRecognizeRecord
    {
        [JsonProperty("currentIndexTimeStamp")]
        public long CurrentIndexTimeStamp { get; set; }

        [JsonProperty("occurTimeSpan")]
        public long OccurTimeSpan { get; set; }

        [JsonProperty("historyLiveRoom")]
        public HistoryLiveRoom historyLiveRoom { get; set; }

        public DateTime ConvetToDateTime()
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            DateTime date = start.AddMilliseconds(OccurTimeSpan).ToLocalTime();
            return date;
        }
    }
    public partial class HistRecognizeRecord
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("taskId")]
        public int? TaskId { get; set; }

        [JsonProperty("taskName")]
        public string TaskName { get; set; }

        [JsonProperty("cameraId")]
        public long? CameraId { get; set; }

        [JsonProperty("cameraName")]
        public string CameraName { get; set; }

        [JsonProperty("personId")]
        public long? PersonId { get; set; }

        [JsonProperty("personName")]
        public string PersonName { get; set; }

        [JsonProperty("sex")]
        // 0 = unkonwn
        [DefaultValue(0)]
        public sbyte? Sex { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("category")]
        public sbyte? Category { get; set; }

        [JsonProperty("libId")]
        public int? LibId { get; set; }

        [JsonProperty("libName")]
        public string LibName { get; set; }

        [JsonProperty("classify")]
        // 0 = stranger
        [DefaultValue(0)]
        public sbyte? Classify { get; set; }

        [JsonProperty("picPath")]
        public string PicPath { get; set; }

        [JsonProperty("capturePath")]
        public string CapturePath { get; set; }

        [JsonProperty("similarity")]
        public float? Similarity { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("visible")]
        // 0 = invisible false =0
        [DefaultValue(0)]
        public sbyte? Visible { get; set; }

        [JsonProperty("captureTime")]
        public DateTime? CaptureTime { get; set; }

        [JsonProperty("createTime")]
        public DateTime CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public DateTime UpdateTime { get; set; }
    }

    public class HistRecognizeRecordIdInput
    {
        [JsonProperty("histRecognizeRecordId")]
        public long HistRecognizeRecordId { get; set; }
    }

    public class CurrentIndexTimeStampInput
    {
        [JsonProperty("currentIndexTimeStamp")]
        public long CurrentIndexTimeStamp { get; set; }
    }

    public class RetNewHistoryLiveRoom
    {
        [JsonProperty("currentIndexTimeStamp")]
        public long CurrentIndexTimeStamp { get; set; }

        [JsonProperty("currentIndexId")]
        public long CurrentIndexId { get; set; }

        [JsonProperty("historyLiveRoom")]
        public HistoryLiveRoom HistoryLiveRoom { get; set; }
    }

    public class HistListInput
    {
        public HistListInput()
        {
            Page = 1;
            PageSize = 100;
            TotalCount = 0;
            Search = string.Empty;
            DeviceId = 0;
        }
        public string OccurDateTimeRange { get; set; }
        public string Search { get; set; }
        [DefaultValue(0)]
        public int Page { get; set; }
        [DefaultValue(100)]
        public int PageSize { get; set; }
        [DefaultValue(0)]
        public int TotalCount { get; set; }
        public string MaincomId { get; set; }

        [DefaultValue(0)]
        public int DeviceId { get; set; }
    }
}
