using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VideoGuard.Business;
using static EnumCode.EnumBusiness;

namespace VideoGuard.ApiModels
{
    #region DeviceModel
    public class DeviceModel
    {
        public DeviceModel()
        {
            this.SysModuleId = string.Empty;
            this.UpdateDateTime = DateTime.Now;
            this.Config = string.Empty;
            this.DeviceEntryMode = EnumBusiness.DeviceEntryMode.UNDEFINED;
        }

        public string DeviceId { get; set; }
        [Required]
        public string SysModuleId { get; set; }
        [Required]
        public string DeviceName { get; set; }
        [Required]
        public EnumBusiness.DeviceEntryMode DeviceEntryMode { get; set; }
        public string DeviceSerialNo { get; set; }
        public string Config { get; set; }
        public string MainComId { get; set; }
        public string OperatedUser { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public int Status { get; set; }
        public string SiteId { get; set; }
        public bool IsReverseHex { get; set; }
    }

    //add a new device

    public class DeviceModelInput
    {
        public int DeviceId { get; set; }
        [Required]
        public string DeviceName { get; set; }
        [Required]
        public DeviceType DeviceType { get; set; }

        [Required]
        public EnumBusiness.DeviceEntryMode DeviceEntryMode { get; set; }

        [Required]
        public string DeviceSerialNo { get; set; }
        [Required]
        public string MainComId { get; set; }
        public string OperatedUser { get; set; }

        public int Status { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public bool IsReverseHex { get; set; }
    }

    public class DeviceSearchInput : GlobalFieldSession
    {
        public DeviceSearchInput() : base()
        {
            Page = 1;
            PageSize = 50;
            TotalCount = 0;
            TotalPage = 0;
        }

        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public int TotalPage { get; set; }

        [Required]
        public string MainComId { get; set; }

        public string DeviceType { get; set; }
        public string Search { get; set; }

    }
    /// <summary>
    /// 列表单元
    /// </summary>
    public partial class Device
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType { get; set; }
        public string DeviceTypeDesc { get; set; }
        public string MaincomId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? LibId { get; set; }
        public string LibraryName { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceConfig { get; set; }
        public bool IsReverseHex { get; set; }
        public DeviceEntryMode DeviceEntryMode { get; set; }
        public string DeviceEntryModeDesc { get; set; }
        public GeneralStatus Status { get; set; }
        public string StatusDesc { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 設備狀態
    /// </summary>
    public partial class DeviceStatus
    {
        [Required]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        [Required]
        public string MaincomId { get; set; }
        [Required]
        public GeneralStatus Status { get; set; }
    }
    /// <summary>
    /// 設備序列號
    /// </summary>
    public partial class DeviceSerialNoUpd
    {
        [Required]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        [Required]
        public string MaincomId { get; set; }
        [Required]
        public string DeviceSerialNo { get; set; }
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    public partial class DeviceDelModel
    {
        [Required]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        [Required]
        public string MaincomId { get; set; }
    }

    /// <summary>
    /// 海康 拍卡/指紋/人臉機 配置 2022-6-28之前出的比較新款的設置
    /// HIK_DS_KIT341BMW
    /// HIK_DS_KIT804MF
    /// 以及其他款的海康機
    /// </summary>
    public class DeviceHikConfA : DeviceHikConfBase
    {
        public DeviceHikConfA(int deviceId = 0)
        {
            DeviceId = 0;
            DevIp = "127.0.0.1";
            DevIpPort = "8000";
            LoginId = "admin";
            LoginPassword = "hik2345";
            TypeName = Lang.Device_TypeNameSample; // "DS-K1T804BMF FINGERP&CARD";
            TypeNo = "DS-K1T804BMF";
            EmployeeNoPrefix = "E"; //工號的前綴
            DeviceSerialNo = "123456";
            MaxFace = 1000;
            MaxFingerPrint = 1000;
            MaxAccessCard = 1000;
            MaxPassKey = 1000;
            MainComId = "-";
            if (deviceId != 0) //傳入設備ID的情況,則想FtConfig獲取配置
            {
                using BusinessContext businessContext = new BusinessContext();
                var device = businessContext.FtDevice.Find(deviceId);
                if (device != null)
                {
                    var config = DeviceBusiness.ReturnConfigName(ConfigType.DEVICE, (DeviceType)device.DeviceType, deviceId);
                    if (config != null)
                    {
                        string configJsonOfDeviceHikConfA = config.Config;
                        if (!string.IsNullOrEmpty(configJsonOfDeviceHikConfA))
                        {
                            try
                            {
                                DeviceHikConfA hikConf = JsonConvert.DeserializeObject<DeviceHikConfA>(configJsonOfDeviceHikConfA);

                                DevIp = hikConf.DevIp;
                                DevIpPort = hikConf.DevIpPort;
                                LoginId = hikConf.LoginId;
                                LoginPassword = hikConf.LoginPassword;
                                TypeName = hikConf.TypeName;
                                TypeNo = hikConf.TypeNo;
                                EmployeeNoPrefix = hikConf.EmployeeNoPrefix;
                                DeviceSerialNo = hikConf.DeviceSerialNo;
                                MaxFace = hikConf.MaxFace;
                                MaxFingerPrint = hikConf.MaxFingerPrint;
                                MaxAccessCard = hikConf.MaxAccessCard;
                                MaxPassKey = hikConf.MaxAccessCard; //访问咔 数量等于 密码存储数量
                                MainComId = hikConf.MainComId;

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[FUNC::viceHikConfA(int deviceId=0)][EXCEPTION][{ex.Message}]");
                            }
                        }
                    }
                }
            }
        }

    }

    public class DeviceHikConfBase
    {
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string DevIp { get; set; }
        [Required]
        public string DevIpPort { get; set; }
        [Required]
        public string LoginId { get; set; }
        [Required]
        public string LoginPassword { get; set; }
        [Required]
        public string TypeName { get; set; }
        [Required]
        public string TypeNo { get; set; }

        public string EmployeeNoPrefix { get; set; }

        [Required]
        public string DeviceSerialNo { get; set; }

        [Required]
        public int MaxFace { get; set; }
        [Required]
        public int MaxFingerPrint { get; set; }
        [Required]
        public int MaxAccessCard { get; set; }
        [Required]
        public int MaxPassKey { get; set; }
        [Required]
        public string MainComId { get; set; }
    }


    /// <summary>
    /// CIC 的配置 一般就是CIC平台的賬號密碼 
    /// 如果CIC的Apps拍卡,需要16進制反向交叉解析為10進制還是以原來順序解析為10進制
    /// 是否需要補0則不在此考慮的問題
    /// </summary>
    public class DeviceCicModel
    {
        public DeviceCicModel()
        {
            DeviceId = 0;
            DeviceName = Lang.Device_TypeNameSample;
            CicAccount = "";
            CicPassword = "123456";
            DeviceEntryMode = DeviceEntryMode.UNDEFINED;
            MainComId = "";
            SiteId = "-";
            IsReverseHex = true;
        }
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string DeviceName { get; set; }
        [Required]
        public string CicAccount { get; set; }
        [Required]
        public string CicPassword { get; set; }
        public EnumBusiness.DeviceEntryMode DeviceEntryMode { get; set; }
        public string DeviceSerialNo { get; set; }

        public string MainComId { get; set; }

        public string SiteId { get; set; }
        public bool IsReverseHex { get; set; }

        public DeviceCicModel ToInstant(int deviceId)
        {
            DeviceCicModel deviceCicModelx = new DeviceCicModel
            {
                DeviceId = 0,
                DeviceName = "DVR SERVER 1",
                CicAccount = "",
                CicPassword = "123456",
                DeviceEntryMode = EnumBusiness.DeviceEntryMode.UNDEFINED,
                DeviceSerialNo = "8080",
                SiteId = "",
                MainComId = "",
                IsReverseHex = true
            };
            if (deviceId != 0) //傳入設備ID的情況,則想FtConfig獲取配置
            {
                using BusinessContext businessContext = new BusinessContext();
                var device = businessContext.FtDevice.Find(deviceId);
                if (device != null)
                {
                    var config = DeviceBusiness.ReturnConfigName(ConfigType.DEVICE, (DeviceType)device.DeviceType, deviceId);
                    if (config != null)
                    {
                        string configJsonOfDeviceCicModel = config.Config;
                        if (!string.IsNullOrEmpty(configJsonOfDeviceCicModel))
                        {
                            try
                            {
                                DeviceCicModel deviceCicModel = JsonConvert.DeserializeObject<DeviceCicModel>(configJsonOfDeviceCicModel);
                                return deviceCicModel;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[FUNC::DeviceCicModel(int deviceId=0)][EXCEPTION][{ex.Message}]");
                                return deviceCicModelx;
                            }
                        }
                    }
                }
            }

            return deviceCicModelx;
        }
    }

    /// <summary>
    /// 錄像服務器的配置
    /// </summary>
    public class DeviceDVRModel
    {
        public DeviceDVRModel()
        {
            DeviceId = 0;
            DeviceName = "DVR SERVER 1";
            NvrType = "MEDIA";  //默認值是Media.exe錄像系統
            Account = "-";
            Password = "123456";
            DvrIp = "127.0.0.1";
            DvrPort = "8090";
            SiteId = "0";
        }
        [Required]
        public int DeviceId { get; set; }
        [Required]
        public string DeviceName { get; set; }
        /// <summary>
        ///  錄像系統類型（目前有兩款錄像軟件類型：Media.exe 和 MediaGuard.exe）
        /// </summary>
        [Required]
        public string NvrType { get; set; }

        [Required]
        public string DvrIp { get; set; }
        [Required]
        public string DvrPort { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string SiteId { get; set; }

        public string MaincomId { get; set; }

        public DeviceDVRModel ToInstant(int deviceId)
        {
            DeviceDVRModel deviceDVRModelx = new DeviceDVRModel
            {
                MaincomId = "",
                DeviceId = 0,
                DeviceName = "DVR SERVER 1",
                NvrType = NVR_TYPE.MEDIA.ToString(),
                Account = "-",
                Password = "123456",
                DvrIp = "127.0.0.1",
                DvrPort = "8090",
                SiteId = "0"

            };
            if (deviceId != 0) //傳入設備ID的情況,則想FtConfig獲取配置
            {
                using BusinessContext businessContext = new BusinessContext();
                var device = businessContext.FtDevice.Find(deviceId);
                if (device != null)
                {
                    var config = DeviceBusiness.ReturnConfigName(ConfigType.DEVICE, (DeviceType)device.DeviceType, deviceId);
                    if (config != null)
                    {
                        string configJsonOfDeviceDVRModel = config.Config;
                        if (!string.IsNullOrEmpty(configJsonOfDeviceDVRModel))
                        {
                            try
                            {
                                DeviceDVRModel deviceDVRModel = JsonConvert.DeserializeObject<DeviceDVRModel>(configJsonOfDeviceDVRModel);
                                MaincomId = device.MaincomId;
                                DeviceId = deviceDVRModel.DeviceId;
                                DeviceName = deviceDVRModel.DeviceName ?? string.Empty;
                                NvrType = deviceDVRModel.NvrType ?? NVR_TYPE.MEDIA.ToString();
                                Account = deviceDVRModel.Account ?? string.Empty;
                                Password = deviceDVRModel.Password ?? string.Empty;
                                DvrIp = deviceDVRModel.DvrIp ?? string.Empty;
                                DvrPort = deviceDVRModel.DvrPort ?? string.Empty;
                                SiteId = deviceDVRModel.SiteId == "0" ? string.Empty : deviceDVRModel.SiteId;

                                return deviceDVRModel;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[FUNC::DeviceCicModel(int deviceId=0)][EXCEPTION][{ex.Message}]");
                                return deviceDVRModelx;
                            }
                        }
                    }
                }
            }

            return deviceDVRModelx;
        }

    }

    /// <summary>
    /// QQ 地圖配置定位 主要是Key(來自qq map平台生成的授權key)
    /// </summary>
    public partial class QQmapApiModel
    {
        [Required]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        [Required]
        public string MaincomId { get; set; }

        /// <summary>
        /// 選填 ip
        /// </summary>
        public string MapApiIp { get; set; } = "127.0.0.1";

        /// <summary>
        /// qq map key 關鍵 的 訪問的 key 
        /// </summary>
        [Required]
        public string MapApiKey { get; set; }
        /// <summary>
        /// qq map api output format : json/jsonp 輸出結果格式
        /// </summary>
        public string ApiOutput { get; set; } = "json";
        /// <summary>
        /// qq map api callback function api回調函數
        /// </summary>
        public string ApiCallback { get; set; } = "function1";
    }

    /// <summary>
    /// 設備出入口模式(如有)
    /// </summary>
    public class DeviceEntryModeInput
    {
        public int DeviceId { get; set; }
        public EnumBusiness.DeviceEntryMode DeviceEntryMode { get; set; } = EnumBusiness.DeviceEntryMode.UNDEFINED;
        public string MainComId { get; set; }
    }
    /// <summary>
    /// 根據 hist_recognize_record.mode 返回對應的 和 DGX的AttendanceMode (移植過來的)
    /// </summary>
    public class DeviceMediaType
    {
        public AttendanceMode AttendanceMode { get; set; }
        public string CatchPictureFileNameIfHave { get; set; }
    }

    public class CameraOnLiveModel
    {
        public int DeviceId { get; set; }
        public int CameraId { get; set; }
        public string OonliveUrl { get; set; }
        public string PlayUrl { get; set; }
        public string DeviceToken { get; set; }
    }

    public class DeviceSiteTreeModel
    {
        public int NodeId { get; set; }
        public string Text { get; set; }
        public string MainComId { get; set; }
        public int CameraId { get; set; }
        public string CameraName { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int ParentsId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

    }

    /// <summary>
    /// 指定設備人員群組庫
    /// </summary>
    public class DeviceLibInput
    {
        public int DeviceId { get; set; }
        public int LibId { get; set; }
        public string MainComId { get; set; }
    }
    #endregion DeviceModel

}

