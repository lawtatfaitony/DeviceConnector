using LanguageResource;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace VideoGuard.ApiModels.CameraMpeg
{
    #region CameraMpegApiModel 
    /// <summary>
    /// 從DVR傳入錄像文件的屬性信息 包括文件名,Size,時長等等 
    /// </summary>
    public class CamMpegInfoInput 
    {
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        [JsonProperty("cameraId")]
        public int CameraId { get; set; } 

        [JsonProperty("deviceSerialNo")]
        public string DeviceSerialNo { get; set; }

        [JsonProperty("isGroup")]
        [DefaultValue(false)]
        public bool IsGroup { get; set; }

        [JsonProperty("mpegFileName")]
        public string MpegFileName { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }
        [JsonProperty("fileFormat")]
        public string FileFormat { get; set; }

        [JsonProperty("startTimestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("endTimestamp")]
        public long EndTimestamp { get; set; }

    }

    /// <summary>
    /// 刪除DVR傳入的錄像文件屬性信息 
    /// </summary>
    public class DelMpegInfoInput
    {
        [JsonProperty("recordId")]
        public long RecordId { get; set; }
        [JsonProperty("deviceSerialNo")]
        public string DeviceSerialNo { get; set; }
    }

    #endregion CameraMpegApiModel


    #region StreamInfo 

    public class CameraStreamInfo
    {
        private  int _cameraId ;
        [JsonProperty("cameraId")]
        public int CameraId {
            get {
                return _cameraId;
            }
            set {
                _cameraId = value;
            }
        }

        [JsonProperty("refCount")]
        public int RefCount { get; set; } = 0;
        
        [JsonProperty("input")]
        public string Input { get; set; }

        private string _Output;
        [JsonProperty("output")]
        public string Output {
            get
            {
                 _Output = string.Format("rtmp://127.0.0.1:1935/live/{0}", _cameraId);
                return _Output;
            }
            set
            {
                _Output = value;
            } 
        }

        [JsonProperty("savePic")]
        public bool SavePic { get; set; }

        [JsonProperty("saveVideo")]
        public bool SaveVideo { get; set; }

        [JsonProperty("rtmp")]
        public bool Rtmp { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("PixFmt")]
        public int PixFmt { get; set; } = -1;

        [JsonProperty("HDType")]
        public int HDType { get; set; } = 25;

        [JsonProperty("frameRate")]
        public int FrameRate { get; set; } = 25;

        [JsonProperty("videoIndex")]
        public int VideoIndex { get; set; } = -1;

        [JsonProperty("audioIndex")]
        public int AudioIndex { get; set; } = -1;

        [JsonProperty("videoTime")]
        public int VideoTime { get; set; } = 900;  // 15 minutes
    }
    #endregion StreamInfo

    //device_serial_no and deviceToken Input
    #region DeviceSerialNoInput 

    public class DeviceSerialNoInput : GlobalDeviceSession
    {
        [JsonProperty("deviceSerialNo")]
        public string DeviceSerialNo { get; set; }
    }
    #endregion CameraMpegApiModel
}