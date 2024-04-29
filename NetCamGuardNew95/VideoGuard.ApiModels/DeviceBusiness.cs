using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels;
using static EnumCode.EnumBusiness;

namespace VideoGuard.Business
{
    public partial class DeviceBusiness
    {
        public static string CameraStreamInfo_Prefix { get; set; } = "CameraStreamInfo_";

        /// <summary>
        /// 生成 配置表 FtConfig的主键ID,格式如: DEVICE_HIK_DS_KIT341BMW_3001
        /// 優先獲取id降序排列的第一個
        /// </summary>
        /// <param name="configType">DEVICE</param>
        /// <param name="deviceType">HIK_DS_KIT341BMW</param>
        /// <param name="deviceId"></param>
        /// <returns>DEVICE_HIK_DS_KIT341BMW_3001</returns>
        public static string GenerateConfigName(EnumBusiness.ConfigType configType, DeviceType deviceType, int deviceId)
        {
            string configName = $"{configType}_{deviceType}_{deviceId}";
            return configName;
        }
        /// <summary>
        /// 通過按規則生成的ConfigName獲取配置,如果沒有返回null
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static FtConfig ReturnConfigName(EnumBusiness.ConfigType configType, DeviceType deviceType, int deviceId)
        {
            string configName = GenerateConfigName(configType, deviceType, deviceId);
            using BusinessContext businessContext = new BusinessContext();
            var config = businessContext.FtConfig.OrderByDescending(c => c.Id).Where(c => c.Name.Contains(configName)).FirstOrDefault();
            return config;
        }
        /// <summary>
        /// 返回配置的對象 object 沒有則返回null
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static object ReturnDeviceConfigObject(int deviceId)
        {
            if (deviceId == 0)
                return null;

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
                return null;

            DeviceType deviceType = (DeviceType)device.DeviceType;
            FtConfig ftConfig = ReturnConfigName(ConfigType.DEVICE, deviceType, deviceId);
            if (ftConfig == null)
                return null;

            if (string.IsNullOrEmpty(ftConfig.Config))
                return null;

            try
            {
                object obj = JsonConvert.DeserializeObject<object>(ftConfig.Config);
                return obj;
            }
            catch (Exception ex)
            {
                string logline = $"[FUNC::DeviceBusiness.ReturnDeviceConfigObject()][EXCEPTION][{ex.Message}]";
                CommonBase.ConsoleWriteline(logline);
                return null;
            }
        }

        /// <summary>
        /// 查詢設備 傳入鏡頭id 返回 鏡頭的解碼的 HLS 包含參數token
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="NvrIp"></param>
        /// <param name="NvrPort"></param>
        /// <param name="deviceId"></param>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public static string RequestCameraHlsUrlFormat(DeviceDVRModel deviceDVRModel, int cameraId)
        {
            string scheme = "http";
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][scheme={scheme}][NvrIp={deviceDVRModel.DvrIp}][NvrPort={deviceDVRModel.DvrPort}][deviceId={deviceDVRModel.DeviceId}][cameraId={cameraId}]");
            string plaintText = $"{deviceDVRModel.Account}:{deviceDVRModel.Password}";  //Bear 要求的格式,
            string token = CommonBase.MD5Encrypt(plaintText);
            string hlsUrlFormat;
            if (deviceDVRModel.NvrType == NVR_TYPE.MEDIA_GUARD.ToString())
            {
                //http://localhost:8080/hls/12/index.m3u8?token=7ad166bdb8395514bb54cc0ac21db289
                hlsUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/hls/{cameraId}/index.m3u8?token={token}";
            }
            else
            {
                // http://192.168.0.146:180/hls/11/1/1/index.m3u8?token=2ccd575f54be0d71277a82f8baf2e8ea
                hlsUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/hls/{cameraId}/1/1/index.m3u8?token={token}"; //1 = channel /1 = 碼流 (0=主碼流)
            }
            return hlsUrlFormat;
        }
        //playUrl
        /// <summary>
        /// 查詢設備 傳入鏡頭id 返回 鏡頭的解碼的 HLS 包含參數token
        /// 例如  http://192.168.0.146:180/play?deviceId={cameraId}&channel=1&stream=1"; 
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="NvrIp"></param>
        /// <param name="NvrPort"></param>
        /// <param name="deviceId"></param>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public static string RequestCameraPlayUrlFormat(DeviceDVRModel deviceDVRModel, int cameraId)
        {
            string scheme = "http";
            string playUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/play?deviceId={cameraId}&channel=1&stream=1"; //1 = channel /1 = 碼流 (0=主碼流)
            return playUrlFormat;
        }
        //轉換為不同錄像軟件類型的 url返回
        public static string RequestMediaUrlFormat(DeviceDVRModel deviceDVRModel, VideoGuard.Business.CameraMpeg cameraMpeg)
        {
            string scheme = "http";
            Console.WriteLine($"[FUNC:DeviceBusiness.RequestMediaUrlFormat][{DateTime.Now:yyyy-MM-dd HH:mm:ss}][scheme={scheme}][NvrIp={deviceDVRModel.DvrIp}][NvrPort={deviceDVRModel.DvrPort}][deviceId={deviceDVRModel.DeviceId}]");
            string plaintText = $"{deviceDVRModel.Account}:{deviceDVRModel.Password}";  //Bear 要求的格式,
            string token = CommonBase.MD5Encrypt(plaintText);
            string mediaUrlFormat;

            if (deviceDVRModel.NvrType == NVR_TYPE.MEDIA_GUARD.ToString())
            {
                //http://localhost:8080/video/2023-07-30/1688.mp4?token=7ad166bdb8395514bb54cc0ac21db289
                mediaUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/{cameraMpeg.MpegFilename}?token={token}";
            }
            else
            {
                mediaUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/record/{cameraMpeg.MpegFilename}?token={token}"; //1 = channel /1 = 碼流 (0=主碼流)
            }
            //#if DEBUG
            //            //http://localhost:8080/video/2023-07-30/1688.mp4?token=7ad166bdb8395514bb54cc0ac21db289
            //            mediaUrlFormat = $"{scheme}://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/{cameraMpeg.MpegFilename}?token={token}";
            //#endif

            Console.WriteLine($"[FUNC:DeviceBusiness.RequestMediaUrlFormat][{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{mediaUrlFormat}]");

            return mediaUrlFormat;
        }

        /// <summary>
        /// 用于 MPEG4REC项目的程序的Token验证
        /// </summary>
        /// <param name="deviceSerialNo"></param>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        public static bool VerifiedDeviceToken(string deviceSerialNo, string deviceToken)
        {
            string serverHamc;
            bool verifiedDeviceToken = false;

            //Method I ::
            // Exact match to minute
            //string exactMatchToMinute = string.Format("{0:yyyyMMddHHmm}", DateTime.Now);
            //if (long.TryParse(exactMatchToMinute, out long intYYMMDDhhmmss))
            //{
            //    intYYMMDDhhmmss = intYYMMDDhhmmss - (intYYMMDDhhmmss % 10);
            //}
            //else
            //{
            //    intYYMMDDhhmmss = 0;  // bug 
            //}
            //serverHamc = CommonBase.HMACSHA1Encode(deviceSerialNo, intYYMMDDhhmmss.ToString());

            //Method II ::
            //Exact match to year
            int exactMatchToYear = DateTime.Now.Year;
            //Hamc by exactMatchToYear
            serverHamc = CommonBase.HMACSHA1Encode(deviceSerialNo, exactMatchToYear.ToString());

            //------------------------------------------------------------------------------------------

            if (deviceToken == serverHamc)
            {
                return true;
            }
            //不使用如下办法的hamc进行补错
            //for (int i = 1; i <= 4; i++)
            //{
            //    long newTime;

            //    if (i%2==0)
            //    {
            //        newTime = intYYMMDDhhmmss + i;
            //    }
            //    else
            //    {
            //        newTime = intYYMMDDhhmmss - i;
            //    }

            //    serverHamc = CommonBase.HMACSHA1Encode(deviceSerialNo, newTime.ToString());

            //    if (deviceToken == serverHamc)
            //    {
            //        verifiedDeviceToken = true;
            //        break;
            //    }
            //}
            return verifiedDeviceToken;
        }

        /// <summary>
        /// 获取设备名称和序列号，用于镜头指派到设备
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static CameraDeviceSetting GetCameraDeviceSetting(int deviceId)
        {
            CameraDeviceSetting cameraDeviceSetting = new CameraDeviceSetting { DeviceId = 0, DeviceName = String.Empty, DeviceSerialNo = String.Empty };
            if (deviceId == 0)
            {
                return cameraDeviceSetting;
            }
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                return cameraDeviceSetting;

            }
            else
            {
                cameraDeviceSetting = new CameraDeviceSetting { DeviceId = device.DeviceId, DeviceName = device.DeviceName, DeviceSerialNo = device.DeviceSerialNo };
                return cameraDeviceSetting;
            }
        }
        /// <summary>
        /// 检查设备是否同名
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="DeviceName"></param>
        /// <param name="responseModalX"></param>
        /// <returns>false 表示没有同名</returns>
        public static bool IsTheSameDeviceName(int deviceId, string DeviceName, ref ResponseModalX responseModalX)
        {
            using BusinessContext businessContext = new BusinessContext();

            DeviceName = DeviceName.Trim();

            FtDevice device = new FtDevice();
            if (deviceId == 0)
            {
                device = businessContext.FtDevice.Where(c => c.DeviceName.Contains(DeviceName)).FirstOrDefault();
            }
            else
            {
                device = businessContext.FtDevice.Where(c => c.DeviceName.Contains(DeviceName) && c.DeviceId != deviceId).FirstOrDefault();
            }

            if (device != null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.EXIST_THE_SAME_NAME, Success = false, Message = Lang.DEVICE_EXIST_THE_SAME_NAME },
                    data = null
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX();
                return false;
            }
        }
        /// <summary>
        /// 判断是否有其他设备已经占用序列号
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool IsTheSameDeviceSerialNo(int deviceId, string deviceSerialNo, ref ResponseModalX responseModalX)
        {
            using BusinessContext businessContext = new BusinessContext();

            if (!string.IsNullOrEmpty(deviceSerialNo))
                deviceSerialNo = deviceSerialNo.Trim();

            FtDevice device = new FtDevice();
            if (deviceId == 0)
            {
                device = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(deviceSerialNo)).FirstOrDefault();
            }
            else
            {
                device = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(deviceSerialNo) && c.DeviceId != deviceId).FirstOrDefault();
            }

            if (device != null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.EXIST_THE_SAME_DEVCIE_SERIALNO, Success = false, Message = Lang.Device_ExistDeviceSerialNoTips },
                    data = null
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX();
                return false;
            }
        }

        /// <summary>
        /// 保存 DeviceHikConfA 海康拍卡 识别机 系列的 配置
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool SaveconfigForDevice(int deviceId, object objForJson)
        {
            if (deviceId == 0)
            {
                return false;
            }
            else
            {
                using BusinessContext businessContext = new BusinessContext();

                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                string deviceHikConfAJson = JsonConvert.SerializeObject(objForJson, jsonSettings);

                if (string.IsNullOrEmpty(deviceHikConfAJson))
                    return false;

                FtDevice ftDevice = businessContext.FtDevice.Find(deviceId);

                if (ftDevice == null)
                    return false;


                DeviceType deviceType = (DeviceType)ftDevice.DeviceType;
                string configName = GenerateConfigName(ConfigType.DEVICE, deviceType, deviceId);

                FtConfig ftConfig = businessContext.FtConfig.Where(c => c.Name.Contains(configName)).FirstOrDefault();
                if (ftConfig != null)
                {
                    ftConfig.Visible = (int)GeneralVisible.VISIBLE;
                    ftConfig.Config = deviceHikConfAJson ?? String.Empty;
                    ftConfig.Type = (int)ConfigType.DEVICE;
                    ftConfig.UpdateTime = DateTime.Now;

                    try
                    {
                        businessContext.FtConfig.Update(ftConfig);
                        bool res = businessContext.SaveChanges() > 0;
                        return res;
                    }
                    catch (Exception ex)
                    {
                        string logline = $"[FUNC::DeviceBusiness.SaveconfigForDeviceHikConfA()][UPDATE][EXCEPTION][{ex.Message}]";
                        CommonBase.ConsoleWriteline(logline);
                        return false;
                    }
                }
                else
                {

                    int maxId = 100;  //Ftconfig配置表的ID自增基数
                    if (businessContext.FtConfig.Count() > 0)
                    {
                        maxId = businessContext.FtConfig.Max(c => c.Id) + 1;
                    }
                    FtConfig ftConfigInsert = new FtConfig
                    {
                        Id = maxId,
                        Visible = (int)GeneralVisible.VISIBLE,
                        Name = configName,
                        Config = deviceHikConfAJson ?? String.Empty,
                        Type = (int)ConfigType.DEVICE,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };
                    try
                    {
                        businessContext.FtConfig.Add(ftConfigInsert);
                        bool res = businessContext.SaveChanges() > 0;
                        return res;
                    }
                    catch (Exception ex)
                    {
                        string logline = $"[FUNC::DeviceBusiness.SaveconfigForDeviceHikConfA()][ADD][EXCEPTION][{ex.Message}]";
                        CommonBase.ConsoleWriteline(logline);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 通過設備序列號 獲取設備id
        /// </summary>
        /// <param name="deviceSerialNo"></param>
        /// <param name="deviceId">輸出設備id</param>
        /// <returns></returns>
        public static bool GetDeviceIdByDeviceSerialNo(string deviceSerialNo, out int deviceId)
        {
            using BusinessContext businessContext = new BusinessContext();
            //值查詢 DeviceId和DeviceSerialNo 提升查詢速度  { s.DeviceId,s.DeviceSerialNo })
            var ftDevice = businessContext.FtDevice.Select(s => new { s.DeviceId, s.DeviceSerialNo }).Where(c => c.DeviceSerialNo.Contains(deviceSerialNo)).FirstOrDefault();
            if (ftDevice != null)
            {
                deviceId = ftDevice.DeviceId;
                return true;
            }
            else
            {
                deviceId = 0;
                return false;
            }
        }

        /// <summary>
        /// 通過卡號取得人員資料,如果沒有返回null
        /// 系統規則: card no 在人員錄入和更新資料 保證card no 是唯一的.
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="personCardInfo"></param>
        /// <returns></returns>
        public static bool IsCardNumberRelatePerson(string maincomId, string cardNo, ref PersonCardInfo personCardInfo)
        {
            personCardInfo = null;
            if (string.IsNullOrEmpty(maincomId) || string.IsNullOrEmpty(cardNo))
            {
                return false;
            }
            using BusinessContext businessContext = new BusinessContext();

            FtPerson ftPerson = businessContext.FtPerson.Where(c => c.CardNo.Contains(cardNo) && c.MaincomId.Contains(maincomId)).FirstOrDefault();

            if (ftPerson != null)
            {
                personCardInfo = new PersonCardInfo
                {
                    MaincomId = ftPerson.MaincomId,
                    OuterId = ftPerson.OuterId,
                    Category = (sbyte)ftPerson.Category,   //ftPerson.Category==0 ? (sbyte)PersonCategory.UNBLOCKED: ftPerson.Category,
                    LibId = ftPerson.LibId,
                    LibIdGroups = ftPerson.LibIdGroups,
                    PersonId = ftPerson.Id,
                    Name = ftPerson.Name,
                    CardNo = ftPerson.CardNo,
                    CreateTime = ftPerson.CreateTime,
                    UpdateTime = ftPerson.UpdateTime
                };
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 由於獲取到的卡號可能是工號(EmployeeId 工號) (這是測試出現的情況) 
        /// 解決: 判斷卡號是否 為工號
        /// </summary>
        /// <param name="maincomId"></param>
        /// <param name="physicalId"></param>
        /// <param name="personCardInfo"></param>
        /// <returns></returns>
        public static bool IsPersonOuterId(string maincomId, string physicalId, ref PersonCardInfo personCardInfo)
        {
            personCardInfo = null;
            if (string.IsNullOrEmpty(maincomId) || string.IsNullOrEmpty(physicalId))
            {
                return false;
            }
            using BusinessContext businessContext = new BusinessContext();

            FtPerson ftPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(physicalId) && c.MaincomId.Contains(maincomId)).FirstOrDefault();

            if (ftPerson != null)
            {
                personCardInfo = new PersonCardInfo
                {
                    MaincomId = ftPerson.MaincomId,
                    OuterId = ftPerson.OuterId,
                    Category = (sbyte)ftPerson.Category,   //ftPerson.Category==0 ? (sbyte)PersonCategory.UNBLOCKED: ftPerson.Category,
                    LibId = ftPerson.LibId,
                    LibIdGroups = ftPerson.LibIdGroups,
                    PersonId = ftPerson.Id,
                    Name = ftPerson.Name,
                    CardNo = string.Empty,
                    CreateTime = ftPerson.CreateTime,
                    UpdateTime = ftPerson.UpdateTime
                };
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 根據設備ID獲得設備具體位置
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static string GetDeviceSiteName(int deviceId, ref int siteId)
        {
            if (deviceId == 0)
            {
                siteId = 0;
                return string.Empty;
            }
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                siteId = 0;
                return string.Empty;
            }

            siteId = device.SiteId;
            if (siteId == 0)
            {
                return string.Empty;
            }
            string siteName = businessContext.FtSite.Find(siteId)?.SiteName ?? String.Empty;
            return siteName;
        }

        /// <summary>
        /// 根據 [設備] 獲得設備具體位置
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static string GetDeviceSiteName(FtDevice device)
        {
            if (device == null)
            {
                return string.Empty;
            }
            using BusinessContext businessContext = new BusinessContext();

            int siteId = device.SiteId;
            if (siteId == 0)
            {
                return string.Empty;
            }
            string siteName = businessContext.FtSite.Find(siteId)?.SiteName ?? String.Empty;
            return siteName;
        }

        /// <summary>
        /// 考勤模式和設備類型的轉換
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public static AttendanceMode DeviceTypeToAttendanceMode(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.UNDEFINED_DEVICE:
                    return AttendanceMode.COMBINE_VERIFY;  //無法對應上
                case DeviceType.ANDROID_NFC:
                    return AttendanceMode.STANDARD_CARD;
                case DeviceType.APPLE_MAP:
                    return AttendanceMode.GPS;
                case DeviceType.GPS:
                    return AttendanceMode.GPS;
                case DeviceType.GOOGLE:
                    return AttendanceMode.GPS;
                case DeviceType.QQ: //qq map
                    return AttendanceMode.GPS;
                case DeviceType.BAIDU: //baidu map
                    return AttendanceMode.GPS;
                case DeviceType.CAM_GUARD:
                    return AttendanceMode.CAM_GUARD;
                case DeviceType.CARD_FINGERPRINT_PASSWD:
                    return AttendanceMode.STANDARD_CARD;
                case DeviceType.CIC_CARD:
                    return AttendanceMode.CIC_CARD;
                case DeviceType.HIK_CARD:
                    return AttendanceMode.HIK_CARD;
                case DeviceType.HIK_DS_KIT341BMW:
                    return AttendanceMode.HIK_CARD;
                case DeviceType.HIK_DS_KIT804MF:
                    return AttendanceMode.HIK_CARD;
                case DeviceType.NFCCIC_CHECK:
                    return AttendanceMode.CIC_CARD;
                case DeviceType.NFCCIC_TAP:
                    return AttendanceMode.CIC_CARD;
                case DeviceType.PASSWORD:
                    return AttendanceMode.STANDARD_CARD;
                case DeviceType.STANDARD_CARD_AND_FINGERPRINT:
                    return AttendanceMode.STANDARD_CARD_AND_FINGERPRINT;
                case DeviceType.FINGERPRINT:
                    return AttendanceMode.STANDARD_CARD_AND_FINGERPRINT;
                case DeviceType.DESTOP_DVR: //AIG系統 如果兼備 識別功能,則歸納到此處
                    return AttendanceMode.FACE;
                //默認情況是標準咔
                default:
                    return AttendanceMode.STANDARD_CARD;
            }
        }

        /// <summary>
        /// 取得設備Bear形式的 Token = md5(userName:Password)
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static string GetDeviceBearToken(int deviceId)
        {
            if (deviceId == 0)
            {
                return string.Empty;
            }

            using BusinessContext businessContext = new BusinessContext();

            DeviceDVRModel deviceDVRModel = new DeviceDVRModel();
            deviceDVRModel.ToInstant(deviceId);

            //不允許無密碼
            if (string.IsNullOrEmpty(deviceDVRModel.Password))
            {
                return string.Empty;
            }

            string tokenTxt = $"{deviceDVRModel.Account}:{deviceDVRModel.Password}";

            string md5Token = CommonBase.md5Encode(tokenTxt);

            return md5Token;
        }



        public static StringBuilder Result = new StringBuilder();
        private static StringBuilder sb = new StringBuilder();
        /// <summary>
        /// 取得設備下的所有位置關聯鏡頭查詢
        /// </summary>
        /// <param name="ftSites"></param>
        /// <param name="parentsId"></param>
        public static void GetDeviceCameraNodeOfSites(List<DeviceSiteTreeModel> deviceSiteTrees, string maincomId, FtDevice device)
        {
            List<DeviceSiteTreeModel> allDeviceSites = GetDeviceSites(device.DeviceId);
            Result.Append(sb);
            sb.Clear();
            if (deviceSiteTrees?.Count() > 0)
            {
                sb.Append("\n[");

                foreach (var row in deviceSiteTrees)
                {
                    sb.Append("{\"nodeid\":" + row.SiteId + ",\"text\":\"" + row.SiteName + "\",\"parentsId\":" + row.ParentsId + ",\"deviceId\":" + row.DeviceId);

                    bool hasCameras = GetDeviceCameraList(maincomId, row.DeviceId, row.SiteId, out string camNodesJson);
                    if (hasCameras)
                    {
                        sb.Append(",\"nodes\":");
                        sb.Append(camNodesJson);
                    }

                    List<DeviceSiteTreeModel> subOfDeviceSiteTrees = allDeviceSites.Where(c => c.DeviceId == device.DeviceId && c.ParentsId == row.SiteId).ToList();

                    if (subOfDeviceSiteTrees?.Count > 0)
                    {
                        sb.Append(",\"nodes\":");
                        GetDeviceCameraNodeOfSites(subOfDeviceSiteTrees, maincomId, device);
                        Result.Append(sb);
                        sb.Clear();
                    }
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::GetDeviceCameraNodeOfSites][{sb}]");
                    Result.Append(sb);
                    sb.Clear();
                    sb.Append("},");
                }
                sb = sb.Remove(sb.Length - 1, 1);
                sb.Append("]");
                Result.Append(sb);
                sb.Clear();
            }
        }
        /// <summary>
        /// 查詢與鏡頭想關聯的位置 用於樹狀圖列出位置 和位置下的鏡頭
        /// </summary>
        /// <param name="deviceId"></param> 
        /// <returns></returns>
        public static List<DeviceSiteTreeModel> GetDeviceSites(int deviceId, int siteId = 0)
        {
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Where(c => c.DeviceId == deviceId).FirstOrDefault();

            if (device == null)
                return null;

            var sites = businessContext.FtSite.Where(c => c.MaincomId.Contains(device.MaincomId)).ToList();
            var cameras = businessContext.FtCamera.Where(c => c.DeviceId == device.DeviceId && c.Visible == (int)CameraErrorCode.CAM_IS_VISIBLE).ToList();


            var siteIdCameras = cameras.Select(s => new { s.SiteId }).Distinct().ToList();

            var cameraSites = from s in sites
                              join x in siteIdCameras on s.SiteId equals x.SiteId
                              join c in cameras on s.SiteId equals c.SiteId
                              select new { c.Id, c.Name, c.DeviceId, c.DeviceName, s.SiteId, s.SiteName, s.MaincomId, s.ParentsId };


            if (siteId != 0)
            {
                cameraSites = cameraSites.Where(c => c.SiteId == siteId);
            }

            if (cameraSites.Any())
            {
                cameraSites = cameraSites.Distinct().ToList();  //Distinct排除重复的

                List<DeviceSiteTreeModel> list = cameraSites.Select(s => new DeviceSiteTreeModel
                {
                    NodeId = s.Id,
                    Text = s.Name,
                    SiteId = s.SiteId,
                    ParentsId = s.ParentsId ?? 0,
                    SiteName = s.SiteName,
                    DeviceId = device.DeviceId,
                    DeviceName = device.DeviceName,
                    MainComId = device.MaincomId,
                    CameraId = s.Id,
                    CameraName = s.Name
                }).ToList();
                int count = list.Count();
                string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::GetDeviceSites][count={count}]\n[{JsonConvert.SerializeObject(list)}]\n";
                Console.WriteLine(loggerline);
                return list;
            }
            else
            {
                return null;
            }
        }
        private static StringBuilder camSb = new StringBuilder();
        /// <summary>
        /// 獲取位置下的鏡頭列表
        /// {[{"nodeid":6,"text":"CAM232AXIS,"cameraId":6","deviceId":3006","siteId":10}]}
        /// </summary>
        /// <param name="siteId"></param>
        public static bool GetDeviceCameraList(string mainComId, int deviceId, int siteId, out string camNodesJson)
        {
            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::GetDeviceCameraList][deviceId ={deviceId}][siteId = {siteId}]";
            Console.WriteLine(loggerline);

            using BusinessContext businessContext = new BusinessContext();
            var cameraNodesList = businessContext.FtCamera.Where(c => c.MaincomId.Contains(mainComId) && c.DeviceId == deviceId && c.SiteId == siteId && c.Visible == (int)CameraErrorCode.CAM_IS_VISIBLE).ToList();

            if (cameraNodesList?.Count() > 0)
            {
                camSb.Append("\n[");

                foreach (var row in cameraNodesList)
                {
                    camSb.Append("{\"nodeid\":" + row.Id + ",\"text\":\"" + row.Name + "\",\"cameraId\":" + row.Id + ",\"deviceId\":" + row.DeviceId + ",\"siteId\":" + row.SiteId + "},");
                }

                if (camSb.Length > 1)
                    camSb = camSb.Remove(camSb.Length - 1, 1);

                camSb.Append("]");
                camNodesJson = camSb.ToString();
                camSb.Clear();
                return true;
            }
            else
            {
                camNodesJson = string.Empty;
                return false;
            }
        }
    }
    public class CameraDeviceSetting
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceSerialNo { get; set; }
    }

    public class DeviceEntryX
    {
        public string MainComId { get; set; }
        [DefaultValue(-1)]
        public int Mode { get; set; }
        public string SiteId { get; set; }
        public string DeviceSerialNo { get; set; }
        public string HolderNameOfAccessCard { get; set; }
        public long OccurDateTime { get; set; }
        public string AccesscardId { get; set; }
    }
}
