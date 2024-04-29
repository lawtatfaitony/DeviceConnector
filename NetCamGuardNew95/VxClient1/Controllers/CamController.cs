using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.CamApiModel;
using VxGuardClient.ExternalDll;
using X.PagedList;
using LogUtility;
using VxGuardClient.Context;
using VideoGuard.Business;
using VxClient.Models;
using Microsoft.Extensions.Logging;
using Common;
using Newtonsoft.Json;
using VxClient;

namespace VxGuardClient.Controllers
{
    public partial class CamController : BaseController
    {
        public CamController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, ILogger<BaseController> logger, IOptions<TokenManagement> tokenManagement)
            : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Add()
        {
            MainCom mainCom = new MainCom();
            CamApiModelInput camApiModelInput = new CamApiModelInput { Type = 0, SiteId = 0 };
            camApiModelInput.MaincomId ??= camApiModelInput.MaincomId ?? WebCookie.MainComId ?? mainCom.MainComId;

            ResponseModalX responseModalX = new ResponseModalX();
            responseModalX.data = camApiModelInput;
            return View(responseModalX);
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult Add(CamApiModelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            bool chkName = ChkTheSameCameraName(0, input.Name, out responseModalX);  //新增的判断情况 把cameraId设0 =没有
            if (chkName)
            {
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
            MetaModalX metaModalX = new MetaModalX();
            if (!Uri.IsWellFormedUriString(input.Rtsp, UriKind.Absolute))
            {
                metaModalX = new MetaModalX
                {
                    ErrorCode = (int)CameraErrorCode.ILLEGAL_RTSP,
                    Success = false,
                    Message = Lang.GeneralUI_ILLEGAL_RTSP
                };
                responseModalX.data = null;
                responseModalX.meta = metaModalX;
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
            if (input.SiteId == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = 0, Message = Lang.CAM_SITE_REQUIED, Success = false },
                    data = null
                };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            int maxId = 1200;
            if (businessContext.FtCamera.Count() > 0)
            {
                maxId = businessContext.FtCamera.Max(c => c.Id) + 1;
            }
            DateTime dt = DateTime.Now;
            CameraRtspInfo cameraRtspInfo = new CameraRtspInfo(input.Rtsp);
            MainCom mainCom = new MainCom(); //默認值  
            CameraDeviceSetting cameraDeviceSetting = DeviceBusiness.GetCameraDeviceSetting(input.DeviceId);
            FtCamera ftCamera = new FtCamera
            {
                Id = maxId,
                MaincomId = input.MaincomId ?? WebCookie.MainComId ?? mainCom.MainComId,
                SiteId = input.SiteId,
                Name = input.Name,
                Rtsp = input.Rtsp,
                Type = Convert.ToSByte(input.Type),
                Remark = input.Remark,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Ip = cameraRtspInfo.Ip,
                Username = cameraRtspInfo.User,
                Password = cameraRtspInfo.Password,
                RecordStatus = (int)CameraRecordStatus.IN_STOP,  //初始状态
                DeviceId = cameraDeviceSetting.DeviceId,
                DeviceName = cameraDeviceSetting.DeviceName,
                DeviceSerialNo = cameraDeviceSetting.DeviceSerialNo,
                OnLive = Convert.ToUInt64(false) //初始为不在线 非直播状态
            };
            businessContext.FtCamera.Add(ftCamera);
            bool result = businessContext.SaveChanges() > 0;
            if (result)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = 0, Message = Lang.GeneralUI_SUCC, Success = true },
                    data = ftCamera
                };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)CameraErrorCode.CAMERA_ADD_FAIL, Success = false, Message = Lang.CamAddNewResult_Fail },
                    data = null
                };
                return Ok(responseModalX);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Update(int CameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtCamera ftCamera = businessContext.FtCamera.Find(CameraId);
                if (ftCamera == null)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = Lang.CAM_GET_DETAILS_FAIL },
                        data = null
                    };
                    return SwitchToApiOrView(responseModalX);
                }
                else
                {
                    CamUpdate camUpdate = new CamUpdate
                    {
                        MaincomId = ftCamera.MaincomId,
                        CameraId = ftCamera.Id,
                        SiteId = ftCamera.SiteId,
                        SiteName = businessContext.FtSite.Find(ftCamera.SiteId)?.SiteName ?? "-",
                        Name = ftCamera.Name,
                        Type = (int)ftCamera.Type,
                        Rtsp = ftCamera.Rtsp,
                        Remark = ftCamera.Remark,
                        DeviceId = ftCamera.DeviceId
                    };
                    responseModalX.data = camUpdate;
                    return SwitchToApiOrView(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult Update([FromForm] CamUpdate camUpdate)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MetaModalX metaModalX = new MetaModalX();

            bool chkName = ChkTheSameCameraName(camUpdate.CameraId, camUpdate.Name, out responseModalX);
            if (chkName)
            {
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
            if (camUpdate.SiteId == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = 0, Message = Lang.CAM_SITE_REQUIED, Success = false },
                    data = null
                };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            if (!Uri.IsWellFormedUriString(camUpdate.Rtsp, UriKind.RelativeOrAbsolute))  ////!camUpdate.Rtsp.StartsWith("rtsp://",StringComparison.OrdinalIgnoreCase)
            {
                metaModalX = new MetaModalX
                {
                    ErrorCode = (int)CameraErrorCode.ILLEGAL_RTSP,
                    Success = false,
                    Message = Lang.GeneralUI_ILLEGAL_RTSP
                };
                responseModalX.data = null;
                responseModalX.meta = metaModalX;
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
            FtCamera ftCamera = businessContext.FtCamera.Find(camUpdate.CameraId);

            // CAMERA_NOT_EXIST
            if (ftCamera == null)
            {
                metaModalX = new MetaModalX
                {
                    ErrorCode = (int)CameraErrorCode.CAMERA_NOT_EXIST,
                    Success = false,
                    Message = Lang.CAM_CAMERA_NOT_EXIST
                };
                responseModalX.data = null;
                responseModalX.meta = metaModalX;
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
            else
            {
                if (camUpdate.MaincomId != ftCamera.MaincomId)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId },
                        data = null
                    };
                    return Ok(responseModalX);
                }

                CameraRtspInfo cameraRtspInfo = new CameraRtspInfo(camUpdate.Rtsp);
                ftCamera.Name = camUpdate.Name;
                ftCamera.Rtsp = camUpdate.Rtsp;
                ftCamera.Type = (sbyte)camUpdate.Type;
                ftCamera.SiteId = camUpdate.SiteId;
                ftCamera.Remark = camUpdate.Remark;
                ftCamera.Ip = cameraRtspInfo.Ip;
                ftCamera.Username = cameraRtspInfo.User;
                ftCamera.Password = cameraRtspInfo.Password;
                ftCamera.UpdateTime = DateTime.Now;

                if (ftCamera.DeviceId != camUpdate.DeviceId)
                {
                    CameraDeviceSetting cameraDeviceSetting = DeviceBusiness.GetCameraDeviceSetting(camUpdate.DeviceId);
                    ftCamera.DeviceId = cameraDeviceSetting.DeviceId;
                    ftCamera.DeviceName = cameraDeviceSetting.DeviceName;
                    ftCamera.DeviceSerialNo = cameraDeviceSetting.DeviceSerialNo;
                }
                //save-------------------------------------------------------
                businessContext.FtCamera.Update(ftCamera);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = 0, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = ftCamera
                    };
                    OkObjectResult okObjectResult = Ok(responseModalX);
                    return okObjectResult;
                }
                else
                {
                    metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)GeneralReturnCode.FAIL,
                        Success = false,
                        Message = Lang.CamUpdateResult_FAIL
                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = metaModalX,
                        data = null
                    };
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult CameraDetails(CameraIdInput cameraIdInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtCamera ftCamera = businessContext.FtCamera.Find(cameraIdInput.CameraId);
                if (ftCamera != null)
                {
                    var sites = businessContext.FtSite.Select(s => new { s.SiteId, s.SiteName, s.MaincomId }).Where(c => c.MaincomId.Contains(ftCamera.MaincomId)).ToList();

                    string CamVisibleDesc = ftCamera.Visible.GetValueOrDefault() == 1 ? Lang.CAM_IS_VISIBLE : Lang.CAM_NOT_VISIBLE;

                    Camera camera = new Camera
                    {
                        MaincomId = ftCamera.MaincomId,
                        CameraId = ftCamera.Id,
                        SiteId = ftCamera.SiteId,
                        SiteName = sites.Where(c => c.SiteId == ftCamera.SiteId).FirstOrDefault()?.SiteName ?? "-",
                        Name = ftCamera.Name,
                        CameraIp = ftCamera.Ip,
                        Type = ftCamera.Type.GetValueOrDefault(),
                        Online = CamVisibleDesc,
                        DeviceId = ftCamera.DeviceId,
                        DeviceName = ftCamera.DeviceName,
                        DeviceType = 0,  //暫時沒用到，不作查詢，臨時給個值過去
                        Onlive = ftCamera.OnLive == 1 ? true : false,
                        Remark = ftCamera.Remark,
                        Rtsp = ftCamera.Rtsp,
                        CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftCamera.CreateTime),
                        RecordStatus = (CameraRecordStatus)ftCamera.RecordStatus
                    };

                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = 0, Message = Lang.GeneralUI_SUCC },
                        data = camera
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAM_GET_DETAILS_FAIL, Message = Lang.CAM_GET_DETAILS_FAIL },
                        data = null
                    };
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult Delete(CameraDelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MetaModalX metaModalX = new MetaModalX();

            using (BusinessContext businessContext = new BusinessContext())
            {
                FtCamera ftCamera = businessContext.FtCamera.Find(input.CameraId);
                // CAMERA_NOT_EXIST
                if (ftCamera == null)
                {
                    metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)CameraErrorCode.CAMERA_NOT_EXIST,
                        Success = false,
                        Message = Lang.CAM_CAMERA_NOT_EXIST
                    };
                    responseModalX.data = null;
                    responseModalX.meta = metaModalX;
                    return Ok(responseModalX);
                }
                else
                {
                    //如果錄像或直播中都不能刪除設備
                    if (ftCamera.RecordStatus == (int)CameraRecordStatus.IN_RECORD || ftCamera.OnLive == Convert.ToUInt64(true))
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = false, ErrorCode = 0, Message = $"{Lang.CAM_ONLIVE_OR_IN_RECORD_REJECT_DEL}" },
                            data = ftCamera
                        };
                        return Ok(responseModalX);
                    }

                    //判断是否存在任务的引用此镜头
                    var task = businessContext.FtTask.Where(c => c.CameraList1.Contains(input.CameraId.ToString()) || c.CameraList2.Contains(input.CameraId.ToString())).FirstOrDefault();
                    if (task != null)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = false, ErrorCode = 0, Message = $"{Lang.CAM_TASK_EXISTS_REFERENCE} TASK:{task.Name} {task.Id}" },
                            data = ftCamera
                        };
                        return Ok(responseModalX);
                    }

                    //save-------------------------------------------------------
                    businessContext.FtCamera.Remove(ftCamera);
                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = 0, Message = Lang.GeneralUI_SUCC, Success = true },
                            data = ftCamera
                        };
                        return Ok(responseModalX);
                    }
                    else
                    {
                        metaModalX = new MetaModalX
                        {
                            ErrorCode = (int)GeneralReturnCode.FAIL,
                            Success = false,
                            Message = Lang.CamUpdateResult_FAIL
                        };
                        responseModalX = new ResponseModalX
                        {
                            meta = metaModalX,
                            data = null
                        };
                        return Ok(responseModalX);
                    }
                }
            }
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult QueryCameraList([FromQuery] QueryCameraListInput input)  //QueryCameraListInput queryInput //int? PageNo, int? PageSize, int? Type, string Name
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (!string.IsNullOrEmpty(input.Name))
            {
                input.Name = Uri.UnescapeDataString(input.Name).Trim();
            }

            input.MaincomId ??= WebCookie.MainComId;

            ViewBag.QueryCameraListInput = input;

            QueryCameraListInfoReturn queryCameraListInfoReturn = new QueryCameraListInfoReturn();
            List<Camera> items = new List<Camera>();
            try
            {
                using BusinessContext businessContext = new BusinessContext();

                var cameras = businessContext.FtCamera.Where(c => c.MaincomId.Contains(input.MaincomId)).AsNoTracking();
                var sites = businessContext.FtSite.Select(s => new { s.SiteId, s.SiteName, s.MaincomId }).AsNoTracking().Where(c => c.MaincomId.Contains(input.MaincomId)).ToList();
                var devices = businessContext.FtDevice.Select(c => new { c.MaincomId, c.DeviceType, c.DeviceId }).Where(c => c.MaincomId.Contains(input.MaincomId)).ToList();
                if (!string.IsNullOrEmpty(input.Name))
                {
                    cameras = cameras.AsNoTracking().Where(c => c.Name.Contains(input.Name));
                }
                if (input.Type != 0)
                {
                    cameras = cameras.AsNoTracking().Where(c => c.Type == input.Type);
                }

                if (input.DeviceId != 0)
                {
                    cameras = cameras.AsNoTracking().Where(c => c.DeviceId == input.DeviceId);
                }

                foreach (var item in cameras)
                {
                    var site = sites.Where(c => c.SiteId == item.SiteId).FirstOrDefault();
                    CameraVisible cameraVisible = (CameraVisible)item.Visible;
                    string siteName = site?.SiteName ?? "-";
                    EnumBusiness.DeviceType deviceType = EnumBusiness.DeviceType.UNDEFINED_DEVICE;
                    var devicex = devices.Where(c => c.DeviceId == item.DeviceId).FirstOrDefault();
                    if (devicex != null)
                    {
                        deviceType = (EnumBusiness.DeviceType)devicex.DeviceType;
                    }
                    Camera cameraX = new Camera
                    {
                        MaincomId = item.MaincomId,
                        CameraId = item.Id,
                        Name = item.Name,
                        SiteId = item.SiteId,
                        SiteName = siteName,
                        Rtsp = item.Rtsp,
                        Type = (int)item.Type,
                        Online = cameraVisible.GetEnumDesc(),   // item.Visible.ToString(), //item.Visible.HasValue ? item.Visible.ToString() : "0",
                        Remark = item.Remark,
                        CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", item.CreateTime),
                        DeviceId = item.DeviceId,
                        DeviceName = item.DeviceName,
                        RecordStatus = (CameraRecordStatus)item.RecordStatus,
                        DeviceType = deviceType,
                        Onlive = Convert.ToBoolean(item.OnLive)
                    };
                    items.Add(cameraX);
                }
                var newItems = items.ToPagedList(input.PageNo, input.PageSize);
                queryCameraListInfoReturn.PageCount = newItems.PageCount;
                queryCameraListInfoReturn.PageNo = newItems.PageNumber;
                queryCameraListInfoReturn.PageSize = input.PageSize;
                queryCameraListInfoReturn.TotalCount = items.Count();
                queryCameraListInfoReturn.Items = newItems.ToList();

                responseModalX.data = queryCameraListInfoReturn;

                return SwitchToApiOrView(responseModalX);
            }
            catch (Exception ex)
            {
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)CameraErrorCode.QUERY_CAMERA_LIST_FAIL, Success = false, Message = $"{Lang.QUERY_CAMERA_LIST_FAIL}:{ex.Message}" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
        }

        /// <summary>
        /// 獲取設備的鏡頭列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult CameraList([FromBody] CameraListOfDeviceInput input)
        {
            try
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FUNC:CamController.CameraList][INPUT][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }
            catch (Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FUNC:CamController.CameraList][EXCEPTION][{ex.Message}][{JsonConvert.SerializeObject(input)}]";
                Logger.LogError(loggerLine);
                LogHelper.Fatal(loggerLine);
            }

            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.MaincomId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId}-{input.DeviceId}" };
                return Ok(responseModalX);
            }

            List<CameraRtspDetail> cameraLists = new List<CameraRtspDetail>();
            try
            {
                using BusinessContext businessContext = new BusinessContext();
                var device = businessContext.FtDevice.Find(input.DeviceId);
                if (device == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Message = $"{Lang.Config_FAIL} {DeviceErrorCode.DEVICE_NOT_EXIST.GetEnumDesc()}" };
                    return Ok(responseModalX);
                }

                EnumBusiness.DeviceType deviceType = (EnumBusiness.DeviceType)device.DeviceType;

                var cameras = businessContext.FtCamera.Where(c => c.MaincomId.Contains(input.MaincomId) && c.DeviceId == input.DeviceId && c.Visible == (int)CameraVisible.VISIBLE).AsNoTracking();

                var sites = businessContext.FtSite.Select(s => new { s.SiteId, s.SiteName, s.MaincomId }).AsNoTracking().Where(c => c.MaincomId.Contains(input.MaincomId)).ToList();
                var devices = businessContext.FtDevice.Select(c => new { c.MaincomId, c.DeviceType, c.DeviceId }).Where(c => c.MaincomId.Contains(input.MaincomId));

                //添加一個默認的模型 後續開發添加完善 2024-3-14
                ModelSetting modelSetting = new ModelSetting();
                List<ModelSetting> modelListSetting = new List<ModelSetting>();
                modelListSetting.Add(modelSetting);

                foreach (var item in cameras)
                {
                    var site = sites.Where(c => c.SiteId == item.SiteId).FirstOrDefault();
                    CameraVisible cameraVisible = (CameraVisible)item.Visible;
                    string siteName = site?.SiteName ?? "-";
                     
                    CameraRtspDetail cameraRtspDetail = new CameraRtspDetail
                    {
                        MaincomId = item.MaincomId,
                        CameraId = item.Id,
                        CameraIp = item.Ip,
                        Name = item.Name,
                        SiteId = item.SiteId,
                        SiteName = siteName,
                        Rtsp = item.Rtsp,
                        RtspIp = item.Ip,
                        RtspUsername = item.Username,
                        RtspPassword = item.Password,
                        Type = (int)item.Type,
                        Online = cameraVisible.GetEnumDesc(),   // item.Visible.ToString(), //item.Visible.HasValue ? item.Visible.ToString() : "0",
                        Remark = item.Remark,
                        CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", item.CreateTime),
                        DeviceId = item.DeviceId,
                        DeviceName = item.DeviceName,
                        RecordStatus = (CameraRecordStatus)item.RecordStatus,
                        DeviceType = deviceType,
                        Onlive = Convert.ToBoolean(item.OnLive),
                        ModelListSetting = modelListSetting
                    };
                    cameraLists.Add(cameraRtspDetail);
                }
                if (cameraLists == null)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)CameraErrorCode.CAM_GET_CAMLIST_FAIL, Success = false, Message = $"{Lang.GeneralUI_NoRecord}" };
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }

                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FUNC:CamController.CameraList][RESULT][{JsonConvert.SerializeObject(cameraLists, Formatting.Indented)}]";
                Logger.LogInformation(loggerLine);

                responseModalX.data = cameraLists;
                return Ok(responseModalX);
            }
            catch (Exception ex)
            {
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)CameraErrorCode.QUERY_CAMERA_LIST_FAIL, Success = false, Message = $"{Lang.QUERY_CAMERA_LIST_FAIL}:{ex.Message}" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return Ok(responseModalX);
            }
        }
        private bool ChkTheSameCameraName(int cameraId, string cameraName, out ResponseModalX responseModalX)
        {
            using BusinessContext businessContext = new BusinessContext();

            FtCamera ftCamera = new FtCamera();
            if (cameraId == 0)
            {
                ftCamera = businessContext.FtCamera.Where(c => c.Name == cameraName).FirstOrDefault();
            }
            else
            {
                ftCamera = businessContext.FtCamera.Where(c => c.Name == cameraName && c.Id != cameraId).FirstOrDefault();
            }

            if (ftCamera != null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)CameraErrorCode.CAM_EXIST_THE_SAME_NAME, Success = false, Message = Lang.CAM_EXIST_THE_SAME_NAME },
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
        /// 使用onvif協議獲取RTSP
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //public IActionResult getRtsp()
        //{
        //    DeviceInfoRtsp deviceInfoRtsp = new DeviceInfoRtsp { ip = "192.168.0.232", name = "root", uri = "192.168.0.232" };
        //    SearchCamera searchCamera = new SearchCamera();
        //    string rtsp = SearchCamera.SearchCameras("RTSP", deviceInfoRtsp);
        //}

        /// <summary>
        /// 變更镜头狀態
        /// CameraRecordStatus :  IN_STOP = 0, IN_RECORD = 1, //錄像進行中 SUSPEND_RECORD = 2 //錄像暫停中 錄像狀態
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeCameraStatus([FromBody] CameraStatus input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext businessContext = new BusinessContext();
            var camera = businessContext.FtCamera.Where(c => c.Id == input.CameraId).FirstOrDefault();
            if (camera != null)
            {
                if (input.MaincomId != camera.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                camera.RecordStatus = (sbyte)input.RecordStatus;
                //camera.Visible = (int)GeneralVisible.VISIBLE;  //僅限於UI界面用戶來控制

                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ChangeCameraStatus] [RecordStatus.:{input.RecordStatus.GetEnumDesc()}]");

                businessContext.FtCamera.Update(camera);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    input.CameraName = camera.Name;
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{input.RecordStatus.GetEnumDesc()}{Lang.GeneralUI_SUCC}" },
                        data = input
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Device_Status}{Lang.GeneralUI_Fail}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"{Lang.CAM_CAMERA_NOT_EXIST} CameraId={input.CameraId}" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::CamController.ChangeCameraStatus][RecordStatus.:{input.RecordStatus}]");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 批量變更镜头狀態
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeListCameraStatus([FromBody] List<CameraStatus> input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            string loggerLine;
            using BusinessContext businessContext = new BusinessContext();

            List<CameraStatus> results = new List<CameraStatus>();

            foreach (CameraStatus item in input)
            {
                var camera = businessContext.FtCamera.Where(c => c.Id == item.CameraId).FirstOrDefault();
                if (camera != null)
                {
                    if (item.MaincomId != camera.MaincomId)
                        continue;

                    camera.RecordStatus = (sbyte)item.RecordStatus;
                    //camera.Visible = (int)GeneralVisible.VISIBLE; //僅限於UI界面用戶來控制

                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ChangeCameraStatus] [BATCH][RecordStatus.:{item.RecordStatus.GetEnumDesc()}]");

                    businessContext.FtCamera.Update(camera);
                    bool res = businessContext.SaveChanges() > 0;
                    if (res)
                    {
                        item.CameraName = camera.Name;
                        results.Add(item);
                    }
                    else
                    {
                        loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ChangeCameraStatus][BATCH] [RecordStatus.:{item.RecordStatus.GetEnumDesc()}]";
                        Logger.LogInformation(loggerLine);
                        CommonBase.OperateDateLoger(loggerLine, CommonBase.LoggerMode.ERROR);
                    }
                }
                else
                {
                    loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ChangeCameraStatus][BATCH][CamerId={item.CameraId}][{Lang.GeneralUI_NoRecord}]";
                    Logger.LogInformation(loggerLine);
                    CommonBase.OperateDateLoger(loggerLine, CommonBase.LoggerMode.ERROR);
                }
            }
            responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)DeviceErrorCode.SUCC_UPD_DEVICE_CAMERA_STATUS_LIST, Message = Lang.SUCC_UPD_DEVICE_CAMERA_STATUS_LIST };
            responseModalX.data = results;
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::CamController.ChangeCameraStatus][Batch RecordStatus List:{Lang.SUCC_UPD_DEVICE_CAMERA_STATUS_LIST}]");
            return Ok(responseModalX);
        }

        /// <summary>
        /// 启用录像镜头|停用录像镜头
        /// 如果启用则改为停用，反之也是这个规则
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeCameraVisible(CameraVisibleInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext businessContext = new BusinessContext();
            var camera = businessContext.FtCamera.Where(c => c.Id == input.CameraId).FirstOrDefault();
            if (camera != null)
            {
                if (input.MaincomId != camera.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }

                if (camera.Visible == (int)CameraVisible.VISIBLE)
                {
                    camera.Visible = (int)CameraVisible.INVISIBLE;
                }
                else
                {
                    camera.Visible = (int)CameraVisible.VISIBLE;
                }

                CameraVisible visible = (CameraVisible)camera.Visible;
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ChangeCameraStatus] [VISIBLE.:{visible.GetEnumDesc()}]");

                businessContext.FtCamera.Update(camera);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    CameraVisibleResponse cameraVisibleResponse = new CameraVisibleResponse
                    {
                        CameraId = camera.Id,
                        CameraVisible = visible,
                        CameraVisibleDesc = visible.GetEnumDesc(),
                        MaincomId = camera.MaincomId

                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{visible.GetEnumDesc()}{Lang.GeneralUI_SUCC}" },
                        data = cameraVisibleResponse
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{visible.GetEnumDesc()}{Lang.GeneralUI_Fail}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.CAM_CAMERA_NOT_EXIST };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::CamController.ChangeCameraVisible]");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 獲取對應镜头的调度策略配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("[controller]/[action]/{cameraId}")]
        public IActionResult ReturnCamerScheduleConfigJson(int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            object obj = CameraBusiness.ReturnCameraConfigObject(cameraId);
            if (obj == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Camera_NotExistSettingNScheduleConfg };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::CamController.ReturnCamerScheduleConfigJson][cameraId.:{cameraId}][ReturnCamerScheduleConfigJson][return obj = null ]");

                return Ok(responseModalX);
            }
            string jsonObj = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::CamController.ReturnCamerScheduleConfigJson][cameraId.:{cameraId}][ReturnCamerScheduleConfigJson]\n{jsonObj}");

            responseModalX.data = obj;
            return Ok(responseModalX);
        }

        /// <summary>
        /// DVR CAMER CONFIG 镜头设置配置
        /// 獲取json : http://localhost:5002/Cam/ReturnCamerScheduleConfigJson/11
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult CameraDVRSettingSchedule(CameraDVRSettingNSchedule input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            try
            {
                string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUN:CameraDVRSettingSchedule][INPUT JSON SERIALIZE][INPUT:{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerline);
            }
            catch (Exception ex)
            {
                string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUN:CameraDVRSettingSchedule][INPUT JSON SERIALIZE][EXCEPTION:{ex.Message}]";
                Common.CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);
            }

            if (string.IsNullOrEmpty(input.MaincomId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId}-{input.DeviceId}" };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();

            var camera = businessContext.FtCamera.Find(input.CameraId);
            if (camera == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)CameraErrorCode.CAMERA_NOT_EXIST, Message = $"{Lang.Config_FAIL} {CameraErrorCode.CAMERA_NOT_EXIST.GetEnumDesc()}" };
                return Ok(responseModalX);
            }
            input.SiteId ??= camera.SiteId.ToString();
            input.CameraName = camera.Name;
            input.CameraIp = camera.Ip;

            var device = businessContext.FtDevice.Find(input.DeviceId);
            if (device == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Message = $"{Lang.Config_FAIL} {DeviceErrorCode.DEVICE_NOT_EXIST.GetEnumDesc()}" };
                return Ok(responseModalX);
            }
            input.DeviceName ??= camera.DeviceName ?? String.Empty;
            if (device.DeviceId != camera.DeviceId)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()}" };
                return Ok(responseModalX);
            }
            //過濾邏輯conflict
            if (input.SavePictRate <= 0)
                input.SavePic = false;
            else
                input.SavePic = true;

            bool res = CameraBusiness.SaveCamerScheduleConfigForCamera(input.CameraId, input);
            if (res == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Camera_ConfigTitle_Title}{Lang.Config_FAIL}" };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.Camera_ConfigTitle_Title}-{Lang.GeneralUI_SUCC}" };
                return Ok(responseModalX);
            }
        }
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeCameraOnlive([FromBody] CameraOnliveInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::CamController.ChangeCameraOnlive] [Onlive.:{JsonConvert.SerializeObject(input)}]");
            using BusinessContext businessContext = new BusinessContext();
            var camera = businessContext.FtCamera.Where(c => c.Id == input.CameraId).FirstOrDefault();
            if (camera != null)
            {
                if (input.MaincomId != camera.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                camera.OnLive = Convert.ToByte(input.IsOnlive);
                camera.Visible = (int)GeneralVisible.VISIBLE;



                businessContext.FtCamera.Update(camera);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    input.CameraName = camera.Name;
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::CamController.ChangeCameraOnlive] [SUCCESS]");
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{ input.CameraName}{Lang.GeneralUI_SUCC}" },
                        data = input
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{input.CameraName}{Lang.GeneralUI_Fail}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"{Lang.CAM_CAMERA_NOT_EXIST} CameraId={input.CameraId}" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::CamController.ChangeCameraOnlive][CAMERA :{Lang.GeneralUI_NoRecord}]");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 攝影鏡頭直播請求
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Language}/[controller]/[action]/{mainComId}/{cameraId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult CamOnliveUrl(string mainComId, int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string onliveUrl = CameraBusiness.GetCamOnliveUrl(mainComId, cameraId, out int deviceId, out DeviceDVRModel deviceDVRModel, ref responseModalX);
            string playUrl = $"http://{deviceDVRModel.DvrIp}:{deviceDVRModel.DvrPort}/play?deviceId={cameraId}&channel=1&stream=1";
            string deviceToken = DeviceBusiness.GetDeviceBearToken(deviceId);
            CameraOnLiveModel cameraOnLive = new CameraOnLiveModel { DeviceId = deviceId, CameraId = cameraId, OonliveUrl = onliveUrl, PlayUrl = playUrl, DeviceToken = deviceToken };
            responseModalX.data = cameraOnLive;
            return SwitchToApiOrView(responseModalX);
        }

        /// <summary>
        /// 返回播放录像页面
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("{Language}/[controller]/[action]/{cameraId}")]
        public IActionResult CameraHistory(int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            Camera camera = CameraBusiness.CameraDetails(cameraId, ref responseModalX);

            ViewBag.Title = $"{camera.DeviceName}.{camera.Name} [{Lang.CameraHistory_Title}]";

#if DEBUG
            ViewBag.Title = "TEST TEST RECORD";
#endif

            return View(responseModalX);
        }

        /// <summary>
        /// 镜头录像历史记录 JSON
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{cameraId}/{timeScaleValue}/{isRecent}")]
        public IActionResult GetCamHistList(int cameraId, long timeScaleValue, bool isRecent)
        {
            if (timeScaleValue == 0)
            {
                timeScaleValue = new DateTimeOffset().ToUnixTimeMilliseconds();
            }
            DateTime datetimeScale = DateTimeHelp.ConvertToDateTime(timeScaleValue);
            ResponseModalX responseModalX = new ResponseModalX();
            bool getDataResult = CameraBusiness.GetCamHistList(cameraId, timeScaleValue, isRecent, ref responseModalX);
            string loggerline = $"[FUNC::GetCamHistList][GetCamHistList][getDataResult={getDataResult} {timeScaleValue}={datetimeScale:yyyy-MM-dd HH:mm:ss fff}][{responseModalX.meta.Message}]";
            Logger.LogInformation(loggerline);
            //ServerHub.s
            return Ok(responseModalX);
        }

        /// <summary>
        /// 简单版的数据单元 CamHistX 僅測試用
        /// </summary>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{cameraId}")]
        public IActionResult GetCamHistListX(int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            bool getDataResult = CameraBusiness.GetCamHistListX(cameraId, ref responseModalX);
            string loggerline = $"[FUNC::GetCamHistListX][getDataResult={getDataResult}][cameraId={cameraId}]";
            Logger.LogInformation(loggerline);
            return Ok(responseModalX);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{cameraId}/{timepoint}")]
        public IActionResult GetCamHistTimePointMediaFile(int cameraId, long timepoint)
        {
            string loggerline;
            try
            {
                loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::GetCamHistTimePointMediaFile][cameraId={cameraId}][timepoint={timepoint}={DateTimeHelp.ConvertToDateTime(timepoint):yyyy-MM-dd HH:mm:ss fff}]";
                Logger.LogInformation(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::GetCamHistTimePointMediaFile][PARSE INPUT JSON][EXCEPTION::{ex.Message}]";
                Logger.LogError(loggerline);
            }
            ResponseModalX responseModalX = new ResponseModalX();
            bool res = CameraBusiness.GetCamHistTimePointMediaFile(cameraId, timepoint, ref responseModalX);
            loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::GetCamHistTimePointMediaFile][Result={res}][cameraId={cameraId},timepoint={timepoint}]";
            Logger.LogInformation(loggerline);
            return Ok(responseModalX);
        }
    }
}