using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static EnumCode.EnumBusiness;

namespace VideoGuard.ApiModels.CamApiModel
{
    #region CamApiModel 
    public class CamApiModel : GlobalFieldSession
    {
        public CamApiModel() : base()
        {
        }
        [DefaultValue("1")]
        public string CameraId { get; set; }
        [Required(ErrorMessageResourceName = "Cam_Name_required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "Cam_Rtsp_required", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [RegularExpression(@"^rt(?:s|m)p:\/\/\w{1,20}:\w{1,20}@\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}?([\\.\\w]+)([:\\.\\w]+)?(.*)$", ErrorMessageResourceName = "Cam_Rtsp_Format", ErrorMessageResourceType = typeof(ResourceLocalize))] //for example: rtsp://admin:passward@192.168.1.3 
        public string Rtsp { get; set; }
        [DefaultValue(0)]
        public int Type { get; set; }
        public string Remark { get; set; }
    }

    public class CamApiModelInput : GlobalFieldSession
    {
        public CamApiModelInput() : base()
        {
        }
        public string MaincomId { get; set; }
        public string Name { get; set; }
        public string Rtsp { get; set; }
        [DefaultValue(0)]
        public int Type { get; set; }
        [Required]
        public int SiteId { get; set; }
        [DefaultValue(0)]
        public int DeviceId { get; set; }

        public string Remark { get; set; }
    }
    #endregion CamApiModel

    #region QueryCameraList
    public class QueryCameraListInput : GlobalFieldSession
    {
        public QueryCameraListInput() : base()
        {
            _PageNo = 1;
            _PageSize = 24;
            _Type = 0;
        }
        private int _PageNo;
        [JsonProperty("pageNo")]
        public int PageNo
        {
            get
            {
                return _PageNo;
            }

            set
            {
                if (value == 0)
                {
                    _PageNo = 1;
                }
                else
                {
                    _PageNo = value;
                }
            }
        }

        private int _PageSize;
        [DefaultValue(24)]
        [JsonProperty("pageSize")]
        public int PageSize
        {
            get
            {
                return _PageSize;
            }

            set
            {
                if (value == 0)
                {
                    _PageSize = 24;
                }
                else
                {
                    _PageSize = value;
                }
            }
        }
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }

        [JsonProperty("name")]
        [DefaultValue("")]
        public string Name { get; set; }

        private int _Type;
        [DefaultValue(0)]
        [JsonProperty("type")]
        public int Type
        {
            get
            {
                return _Type;
            }

            set
            {
                if (value == 0)
                {
                    _Type = 24;
                }
                else
                {
                    _Type = value;
                }
            }
        }

        [DefaultValue(0)]
        public int DeviceId { get; set; }
    }

    /// <summary>
    /// 查詢設備鏡頭列表接口
    /// </summary>
    public class CameraListOfDeviceInput
    {
        [JsonProperty("maincomId")]
        public string MaincomId { get; set; }
        [JsonProperty("deviceId")]
        [DefaultValue(0)]
        public int DeviceId { get; set; }
    }
    //Camera For Rtsp
    public class CameraRtspDetail : Camera
    {
        public string RtspIp { get; set; }


        public string RtspUsername { get; set; }


        public string RtspPassword { get; set; }

        public List<ModelSetting> ModelListSetting { get; set; } = new List<ModelSetting>();
    }
    public class ModelSetting
    {
        public ModelSetting()
        {
            ModelId = "1M0001";
            ModelName = "yolovx2.pt";
            AlertTriggerConditions = true;
            PrecisionRate = 90.0M;
            AlertVoice = "Alert Voice";
            AlertVoiceFile = "";
        } 
        public string ModelId { get; set; } //1M00122",
        public string ModelName { get; set; } //  "yolovx2.pt",
        public bool AlertTriggerConditions { get; set; }  // true | false 當model為true|false 觸發
        public decimal PrecisionRate { get; set; } 
        public string AlertVoice { get; set; } //"請使用指定袋",
        public string AlertVoiceFile { get; set; } // aaa.mp3"
    }
    public class Camera
    {
        public string MaincomId { get; set; }
        public int CameraId { get; set; }
        public string Name { get; set; }
        public string CameraIp { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string Rtsp { get; set; }
        public int Type { get; set; }
        /// <summary>
        /// 是VISIBLE转义过来的，表示UI页面层是否显示或者不显示，或者用于删除状态
        /// </summary>
        public string Online { get; set; }
        public string Remark { get; set; }
        public string CreateTime { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public CameraRecordStatus RecordStatus { get; set; }

        public DeviceType DeviceType { get; set; } = EnumBusiness.DeviceType.UNDEFINED_DEVICE;
        /// <summary>
        /// DVR录像系统使用的功能。主要显示是否直播中的状态
        /// </summary>
        public bool Onlive { get; set; } = false;
    }

    public class QueryCameraListInfoReturn
    {
        private int _PageCount;
        public int PageCount
        {
            get
            {
                int _PageCount = (TotalCount + PageSize - 1) / PageSize;
                return _PageCount;
            }
            set
            {
                _PageCount = value;
            }
        }

        private int _PageNo;
        [JsonProperty("pageNo")]
        public int PageNo
        {
            get
            {
                return _PageNo;
            }
            set
            {
                _PageNo = value;
            }
        }

        private int _PageSize;
        [DefaultValue(4)]
        [JsonProperty("pageSize")]
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }
        private int _TotalCount;
        [JsonProperty("totalCount")]
        public int TotalCount
        {
            get
            {
                return _TotalCount;
            }
            set
            {
                _TotalCount = value;
            }
        }
        private List<Camera> _Items;
        [JsonProperty("items")]
        public List<Camera> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                _Items = value;
            }
        }
    }

    public class QueryCameraListSelect
    {

        [JsonProperty("label")]
        public string label { get; set; }


        [JsonProperty("value")]
        public string value { get; set; }

    }

    #endregion QueryCameraList

    #region CameraRtsp
    public class CameraRtsp : GlobalFieldSession
    {
        public CameraRtsp() : base()
        {
        }
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("user")]
        [DefaultValue("")]
        public string User { get; set; }
        [JsonProperty("Password")]
        [DefaultValue("")]
        public string Password { get; set; }
    }
    public class CameraRtspSuccResult : GlobalReturnResult
    {
        [JsonProperty("info")]
        public CameraRtspInfoResult Info { get; set; }
    }
    public class CameraRtspInfoResult
    {
        [JsonProperty("rtsp")]
        public string Rtsp { get; set; }
    }
    #endregion

    #region DELETE CAMERA
    public class CameraDelInput : GlobalFieldSession
    {
        public CameraDelInput() : base()
        {
        }
        [JsonProperty("cameraId")]
        public int CameraId { get; set; }
    }
    #endregion

    #region DETAILS
    public class CameraIdInput : GlobalFieldSession
    {
        public CameraIdInput() : base()
        {
        }
        [JsonProperty("cameraId")]
        public int CameraId { get; set; }
    }
    #endregion

    #region CamUpdate

    public class CamUpdate : GlobalFieldSession
    {
        public CamUpdate() : base()
        {
        }
        public string MaincomId { get; set; }

        [JsonProperty("cameraId")]
        public int CameraId { get; set; }

        [JsonProperty("siteId")]
        [Required]
        public int SiteId { get; set; }

        [JsonProperty("siteName")]
        public string SiteName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rtsp")]
        public string Rtsp { get; set; }
        [DefaultValue(0)]
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("deviceId")]
        public int DeviceId { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }
    #endregion

    #region CamSearch

    public class CamSearch : GlobalFieldSession
    {
        public CamSearch() : base()
        {
        }
    }

    public class CamSearchInfo
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
        [JsonProperty("items")]
        public List<CamSearchInfoItem> Items { get; set; }

    }

    public class CamSearchInfoItem
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }

    }
    public class CameraRtspInfo
    {
        public CameraRtspInfo(string rtsp)
        {
            Uri uri = new Uri(rtsp);
            string host = uri.Host;
            _Ip = uri.Host;
            string scheme = uri.Scheme;
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                string[] userInfo = uri.UserInfo.Split(':');
                _User = userInfo[0];
                _Password = userInfo[1];
            }
        }
        private string _Ip;
        [JsonProperty("ip")]
        public string Ip
        {
            get
            {
                return _Ip;
            }
            set
            {
                _Ip = value;
            }
        }

        private string _User;
        [JsonProperty("user")]
        public string User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }
        private string _Password;
        [JsonProperty("password")]
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

    }


    /// <summary>
    /// 镜头录像 狀態 Camera record status
    /// </summary>
    public partial class CameraStatus
    {
        [Required]
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        [Required]
        public string MaincomId { get; set; }
        [Required]
        public CameraRecordStatus RecordStatus { get; set; }
    }

    /// <summary>
    /// 镜头 是否停用
    /// </summary>
    public partial class CameraVisibleInput
    {
        [Required]
        public int CameraId { get; set; }

        [Required]
        public string MaincomId { get; set; }

    }

    /// <summary>
    /// 镜头是否停用状态返回
    /// </summary>
    public partial class CameraVisibleResponse
    {
        [Required]
        public int CameraId { get; set; }

        [Required]
        public string MaincomId { get; set; }

        public EnumCode.CameraVisible CameraVisible { get; set; }

        public string CameraVisibleDesc { get; set; }

    }

    /// <summary>
    /// 镜头录像设置模式 
    /// </summary>
    public class CameraDVRSettingModel
    {
        [Required]
        public int CameraId { get; set; }

        /// <summary>
        /// 从设备配置中获取 没有则提示先定义设备
        /// </summary>
        [Required]
        public int DeviceId { get; set; }
        /// <summary>
        /// 从设备配置中获取
        /// </summary>
        [Required]
        public string DeviceName { get; set; }
        /// <summary>
        /// 从设备配置中获取
        /// 没有则 等于 string.empty
        /// </summary>
        public string SiteId { get; set; }
        /// <summary>
        /// 从设备配置中获取.必须的。
        /// </summary>
        public string MaincomId { get; set; }

        //------------------------------------------------------------------
        /// <summary>
        /// 保存MPEG 4 格式文件
        /// </summary>
        public bool SaveMpeg4 { get; set; } = true;
        /// <summary>
        /// 是否输出HLS
        /// </summary>
        public bool HlsOutput { get; set; } = false;
        /// <summary>
        /// 是否输出RTMP
        /// </summary>
        public bool RtmpOutput { get; set; } = true;
        /// <summary>
        /// 是否人脸识别
        /// </summary>
        public bool Recognize { get; set; } = false;
        /// <summary>
        /// Frame保存JPG图片
        /// </summary>
        public bool SavePic { get; set; } = false;
        /// <summary>
        /// 定义输出的大小 1920px > 768px 单位 px
        /// </summary>
        public int FrameWidth { get; set; } = 1920;
        /// <summary>
        /// 定义输出的大小 1920px > 768px  单位 px
        /// </summary>
        public int FrameHeight { get; set; } = 768;
        /// <summary>
        /// 定义输出的大小 1920px > 768px  单位 侦
        /// </summary>
        public int FrameRate { get; set; } = 25;
        /// <summary>
        /// 保存MP4文件时长 单位 秒
        /// </summary>

        public int FileOutputTimeSpan { get; set; } = 3600;
        /// <summary>
        /// 保存MP4文件的大小 （字节） 默认1GB   说明：1 千兆字节(GB)=1073741824 字节
        /// </summary>
        public long FileOutputFileSize { get; set; } = 1073741824;
    }

    /// <summary>
    /// 镜头录像设置与录像策略配置
    /// </summary>
    public class CameraDVRSettingNSchedule
    {
        [Required]
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        public string CameraIp { get; set; }
        /// <summary>
        /// 从设备配置中获取 没有则提示先定义设备
        /// </summary>
        [Required]
        public int DeviceId { get; set; }
        /// <summary>
        /// 从设备配置中获取
        /// </summary>
        [Required]
        public string DeviceName { get; set; }
        /// <summary>
        /// 从设备配置中获取
        /// 没有则 等于 string.empty
        /// </summary>
        public string SiteId { get; set; }
        /// <summary>
        /// 从设备配置中获取.必须的。
        /// </summary>
        public string MaincomId { get; set; }

        /// <summary>
        /// 是否啟動策略
        /// </summary>
        public bool PolicyIsStart { get; set; }
        //------------------------------------------------------------------
        /// <summary>
        /// 保存錄像 SaveVideo
        /// </summary>
        public bool SaveVideo { get; set; }
        /// <summary>
        /// 保存MPEG 4 格式文件 MPEG/FLV
        /// </summary>
        public bool SaveMpeg4 { get; set; }

        /// <summary>
        /// 是否输出HLS
        /// </summary>
        public bool HlsOutput { get; set; }
        /// <summary>
        /// 是否输出RTMP
        /// </summary>
        public bool RtmpOutput { get; set; }

        /// <summary>
        /// 是否保存图片流 保存照片流才可以启动个种识别任务
        /// </summary>
        public bool SavePic { get; set; }

        /// <summary>
        /// 一般幀率是25幀1秒，如果要設置2秒獲取一張圖片則是 SavePictRate=50，
        /// 設置僅僅是大概數值，視乎計算機效率性能。
        /// 反正依據這個規則來設置獲取圖片的間隔時長
        /// </summary> 
        public int SavePictRate { get; set; }
        //------------------------------------------------------------------
        /// <summary>
        /// 星期日 开始时间的录像调度安排
        /// </summary> 
        public TimeSpan SundayStart { get; set; }
        /// <summary>
        /// 星期日 结束时间录像调度安排
        /// </summary>
        public TimeSpan SundayEnd { get; set; }

        /// <summary>
        /// 星期一 开始时间的录像调度安排
        /// </summary>
        public TimeSpan MondayStart { get; set; }
        /// <summary>
        /// 星期一结束时间录像调度安排
        /// </summary>
        public TimeSpan MondayEnd { get; set; }
        /// <summary>
        /// 星期二 开始时间的录像调度安排
        /// </summary>
        public TimeSpan TuesdayStart { get; set; }
        /// <summary>
        /// 星期二结束时间录像调度安排
        /// </summary>
        public TimeSpan TuesdayEnd { get; set; }

        /// <summary>
        /// 星期三 开始时间的录像调度安排
        /// </summary>
        public TimeSpan WednesdayStart { get; set; }
        /// <summary>
        /// 星期三结束时间录像调度安排
        /// </summary> 
        public TimeSpan WednesdayEnd { get; set; }

        /// <summary>
        /// 星期四 开始时间的录像调度安排
        /// </summary>
        public TimeSpan ThursdayStart { get; set; }
        /// <summary>
        /// 星期四结束时间录像调度安排
        /// </summary>
        public TimeSpan ThursdayEnd { get; set; }

        /// <summary>
        /// 星期五 开始时间的录像调度安排
        /// </summary>
        public TimeSpan FridayStart { get; set; }
        /// <summary>
        /// 星期五 结束时间录像调度安排
        /// </summary>
        public TimeSpan FridayEnd { get; set; }

        /// <summary>
        /// 星期六 开始时间的录像调度安排
        /// </summary>
        public TimeSpan SaturdayStart { get; set; }
        /// <summary>
        /// 星期六 结束时间录像调度安排
        /// </summary>
        public TimeSpan SaturdayEnd { get; set; }

        /// <summary>
        /// 初始化一个默认值实例
        /// </summary>
        /// <returns></returns>
        public static CameraDVRSettingNSchedule ToInstance()
        {
            TimeSpan span2359 = new TimeSpan(23, 59, 59);
            //default value-------------------------------------------
            CameraDVRSettingNSchedule cameraDVRSettingNSchedule = new CameraDVRSettingNSchedule
            {
                SundayStart = TimeSpan.Zero,
                SundayEnd = span2359,
                MondayStart = TimeSpan.Zero,
                MondayEnd = span2359,
                TuesdayStart = TimeSpan.Zero,
                TuesdayEnd = span2359,
                WednesdayStart = TimeSpan.Zero,
                WednesdayEnd = span2359,
                ThursdayStart = TimeSpan.Zero,
                ThursdayEnd = span2359,
                FridayStart = TimeSpan.Zero,
                FridayEnd = span2359,
                SaturdayStart = TimeSpan.Zero,
                SaturdayEnd = span2359,

                //--------------------------------------------------------
                PolicyIsStart = true,
                SaveVideo = true,
                SaveMpeg4 = false, //默認配置是FLV
                HlsOutput = true,
                RtmpOutput = false
            };

            return cameraDVRSettingNSchedule;
        }
    }

    /// <summary>
    /// 改变 镜头是否直播
    /// </summary>
    public partial class CameraOnliveInput
    {
        [Required]
        public string MaincomId { get; set; }
        [Required]
        public int CameraId { get; set; }
        public string CameraName { get; set; }

        [Required]
        public bool IsOnlive { get; set; }
    }
    #endregion

    #region CameraHistory

    /// <summary>
    /// 录像历史记录数据列表单元
    /// </summary>
    public partial class CamHist
    {
        [Required]
        public string MaincomId { get; set; }
        [Required]
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        public string FileFomat { get; set; }

        [Required]
        public long Timepoint { get; set; }

        [Required]
        public long BeginTime { get; set; }
        [Required]
        public long EndTime { get; set; }
        public CamHistStyle Style { get; set; }
    }

    public partial class CamHistX
    {
        [Required]
        public long BeginTime { get; set; }
        [Required]
        public long EndTime { get; set; }
        public CamHistStyle Style { get; set; }
    }
    public partial class CamHistWithUrl
    {
        public string PlayUrl { get; set; }
    }
    public partial class TimePointData
    {
        [Required]
        public CamHistX CamHistX { get; set; }

        /// <summary>
        /// 如果存在影片則返回當前點擊的時間點,否則返回 BeginTime
        /// </summary>
        [Required]
        public long Timepoint { get; set; }

        /// <summary>
        /// 快進 默認為0
        /// </summary>
        [Required]
        public long Seek { get; set; }

        /// <summary>
        /// 媒体格式 mpeg/flv
        /// </summary>
        [Required]
        public string FileFomat { get; set; }
        [Required]
        public CamHistWithUrl CamHistWithUrl { get; set; }

        [Required]
        public Camera Camera { get; set; }
    }

    public partial class CamHistStyle
    {
        public string Background { get; set; }
    }
    #endregion


}

