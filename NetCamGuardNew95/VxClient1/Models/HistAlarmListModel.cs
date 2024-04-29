using DataBaseBusiness.ModelHistory;
using EnumCode;
using System.ComponentModel;

namespace VxClient.Models
{
    public class HistAlarmListInput
    {
        public HistAlarmListInput()
        {
            Page = 1;
            PageSize = 100;
            TotalCount = 0;
            Search = string.Empty;
            CameraId = 0;
        }
        public string MaincomId { get; set; }
        public string OccurDateTimeRange { get; set; }
        public string Search { get; set; }
        [DefaultValue(0)]
        public int Page { get; set; }
        [DefaultValue(100)]
        public int PageSize { get; set; }
        [DefaultValue(0)]
        public int TotalCount { get; set; }

        [DefaultValue(0)]
        public int CameraId { get; set; }

        /// <summary>
        /// 任務類型
        /// </summary>
        public TaskType TaskType { get; set; }
    }

    /// <summary>
    /// 用於AI識別後提交結果到雲端 
    /// 以下 int?必須設置默認值=0表示無ID傳入
    /// </summary>
    //<param>HistAlarmId 傳入默認值=0</param>
    //<param>MaincomId 必須傳入公司ID</param>
    //<param>int? TaskId 默認=0 </param>
    //<param>int? AlarmLevel 默認消息警報級別則設置0 具體警報級別： NOT_APPLICABLE = -1,INFO_ALARM = 0， CRISIS_ALARM = 1，EMERGENCY_ALARM = 2</param>
    //<param>TaskName</param>
    //<param>int? TaskType</param>
    //<param>string TaskTypeDesc</param>
    //<param>int? CameraId</param>
    //<param>string CameraName</param>
    //<param>string ObjName</param>
    //<param>ObjShortDesc</param>
    //<param>ObjJsonData</param>
    //<param>OccurDatetime</param>
    //<param>CaptureTime</param>
    //<param>CreateTime</param>
    //<param>CapturePath</param>
    //<param>Threshold</param>
    //<param>Remark</param>
    public class HistAlarmModel : HistAlarm
    {
        public HistAlarmModel()
        {
            HistAlarmId = 0;  //INSERT時候，默認設置0
        }
        /// <summary>
        /// 以Base64格式保存原圖
        /// /Upload/EntriesLogImages/202208/1661756358139.jpgs60X60.jpg
        /// </summary>
        public string Base64Picture { get; set; }

        /// <summary>
        /// POST的情況下沒有則設為Empty
        /// </summary>
        public string PictPath { get; set; }
    }

    public class HistAlarmEntriesModel
    {
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
        public double Threshold { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 以Base64格式保存原圖
        /// /Upload/EntriesLogImages/202208/1661756358139.jpgs60X60.jpg
        /// </summary>
        public string Base64Picture { get; set; }
    }
}
