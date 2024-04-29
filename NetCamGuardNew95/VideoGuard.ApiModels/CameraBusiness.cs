using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.CamApiModel;
using static EnumCode.EnumBusiness;

namespace VideoGuard.Business
{
    public class CameraBusiness
    {
        public static string CameraStreamInfo_Prefix { get; set; } = "CameraStreamInfo_";  //MPEG4REC项目

        /// <summary>
        /// 生成 配置表 FtConfig的主键ID,格式如: DEVICE_HIK_DS_KIT341BMW_3001
        /// 優先獲取id降序排列的第一個
        /// </summary>
        /// <param name="configType">CAMERA</param>
        /// <param name="cameraId"></param>
        /// <returns>DEVICE_HIK_DS_KIT341BMW_3001</returns>
        public static string GeneratescheduleConfigName(EnumBusiness.ConfigType configType, int cameraId)
        {
            string configName = $"CAMERA_SCHEDULE{configType}_{cameraId}";
            return configName;
        }
        /// <summary>
        /// 通過按規則生成的ConfigName獲取配置,如果沒有返回null
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public static FtConfig GenerateCamerascheduleConfigName(EnumBusiness.ConfigType configType, int cameraId)
        {
            string configName = GeneratescheduleConfigName(configType, cameraId);
            using BusinessContext businessContext = new BusinessContext();
            var config = businessContext.FtConfig.OrderByDescending(c => c.Id).Where(c => c.Name.Contains(configName)).FirstOrDefault();
            return config;
        }
        /// <summary>
        /// 返回配置的對象 object 沒有則返回null
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static object ReturnCameraConfigObject(int cameraId)
        {
            if (cameraId == 0)
                return null;

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtCamera.Find(cameraId);
            if (device == null)
                return null;


            FtConfig ftConfig = GenerateCamerascheduleConfigName(ConfigType.CAMERA, cameraId);
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
        /// 保存 镜头录像 配置
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="objForJson"></param>
        /// <returns></returns>
        public static bool SaveCamerScheduleConfigForCamera(int cameraId, object objForJson)
        {
            if (cameraId == 0)
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

                string cameraScheduleConfAJson = JsonConvert.SerializeObject(objForJson, jsonSettings);

                if (string.IsNullOrEmpty(cameraScheduleConfAJson))
                    return false;

                string configName = GeneratescheduleConfigName(ConfigType.CAMERA, cameraId);

                FtConfig ftConfig = businessContext.FtConfig.Where(c => c.Name.Contains(configName)).FirstOrDefault();

                if (ftConfig != null)
                {
                    ftConfig.Visible = (int)GeneralVisible.VISIBLE;
                    ftConfig.Config = cameraScheduleConfAJson ?? String.Empty;
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
                        string logline = $"[FUNC::CameraBusiness.SaveCamerScheduleConfigForCamera()][UPDATE][EXCEPTION][{ex.Message}]";
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
                        Config = cameraScheduleConfAJson ?? String.Empty,
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
                        string logline = $"[FUNC::CameraBusiness.SaveCamerScheduleConfigForCamera()][ADD][EXCEPTION][{ex.Message}]";
                        CommonBase.ConsoleWriteline(logline);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 獲取鏡頭詳細信息 包括SiteName
        /// 如果沒有返回 Null
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static Camera CameraDetails(int cameraId, ref ResponseModalX responseModalX)
        {
            using BusinessContext businessContext = new BusinessContext();

            FtCamera ftCamera = businessContext.FtCamera.Find(cameraId);
            if (ftCamera != null)
            {
                var sites = businessContext.FtSite.Select(s => new { s.SiteId, s.SiteName, s.MaincomId }).Where(c => c.MaincomId.Contains(ftCamera.MaincomId)).ToList();

                string CamVisibleDesc = ftCamera.Visible.GetValueOrDefault() == 1 ? Lang.CAM_IS_VISIBLE : Lang.CAM_NOT_VISIBLE;
                EnumBusiness.DeviceType deviceType = EnumBusiness.DeviceType.UNDEFINED_DEVICE;
                deviceType = (EnumBusiness.DeviceType)businessContext.FtDevice.Find(ftCamera.DeviceId).DeviceType;
                Camera camera = new Camera
                {
                    MaincomId = ftCamera.MaincomId,
                    CameraId = ftCamera.Id,
                    SiteId = ftCamera.SiteId,
                    SiteName = sites.Where(c => c.SiteId == ftCamera.SiteId).FirstOrDefault()?.SiteName ?? "-",
                    Name = ftCamera.Name,
                    CameraIp = ftCamera.Ip ?? "0.0.0.0",
                    Type = ftCamera.Type.GetValueOrDefault(),
                    Online = CamVisibleDesc,
                    Remark = ftCamera.Remark,
                    Rtsp = ftCamera.Rtsp,
                    CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftCamera.CreateTime),
                    RecordStatus = (CameraRecordStatus)ftCamera.RecordStatus,
                    DeviceType = deviceType,
                    DeviceName = ftCamera.DeviceName,
                    DeviceId = ftCamera.DeviceId,
                    Onlive = Convert.ToBoolean(ftCamera.OnLive)
                };

                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = true, ErrorCode = 0, Message = Lang.GeneralUI_SUCC },
                    data = camera
                };
                return camera;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = $"{Lang.CAM_GET_DETAILS_FAIL} [CamerId = {cameraId}]" },
                    data = null
                };
                return null;
            }
        }
        /// <summary>
        /// 獲取鏡頭detail
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public static Camera CameraDetails(int cameraId)
        {
            using BusinessContext businessContext = new BusinessContext();

            FtCamera ftCamera = businessContext.FtCamera.Find(cameraId);
            if (ftCamera != null)
            {
                var sites = businessContext.FtSite.Select(s => new { s.SiteId, s.SiteName, s.MaincomId }).Where(c => c.MaincomId.Contains(ftCamera.MaincomId)).ToList();

                string CamVisibleDesc = ftCamera.Visible.GetValueOrDefault() == 1 ? Lang.CAM_IS_VISIBLE : Lang.CAM_NOT_VISIBLE;
                EnumBusiness.DeviceType deviceType = EnumBusiness.DeviceType.UNDEFINED_DEVICE;
                deviceType = (EnumBusiness.DeviceType)businessContext.FtDevice.Find(ftCamera.DeviceId).DeviceType;
                Camera camera = new Camera
                {
                    MaincomId = ftCamera.MaincomId,
                    CameraId = ftCamera.Id,
                    SiteId = ftCamera.SiteId,
                    SiteName = sites.Where(c => c.SiteId == ftCamera.SiteId).FirstOrDefault()?.SiteName ?? "-",
                    Name = ftCamera.Name,
                    CameraIp = ftCamera.Ip ?? "0.0.0.0",
                    Type = ftCamera.Type.GetValueOrDefault(),
                    Online = CamVisibleDesc,
                    Remark = ftCamera.Remark,
                    Rtsp = ftCamera.Rtsp,
                    CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftCamera.CreateTime),
                    RecordStatus = (CameraRecordStatus)ftCamera.RecordStatus,
                    DeviceType = deviceType,
                    DeviceName = ftCamera.DeviceName,
                    DeviceId = ftCamera.DeviceId,
                    Onlive = Convert.ToBoolean(ftCamera.OnLive)
                };
                return camera;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 通過鏡頭ID獲取直播URL
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static string GetCamOnliveUrl(string mainComId, int cameraId, out int deviceId, out DeviceDVRModel deviceDVRModel, ref ResponseModalX responseModalX)
        {
            deviceId = 0;
            deviceDVRModel = new DeviceDVRModel();
            Camera camera = CameraDetails(cameraId, ref responseModalX);
            string camOnliveUrl = string.Empty;

            if (responseModalX.meta.Success == false)
            {
                return camOnliveUrl;
            }
            else
            {
                if (camera.MaincomId != mainComId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                    return camOnliveUrl;
                }

                deviceId = camera.DeviceId;
                //方式I
                //Object格式必須是DVR配置對象
                //object obj = DeviceBusiness.ReturnDeviceConfigObject(camera.DeviceId); //這個只是For 前端請求 
                //方式II

                deviceDVRModel.ToInstant(camera.DeviceId);
                camOnliveUrl = DeviceBusiness.RequestCameraHlsUrlFormat(deviceDVRModel, cameraId); //string.Format(AIG_LIVE_FORMAT, deviceDVRModel.DvrIp, deviceDVRModel.DvrPort, cameraId);

                return camOnliveUrl;
            }
        }

        /// <summary>
        /// 获得镜头的历史记录
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool GetCamHistList(int cameraId, long timeScaleValue, bool isRecent, ref ResponseModalX responseModalX)
        {
            if (cameraId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_EXIST_RECORD, Message = Lang.GeneralUI_ExistRecord };
                return false;
            }
            using BusinessContext businessContext = new BusinessContext();

            Camera camera = CameraDetails(cameraId);

            if (camera != null)
            {
                DateTime dt = DateTime.Now;

                if (timeScaleValue == 0)
                {
                    timeScaleValue = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
                }

                long timeScaleFisrtDate = timeScaleValue - (long)dt.TimeOfDay.TotalMilliseconds;
                double tsOfAday = new TimeSpan(23, 59, 59).TotalMilliseconds;
                long timeScaleLastDate = timeScaleFisrtDate + (long)tsOfAday;


                //whole day
                var cameraMpegs = businessContext.FtCameraMpeg.Select(c => new { c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible, c.IsUpload })
                                                                .Where(c => c.CameraId == cameraId
                                                                && c.StartTimestamp >= timeScaleFisrtDate && c.StartTimestamp <= timeScaleLastDate
                                                                && c.Visible == (int)CamMpegVisible.VISIBLE
                                                                && c.IsUpload == (sbyte)CamMpegIsUpload.DVR_RECORD_DEFAULT
                                                                && c.IsGroup == Convert.ToUInt64(false));

                //query the to the  scale
                cameraMpegs = cameraMpegs.Where(c => c.StartTimestamp >= timeScaleValue);

                //當前刻度查詢的記錄
                Console.WriteLine($"/t[{dt:yyyy-MM-dd HH:mm:ss fff}][FUNC::CameraBusiness.GetCamHistList][SCALE QUERY COUNT={cameraMpegs?.Count() ?? 0}][timeScaleFisrtDate:{DateTimeHelp.ConvertToDateTime(timeScaleFisrtDate):yyyy-MM-dd HH:mm:ss fff}][timeScaleFisrtDate:{DateTimeHelp.ConvertToDateTime(timeScaleFisrtDate):yyyy-MM-dd HH:mm:ss fff}][timeScaleValue={timeScaleValue}]");

                if (cameraMpegs?.Count() == 0 && isRecent == false)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"[{Lang.GeneralUI_NoRecord}][Camera={cameraId}][TimeScale={timeScaleValue}({dt:F})]" };
                    return false;
                }
                else if (cameraMpegs?.Count() == 0 && isRecent)
                {
                    DateTime nearThreeMonth = DateTime.Now.AddMonths(-3);
                    long nearThreeMonthL = new DateTimeOffset(nearThreeMonth).ToUnixTimeMilliseconds();
                    //如果上述查詢沒有,並且可以最近記錄,則 獲取最近大個月的
                    cameraMpegs = businessContext.FtCameraMpeg.Select(c => new { c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible, c.IsUpload })
                                                                .Where(c => c.CameraId == cameraId
                                                                && c.StartTimestamp >= nearThreeMonthL
                                                                && c.Visible == (int)CamMpegVisible.VISIBLE
                                                                && c.IsUpload == (sbyte)CamMpegIsUpload.DVR_RECORD_DEFAULT
                                                                && c.IsGroup == Convert.ToUInt64(false));
                }

                List<CamHistX> camHists = new List<CamHistX>();

                CamHistStyle camHistStyleMpeg = new CamHistStyle { Background = "rgba(7, 42, 219, 0.598039)" };   // deep blue
                CamHistStyle camHistStyleFlv = new CamHistStyle { Background = "rgba(143, 157, 227, 0.598039)" }; // light blue

                foreach (var item in cameraMpegs)
                {
                    CamHistX camHistx = new CamHistX
                    {
                        BeginTime = item.StartTimestamp,
                        EndTime = item.EndTimestamp,
                        Style = camHistStyleMpeg
                    };

                    if (item.FileFomat.ToLower() == "flv")
                        camHistx.Style = camHistStyleFlv;

                    camHists.Add(camHistx);
                }

#if DEBUG 
                int j = 0;
                int i = 0;
                DateTime fromThisTime = DateTime.Now.AddHours(-24);
                long fromThisTimeStamp = new DateTimeOffset(fromThisTime).ToUnixTimeMilliseconds();
                //多少影片 96  = 1440 (24Hours / 15mins)
                for (int x = 0; x < 96; x++)
                {
                    CamHistStyle camHistStyle = new CamHistStyle { Background = "rgba(247, 5, 207, 0.499999)" };

                    long fromEndimeStamp = fromThisTimeStamp;
                    j -= 15 * 60 * 1000;
                    i++;
                    fromThisTimeStamp = fromThisTimeStamp - j;

                    CamHistX camHist = new CamHistX
                    {
                        BeginTime = fromThisTimeStamp,
                        EndTime = fromEndimeStamp,
                        Style = camHistStyle
                    };
                    DateTime dt111 = DateTimeHelp.ConvertToDateTime(fromThisTimeStamp);
                    DateTime dt222 = DateTimeHelp.ConvertToDateTime(fromEndimeStamp);
                    Console.WriteLine($"[CameraBusiness.GetCamHistListX][{j}][Start:{dt111:F}] - [Start:{dt222:F}]");
                    if (i == 5 || i == 10 || i == 20)
                    {
                        Console.WriteLine($"[JUMB][{j}]");
                        continue;
                    }

                    camHists.Add(camHist);
                }
#endif

                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = true, ErrorCode = 0, Message = Lang.GeneralUI_SUCC },
                    data = camHists
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = $"{Lang.CAM_GET_DETAILS_FAIL} [CamerId = {cameraId}]" },
                    data = null
                };
                return false;
            }
        }

        /// <summary>
        /// 获得镜头的历史记录(简单的数据单元测试) 
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool GetCamHistListX(int cameraId, ref ResponseModalX responseModalX)
        {
            if (cameraId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_EXIST_RECORD, Message = Lang.GeneralUI_ExistRecord };
                return false;
            }
            using BusinessContext businessContext = new BusinessContext();

            FtCamera camera = businessContext.FtCamera.Find(cameraId);

            if (camera != null)
            {
                DateTime dtStart = DateTime.Now.AddMonths(-3);
#if DEBUG
                dtStart = DateTime.Now.AddDays(-9);
#endif
                DateTime dtEnd = DateTime.Now;
                long startTimeStamp = new DateTimeOffset(dtStart).ToUnixTimeMilliseconds();
                long endTimeStamp = new DateTimeOffset(dtEnd).ToUnixTimeMilliseconds();
                var cameraMpegs = businessContext.FtCameraMpeg.Select(c => new { c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible })
                                                                .Where(c => c.CameraId == cameraId
                                                                && c.StartTimestamp >= startTimeStamp && c.EndTimestamp <= endTimeStamp
                                                                && c.Visible == (int)CamMpegVisible.VISIBLE
                                                                && c.IsGroup == Convert.ToUInt64(false));
#if DEBUG
                cameraMpegs = cameraMpegs.Take(60);
#endif

                if (cameraMpegs?.Count() == 0)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_ExistRecord };
                    return false;
                }

                List<CamHistX> camHists = new List<CamHistX>();
                int j = 0;
                int i = 0;
                DateTime fromThisTime = DateTime.Now.AddHours(-36);
                long fromThisTimeStamp = new DateTimeOffset(fromThisTime).ToUnixTimeMilliseconds();

                foreach (var item in cameraMpegs)
                {
                    CamHistStyle camHistStyle = new CamHistStyle { Background = "rgba(132, 144, 180, 0.488038)" };

#if DEBUG
                    camHistStyle = new CamHistStyle { Background = "rgba(247, 5, 207, 0.499999)" };
#endif
                    long fromEndimeStamp = fromThisTimeStamp;
                    j -= 15 * 60 * 1000;
                    i++;
                    fromThisTimeStamp = fromThisTimeStamp - j;

                    CamHistX camHist = new CamHistX
                    {
                        BeginTime = fromThisTimeStamp,
                        EndTime = fromEndimeStamp,
                        Style = camHistStyle
                    };
                    DateTime dt111 = DateTimeHelp.ConvertToDateTime(fromThisTimeStamp);
                    DateTime dt222 = DateTimeHelp.ConvertToDateTime(fromEndimeStamp);
                    Console.WriteLine($"[CameraBusiness.GetCamHistListX][{j}][Start:{dt111:F}] - [Start:{dt222:F}]");
                    if (i == 5 || i == 10 || i == 20)
                    {
                        Console.WriteLine($"[JUMB][{j}]");
                        continue;
                    }

                    camHists.Add(camHist);
                }
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = true, ErrorCode = 0, Message = Lang.GeneralUI_SUCC },
                    data = camHists
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = $"{Lang.CAM_GET_DETAILS_FAIL} [CamerId = {cameraId}] [NO CAMERA]" },
                    data = null
                };
                return false;
            }
        }

        /// <summary>
        /// 获取时间点范围内对应的media 录像文件，返回播放链接
        /// 规则：返回此时间点 最近的录像 
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="timepoint"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool GetCamHistTimePointMediaFile(int cameraId, long timepoint, ref ResponseModalX responseModalX)
        {
            if (cameraId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_EXIST_RECORD, Message = $"[{Lang.Camera_CameraId} {Lang.GeneralUI_ExistRecord}]" };
                return false;
            }

            if (timepoint.ToString().Length != 13 && timepoint.ToString().Length == 10)
                timepoint = timepoint * 1000;

            using BusinessContext businessContext = new BusinessContext();

            Camera camera = CameraDetails(cameraId);

            if (camera != null)
            {
                long seek = 0;//默認是從開始刻度播放
                var cameraMpeg = businessContext.FtCameraMpeg.Select(c => new { c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible, c.IsUpload })
                                                                .Where(c => c.CameraId == cameraId
                                                                && c.StartTimestamp >= timepoint && c.EndTimestamp <= timepoint
                                                                && c.Visible == (int)CamMpegVisible.VISIBLE
                                                                && c.IsGroup == Convert.ToUInt64(false)).OrderByDescending(c => c.EndTimestamp).FirstOrDefault();

                //返回播放時間尋址(如適用) 情況: 當點選的播放時間是影片之間的情況
                if (cameraMpeg != null)
                {
                    DateTime startTime = DateTimeHelp.ConvertToDateTime(cameraMpeg.StartTimestamp);
                    DateTime endTime = DateTimeHelp.ConvertToDateTime(cameraMpeg.EndTimestamp);
                    TimeSpan mediaTimeSpan = endTime.Subtract(startTime);
                    DateTime timepointTime = DateTimeHelp.ConvertToDateTime(timepoint);
                    TimeSpan seekTimeStamp = timepointTime.Subtract(startTime);
                    if (mediaTimeSpan > seekTimeStamp)
                    {
                        seek = (int)seekTimeStamp.TotalSeconds;
                    }
                }
                if (cameraMpeg == null)
                {
                    cameraMpeg = businessContext.FtCameraMpeg.Select(c => new { c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible, c.IsUpload })
                                                                .Where(c => c.CameraId == cameraId
                                                                && c.EndTimestamp <= timepoint
                                                                && c.Visible == (int)CamMpegVisible.VISIBLE
                                                                && c.IsGroup == Convert.ToUInt64(false)).OrderByDescending(c => c.EndTimestamp).FirstOrDefault();
                    //時間點查詢不到影片,返回是最近的一個影片
                    if (cameraMpeg != null)
                    {
                        seek = 0;
                        if (cameraMpeg.StartTimestamp.ToString().Length != 13)
                            timepoint = cameraMpeg.StartTimestamp * 1000;
                        else
                            timepoint = cameraMpeg.StartTimestamp;
                    }
                }

                if (cameraMpeg == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][{Lang.GeneralUI_NoRecord}::cameraMpeg==null][line:500]");
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"{Lang.GeneralUI_NoRecord} （[FUNC::GetCamHistTimePointMediaFile][cameraMpeg==null]）" };
                    return false;
                }

                CamHistStyle camHistStyleMpeg = new CamHistStyle { Background = "rgba(33, 252, 95, 0.598039)" }; //green
                CamHistStyle camHistStyleFlv = new CamHistStyle { Background = "rgba(212, 33, 252, 0.598039)" }; // voilet

                CamHistX camHistX = new CamHistX
                {
                    BeginTime = cameraMpeg.StartTimestamp,
                    EndTime = cameraMpeg.EndTimestamp,
                    Style = camHistStyleMpeg
                };

                if (cameraMpeg.FileFomat.ToLower() == "flv")
                    camHistX.Style = camHistStyleFlv;

                DeviceDVRModel deviceDVRModel = new DeviceDVRModel();
                deviceDVRModel.ToInstant(camera.DeviceId);

                CameraMpeg cameraMpegX = new CameraMpeg
                {
                    Id = cameraMpeg.Id,
                    DeviceId = cameraMpeg.DeviceId,
                    CameraId = cameraMpeg.CameraId,
                    MpegFilename = cameraMpeg.MpegFilename,
                    FileFomat = cameraMpeg.FileFomat,
                    IsGroup = cameraMpeg.IsGroup,
                    StartTimestamp = cameraMpeg.StartTimestamp,
                    EndTimestamp = cameraMpeg.EndTimestamp,
                    Visible = cameraMpeg.Visible,
                    IsUpload = cameraMpeg.IsUpload
                };

                string mediaUrlFormat = DeviceBusiness.RequestMediaUrlFormat(deviceDVRModel, cameraMpegX);

                string playUrl = mediaUrlFormat;

                TimePointData timePointData = new TimePointData
                {
                    CamHistX = camHistX,
                    Timepoint = timepoint,
                    Seek = seek,
                    FileFomat = cameraMpeg.FileFomat,
                    CamHistWithUrl = new CamHistWithUrl { PlayUrl = playUrl },
                    Camera = camera
                };

                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = true, ErrorCode = 0, Message = Lang.GeneralUI_SUCC },
                    data = timePointData
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = $"{Lang.CAM_GET_DETAILS_FAIL} [CamerId = {cameraId}]" },
                    data = null
                };
                return false;
            }
        }
    }
}
