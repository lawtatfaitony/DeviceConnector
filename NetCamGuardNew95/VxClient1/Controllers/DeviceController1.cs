using DataBaseBusiness.Models;
using EnumCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGuard.ApiModels;
using VxGuardClient;
using VxGuardClient.Controllers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using VxGuardClient.Context;
using static EnumCode.EnumBusiness;
using VideoGuard.Business;
using LanguageResource;
using System.Net;
using Microsoft.EntityFrameworkCore;
using VxClient.Models;
using X.PagedList;
using Common;
using LogUtility;
using static VideoGuard.Business.HistRecBusiness;
using VideoGuard.Device;
using Newtonsoft.Json.Serialization;

namespace VxClient.Controllers
{
    public class DeviceController : BaseController
    {
        private string HttpHost;
        private IOptions<UploadSetting> _uploadSetting;

        private string wwwRoot { get; set; }
        public DeviceController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, ILogger<BaseController> logger, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement, IOptions<UploadSetting> uploadSetting)
             : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            Logger = logger;
            wwwRoot = webHostEnvironment.WebRootPath;
        }
        //包括CIC都是使用这个，所以不能加入Authorize CIC版本没有做这个功能
        [HttpGet]
        [Route("{Language}/[controller]/[action]/{SerialNo}")]
        public IActionResult GetMainComBySerialNo(string SerialNo)
        {
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::DeviceController.GetMainComBySerialNo][DEVICE SERIAL No.:{SerialNo}]\n");

            ResponseModalX responseModalX = new ResponseModalX();
            DeviceModel deviceModel = new DeviceModel();

            if (!string.IsNullOrEmpty(SerialNo))
            {
                try
                {
                    SerialNo = WebUtility.UrlDecode(SerialNo);

                    BusinessContext dataBaseContext = new BusinessContext();

                    var device = dataBaseContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(SerialNo)).FirstOrDefault();

                    //config json
                    object obj = DeviceBusiness.ReturnDeviceConfigObject(device.DeviceId);
                    if (obj != null)
                    {
                        deviceModel.Config = JsonConvert.SerializeObject(obj);
                    }

                    if (device != null)
                    {
                        MainCom mainCom = new MainCom(); //默認值 
                        deviceModel.DeviceName = device.DeviceName ?? string.Empty;
                        deviceModel.SiteId = mainCom.SiteId;//設備系統沒有地盤ID(位置ID) 
                        deviceModel.DeviceId = device.DeviceId.ToString();
                        deviceModel.SysModuleId = mainCom.SysModuleId; //設備系統沒有模塊ID
                        deviceModel.DeviceEntryMode = (EnumBusiness.DeviceEntryMode)device.DeviceEntryMode;
                        deviceModel.DeviceSerialNo = device.DeviceSerialNo;
                        deviceModel.Status = device.Status;
                        deviceModel.MainComId = device.MaincomId;
                        deviceModel.OperatedUser = mainCom.SystemDefaultUser;
                        deviceModel.IsReverseHex = device.IsReverseHex > 0;
                        responseModalX.data = deviceModel;

                        Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.GetMainComBySerialNo][SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");

                        return Ok(responseModalX);
                    }
                    else
                    {
                        responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.DEVICE_SERIALNO_NOT_EXIST, Success = false, Message = Lang.DEVICE_SERIALNO_NOT_EXIST };
                        Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetMainComBySerialNo][{Lang.DEVICE_SERIALNO_NOT_EXIST}][SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                        return Ok(responseModalX);
                    }
                }
                catch (Exception ex)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = "No Device SerialNo Input" };
                    responseModalX.data = new { noDeviceSerial = "no device serial" };
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetMainComBySerialNo][{ex.Message}]");
                    return Ok(responseModalX);
                }

            }
            else
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = "No Device SerialNo Input" };
                responseModalX.data = new { noDeviceSerial = "no device serial" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetMainComBySerialNo][SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{SerialNo}")]
        public IActionResult GetMainComByDVRSerialNo(string SerialNo)
        {
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][DVR][FUNC::DeviceController.GetMainComByDVRSerialNo][DEVICE SERIAL No.:{SerialNo}]\n");

            ResponseModalX responseModalX = new ResponseModalX();
            DeviceModel deviceModel = new DeviceModel();

            if (!string.IsNullOrEmpty(SerialNo))
            {
                SerialNo = WebUtility.UrlDecode(SerialNo);
                BusinessContext dataBaseContext = new BusinessContext();
                var device = dataBaseContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(SerialNo)).FirstOrDefault();

                if (device != null)
                {
                    //config json
                    object obj = DeviceBusiness.ReturnDeviceConfigObject(device.DeviceId);
                    if (obj != null)
                    {
                        deviceModel.Config = JsonConvert.SerializeObject(obj);
                        Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][DVR][FUNC::DeviceController.GetMainComByDVRSerialNo][DEVICE CONFIG][{deviceModel.Config}]\n");
                    }

                    MainCom mainCom = new MainCom(); //默認值 
                    deviceModel.DeviceName = device.DeviceName ?? string.Empty;
                    deviceModel.SiteId = mainCom.SiteId;//設備系統沒有地盤ID(位置ID) 
                    deviceModel.DeviceId = device.DeviceId.ToString();
                    deviceModel.SysModuleId = mainCom.SysModuleId; //設備系統沒有模塊ID
                    deviceModel.DeviceEntryMode = (EnumBusiness.DeviceEntryMode)device.DeviceEntryMode;
                    deviceModel.DeviceSerialNo = device.DeviceSerialNo;
                    deviceModel.Status = device.Status;
                    deviceModel.MainComId = device.MaincomId;
                    deviceModel.OperatedUser = mainCom.SystemDefaultUser;
                    deviceModel.IsReverseHex = device.IsReverseHex > 0;
                    responseModalX.data = deviceModel;
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.GetMainComByDVRSerialNo]" +
                        $"[SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = "NOT EXIST By Device SerialNo" };
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetMainComByDVRSerialNo]" +
                        $"[SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = "No Device SerialNo" };
                responseModalX.data = null;
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetMainComByDVRSerialNo]" +
                    $"[SerialNo.:{SerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Add()
        {
            MainCom mainCom = new MainCom();
            DeviceModelInput input = new DeviceModelInput
            {
                MainComId = WebCookie.MainComId,
                DeviceType = DeviceType.DESTOP_DVR,
                Status = (int)GeneralStatus.ACTIVE,
                DeviceEntryMode = DeviceEntryMode.UNDEFINED,
                IsReverseHex = true,
                OperatedUser = User.Identity.Name,
                SiteId = 0,
                SiteName = String.Empty
            };
            return View(input);
        }

        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult Add(DeviceModelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.MainComId))
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.DEVICE_SITE_REQUIED },
                    data = null
                };
                return Ok(responseModalX);
            }

            bool isTheSameDeviceName = DeviceBusiness.IsTheSameDeviceName(input.DeviceId, input.DeviceName, ref responseModalX);
            if (isTheSameDeviceName)
                return Ok(responseModalX);
            //IsTheSameDeviceSerialNo
            bool isTheSameDeviceSerialNo = DeviceBusiness.IsTheSameDeviceSerialNo(input.DeviceId, input.DeviceSerialNo, ref responseModalX);
            if (isTheSameDeviceSerialNo)
                return Ok(responseModalX);

            if (input.SiteId == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.SITE_REQUIED, Message = Lang.DEVICE_SITE_REQUIED, Success = false },
                    data = null
                };
                return Ok(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();

            int maxId = 3001; //设置种子起增基数
            if (businessContext.FtDevice.Count() > 0)
            {
                maxId = businessContext.FtDevice.Max(c => c.DeviceId) + 1;
            }
            DateTime dt = DateTime.Now;

            MainCom mainCom = new MainCom(); //默認值 For local deploy

            FtDevice ftDevice = new FtDevice
            {
                DeviceId = maxId,
                DeviceName = input.DeviceName,
                DeviceType = (int)input.DeviceType,
                MaincomId = input.MainComId,
                SiteId = input.SiteId,
                LibId = 0, //如果不需要人员分类群组库的，则设为0.表示没有。
                DeviceSerialNo = input.DeviceSerialNo.Trim(),
                DeviceConfig = string.Empty, //通用增加设备在此不增加具体的配置。
                IsReverseHex = 1, //默认的反向交叉解析16进制，和hik标准一样
                DeviceEntryMode = (int)DeviceEntryMode.UNDEFINED, //不具体业务不定义具体出入口设备
                Status = (int)GeneralStatus.DEACTIVE, //注意：初始状态是停用中。待一切配置ok后，手动改为状态：启动
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };

            //检查序列号是否其他设备占用
            var occupiedDevice = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(input.DeviceSerialNo)).FirstOrDefault();
            if (occupiedDevice != null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = $"{Lang.DEVICE_SERIAL_NO_OCCUPIED}:{occupiedDevice.DeviceName},ID={occupiedDevice.DeviceId}" },
                    data = null
                };
                return Ok(responseModalX);
            }

            businessContext.FtDevice.Add(ftDevice);
            bool result = businessContext.SaveChanges() > 0;

            if (result)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.ADDNEW_SUCCESS, Message = Lang.DEVICE_ADDNEW_SUCCESS, Success = true },
                    data = ftDevice
                };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.ADDNEW_FAIL, Success = false, Message = Lang.DEVICE_ADDNEW_FAIL },
                    data = null
                };
                return Ok(responseModalX);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("{Language}/[controller]/[action]/{deviceId}")]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult Update(int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (deviceId == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_NoRecord },
                    data = null
                };
                return SwitchToApiOrView(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            FtDevice ftDevice = businessContext.FtDevice.Find(deviceId);
            if (ftDevice == null)
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
                var siteName = String.Empty;
                if (ftDevice.SiteId != 0)
                {
                    siteName = businessContext.FtSite.Find(ftDevice.SiteId)?.SiteName ?? String.Empty;
                    ViewBag.SiteName = siteName;
                }
                //IsTheSameDeviceSerialNo
                bool isTheSameDeviceSerialNo = DeviceBusiness.IsTheSameDeviceSerialNo(ftDevice.DeviceId, ftDevice.DeviceSerialNo, ref responseModalX);
                if (isTheSameDeviceSerialNo)
                    return Ok(responseModalX);

                DeviceModelInput deviceModelInput = new DeviceModelInput
                {
                    MainComId = ftDevice.MaincomId,
                    DeviceId = ftDevice.DeviceId,
                    SiteId = ftDevice.SiteId,
                    SiteName = siteName,
                    DeviceName = ftDevice.DeviceName,
                    DeviceType = (DeviceType)ftDevice.DeviceType,
                    DeviceEntryMode = (DeviceEntryMode)ftDevice.DeviceEntryMode,
                    IsReverseHex = ftDevice.IsReverseHex == 1,
                    DeviceSerialNo = ftDevice.DeviceSerialNo,
                    OperatedUser = User.Identity.Name,
                    Status = ftDevice.Status
                };
                responseModalX.data = deviceModelInput;
                return SwitchToApiOrView(responseModalX);
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult Update([FromForm] DeviceModelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MetaModalX metaModalX = new MetaModalX();

            bool isTheSameDeviceName = DeviceBusiness.IsTheSameDeviceName(input.DeviceId, input.DeviceName, ref responseModalX);
            if (isTheSameDeviceName)
                return Ok(responseModalX);

            if (input.SiteId == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = 0, Message = Lang.CAM_SITE_REQUIED },
                    data = null
                };
                return Ok(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();

            FtDevice ftDevice = businessContext.FtDevice.Find(input.DeviceId);

            // CAMERA_NOT_EXIST
            if (ftDevice == null)
            {
                metaModalX = new MetaModalX
                {
                    ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST,
                    Success = false,
                    Message = Lang.DEVICE_NOT_EXIST
                };
                responseModalX.data = null;
                responseModalX.meta = metaModalX;
                return Ok(responseModalX);
            }
            else
            {
                if (input.MainComId != ftDevice.MaincomId)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId },
                        data = null
                    };
                    return Ok(responseModalX);
                }
                //检查序列号是否其他设备占用
                var occupiedDevice = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(input.DeviceSerialNo) && c.DeviceId != input.DeviceId).FirstOrDefault();
                if (occupiedDevice != null)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = $"{Lang.DEVICE_SERIAL_NO_OCCUPIED}:{occupiedDevice.DeviceName},ID={occupiedDevice.DeviceId}" },
                        data = null
                    };
                    return Ok(responseModalX);
                }

                ftDevice.DeviceName = input.DeviceName?.Trim();
                ftDevice.DeviceType = (int)input.DeviceType;
                ftDevice.DeviceEntryMode = (int)input.DeviceEntryMode;
                ftDevice.DeviceSerialNo = input.DeviceSerialNo?.Trim();
                ftDevice.SiteId = input.SiteId;

                //save-------------------------------------------------------
                businessContext.FtDevice.Update(ftDevice);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = ftDevice
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

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult DeviceList([FromQuery] DeviceSearchInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            MainCom mainCom = new MainCom();
            input.MainComId ??= WebCookie.MainComId ?? mainCom.MainComId;

            if (!string.IsNullOrEmpty(input.Search))
            {
                input.Search = WebUtility.UrlDecode(input.Search);
                input.Search = input.Search.Trim();
            }

            ViewBag.DeviceSearchInput = input;

            try
            {
                using BusinessContext businessContext = new BusinessContext();

                var devices = businessContext.FtDevice.Where(c => c.MaincomId.Contains(input.MainComId)).AsNoTracking();

                if (!string.IsNullOrEmpty(input.Search))
                {
                    devices = devices.AsNoTracking().Where(c => c.DeviceName.Contains(input.Search)
                                                            || c.LibName.Contains(input.Search)
                                                            || c.TaskName.Contains(input.Search)
                                                            || c.DeviceSerialNo.Contains(input.Search));
                }

                if (!String.IsNullOrEmpty(input.DeviceType))
                {
                    DeviceType deviceType = DeviceType.Parse<DeviceType>(input.DeviceType);
                    devices = devices.AsNoTracking().Where(c => c.DeviceType == (int)deviceType);
                }

                var sites = businessContext.FtSite.Where(c => c.MaincomId.Contains(input.MainComId)).ToList();
                var libraries = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(input.MainComId)).ToList();

                List<Device> deviceList = new List<Device>();
                foreach (var item in devices)
                {
                    string siteName = "-";
                    if (item.SiteId != 0)
                        siteName = sites.Where(c => c.SiteId == item.SiteId).FirstOrDefault()?.SiteName ?? "-";

                    DeviceType deviceType = (DeviceType)item.DeviceType;
                    string deviceTypeDesc = deviceType.GetEnumDesc();
                    DeviceEntryMode deviceEntryMode = (DeviceEntryMode)item.DeviceEntryMode;
                    GeneralStatus generalStatus = (GeneralStatus)item.Status;

                    string LibraryName = string.Empty;
                    if (item.LibId.HasValue)
                        LibraryName = libraries.Where(c => c.LibId == item.LibId.GetValueOrDefault()).FirstOrDefault()?.Name ?? "-";

                    Device device = new Device
                    {
                        MaincomId = item.MaincomId,
                        DeviceId = item.DeviceId,
                        DeviceName = item.DeviceName,
                        DeviceType = (DeviceType)item.DeviceType,
                        DeviceTypeDesc = deviceTypeDesc,
                        SiteId = item.SiteId,
                        SiteName = siteName,
                        LibId = item.LibId.GetValueOrDefault(),
                        LibraryName = LibraryName,
                        DeviceSerialNo = item.DeviceSerialNo,
                        DeviceConfig = string.Empty,
                        IsReverseHex = item.IsReverseHex == 1,
                        DeviceEntryMode = deviceEntryMode,
                        DeviceEntryModeDesc = deviceEntryMode.GetEnumDesc(),
                        Status = generalStatus,
                        StatusDesc = generalStatus.GetEnumDesc(),
                        CreateTime = item.CreateTime,
                        UpdateTime = item.UpdateTime
                    };
                    deviceList.Add(device);
                }

                if (deviceList == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = GeneralReturnCode.LIST_NO_RECORD.GetEnumDesc() };
                    responseModalX.data = null;
                    return View("ResponseModal", responseModalX);
                }
                else
                {
                    var newItems = deviceList.ToPagedList(input.Page, input.PageSize);

                    input.TotalPage = newItems.PageCount;
                    input.TotalCount = newItems.TotalItemCount;
                    ViewBag.DeviceSearchInput = input;

                    return View(newItems);
                }
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
        /// 根據設備類型,獲取設備相對應的配置  
        /// NOTE: 使用函數 ReturnDeviceConfigJson 更直接
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{mainComId}/{deviceId}")]
        public IActionResult GetDeviceTypeConfig([FromQuery] string mainComId, string deviceSerialNo)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext dataBaseContext = new BusinessContext();
            var device = dataBaseContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(deviceSerialNo)).FirstOrDefault();
            if (device != null)
            {
                if (mainComId != device.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                DeviceType deviceType = (DeviceType)device.DeviceType;

                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.GetDeviceTypeConfig]" +
                   $"[SerialNo.:{deviceSerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}{deviceType}");
                int deviceId = device.DeviceId;
                switch (deviceType)
                {
                    case DeviceType.HIK_DS_KIT804MF:
                        DeviceHikConfA deviceHikConfA1 = new DeviceHikConfA(deviceId);
                        responseModalX.data = deviceHikConfA1;
                        return Ok(responseModalX);
                    case DeviceType.HIK_DS_KIT341BMW:
                        DeviceHikConfA deviceHikConfA2 = new DeviceHikConfA(deviceId);
                        responseModalX.data = deviceHikConfA2;
                        return Ok(responseModalX);
                    case DeviceType.DESTOP_DVR:
                        DeviceDVRModel deviceDVRModel = new DeviceDVRModel();
                        responseModalX.data = deviceDVRModel.ToInstant(deviceId);
                        return Ok(responseModalX);
                    //其他case 因應增加的設備繼續寫相關的業務......
                    default:
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.DeviceModel_NoRealteConfigTips };
                        return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Device_NotExistByDeviceServialNo };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetDeviceTypeConfig]" +
                    $"[SerialNo.:{deviceSerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeviceHIK_DS_KIT(DeviceHikConfBase input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (!int.TryParse(input.DevIpPort, out int port))
            {
                input.DevIpPort = "8000"; //如果端口格式不对，默认为8000
            }

            if (string.IsNullOrEmpty(input.MainComId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId}" };
                return Ok(responseModalX);
            }

            DeviceHikConfA deviceHikConfA = new DeviceHikConfA(0); //0 = 不获取config配置，用于更新和insert

            deviceHikConfA.DeviceId = input.DeviceId;
            deviceHikConfA.DevIp = input.DevIp;
            deviceHikConfA.DevIpPort = input.DevIpPort;
            deviceHikConfA.LoginId = input.LoginId;
            deviceHikConfA.LoginPassword = input.LoginPassword;
            deviceHikConfA.TypeName = input.TypeName;
            deviceHikConfA.TypeNo = input.TypeNo;
            deviceHikConfA.EmployeeNoPrefix = input.EmployeeNoPrefix;
            deviceHikConfA.DeviceSerialNo = input.DeviceSerialNo;
            deviceHikConfA.MaxFace = input.MaxFace;
            deviceHikConfA.MaxFingerPrint = input.MaxFingerPrint;
            deviceHikConfA.MaxAccessCard = input.MaxAccessCard;
            deviceHikConfA.MaxPassKey = input.MaxPassKey;
            deviceHikConfA.MainComId = input.MainComId;

            bool res = DeviceBusiness.SaveconfigForDevice(input.DeviceId, deviceHikConfA);
            if (res == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = Lang.Config_FAIL };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_Config}-{Lang.GeneralUI_SUCC}" };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 設置CIC配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult SetDeviceCicModel(DeviceCicModel input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.CicAccount))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.DeviceCicModel_CicAccount}{Lang.GeneralUI_Required}" };
                return Ok(responseModalX);
            }

            if (string.IsNullOrEmpty(input.MainComId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId}" };
                return Ok(responseModalX);
            }

            DeviceCicModel deviceCicModel = new DeviceCicModel();

            deviceCicModel.DeviceId = input.DeviceId;
            deviceCicModel.DeviceName = input.DeviceName;
            deviceCicModel.CicAccount = input.CicAccount;
            deviceCicModel.CicPassword = input.CicPassword;
            deviceCicModel.DeviceEntryMode = input.DeviceEntryMode;
            deviceCicModel.MainComId = input.MainComId;
            deviceCicModel.SiteId = input.SiteId;
            deviceCicModel.IsReverseHex = input.IsReverseHex;

            bool res = DeviceBusiness.SaveconfigForDevice(input.DeviceId, deviceCicModel);
            if (res == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = Lang.Config_FAIL };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_Config}-{Lang.GeneralUI_SUCC}" };
                return Ok(responseModalX);
            }
        }


        /// <summary>
        /// QQ 地圖配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult SetQQmapApiModel(QQmapApiModel input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.MapApiKey))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"QQ Map Key {Lang.GeneralUI_Required}" };
                return Ok(responseModalX);
            }

            if (string.IsNullOrEmpty(input.MaincomId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId}" };
                return Ok(responseModalX);
            }

            QQmapApiModel qQmapApiModel = new QQmapApiModel();

            qQmapApiModel.DeviceId = input.DeviceId;
            qQmapApiModel.MapApiIp = input.MapApiIp;
            qQmapApiModel.MapApiKey = input.MapApiKey;
            qQmapApiModel.ApiOutput = input.ApiOutput;
            qQmapApiModel.ApiCallback = input.ApiCallback;
            qQmapApiModel.MaincomId = input.MaincomId;
            qQmapApiModel.DeviceName = input.DeviceName;


            bool res = DeviceBusiness.SaveconfigForDevice(input.DeviceId, qQmapApiModel);
            if (res == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = Lang.Config_FAIL };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_Config}-{Lang.GeneralUI_SUCC}" };
                return Ok(responseModalX);
            }
        }
        /// <summary>
        /// DVR SERVER 配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeviceDVRServerConfig(DeviceDVRModel input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (!int.TryParse(input.DvrPort, out int port))
            {
                input.DvrPort = "8080"; //如果端口格式不对，默认为8000
            }

            if (string.IsNullOrEmpty(input.MaincomId) || input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = $"{Lang.Config_FAIL} {GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc()} OR {Lang.Device_DeviceId} {port}" };
                return Ok(responseModalX);
            }

            DeviceDVRModel deviceDVRModel = new DeviceDVRModel(); //0 = 不获取config配置，用于更新和insert

            deviceDVRModel.DeviceId = input.DeviceId;
            deviceDVRModel.DeviceName = input.DeviceName;
            deviceDVRModel.NvrType = input.NvrType;
            deviceDVRModel.DvrIp = input.DvrIp;
            deviceDVRModel.DvrPort = input.DvrPort;
            deviceDVRModel.Account = input.Account;
            deviceDVRModel.Password = input.Password;
            deviceDVRModel.SiteId = input.SiteId ?? string.Empty;
            deviceDVRModel.MaincomId = input.MaincomId;

            bool res = DeviceBusiness.SaveconfigForDevice(input.DeviceId, deviceDVRModel);
            if (res == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.CONFIG_FAIL, Message = Lang.Config_FAIL };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_Config}-{Lang.GeneralUI_SUCC}" };
                return Ok(responseModalX);
            }
        }
        /// <summary>
        /// 變更設備狀態
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeDeviceStatus(DeviceStatus input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Device_Status}{Lang.DEVICE_DEVICE_ID_UNFORMAT}" };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Where(c => c.DeviceId == input.DeviceId).FirstOrDefault();
            if (device != null)
            {
                if (input.MaincomId != device.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                device.Status = (int)input.Status;

                DeviceType deviceType = (DeviceType)device.DeviceType;

                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.ChangeDeviceStatus]" +
                   $"[Status.:{device.Status}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}{deviceType}");

                businessContext.FtDevice.Update(device);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.Device_Status}{Lang.GeneralUI_SUCC}" },
                        data = device
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
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.DEVICE_NOT_EXIST };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ChangeDeviceStatus]" +
                    $"[Status.:{input.Status}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 删除设备
        /// 前提存在被指派的设备是不能删除
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeviceDelete(DeviceDelModel input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = DeviceErrorCode.ILLEGAL_DEVICE_ID.GetEnumDesc() };
                return Ok(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Where(c => c.DeviceId == input.DeviceId).FirstOrDefault();
            if (device != null)
            {
                if (input.MaincomId != device.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }

                var camera = businessContext.FtCamera.Where(c => c.DeviceId == input.DeviceId).FirstOrDefault();
                if (camera != null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_HAS_STILL_INUSE, Message = $"{DeviceErrorCode.DEVICE_HAS_STILL_INUSE.GetEnumDesc()}<br> {camera.Name} {camera.Ip} ID:{ camera.Id}" };
                    return Ok(responseModalX);
                }

                //for 删除查询用
                string configName = DeviceBusiness.GenerateConfigName(ConfigType.DEVICE, (DeviceType)device.DeviceType, device.DeviceId);

                businessContext.FtDevice.Remove(device);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.DeviceDelete]" +
                                                            $"[{device.DeviceName}:DELETE]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{device.DeviceName}{Lang.DEVICE_DELETE_SUCCSS}" },
                        data = device
                    };
                    //删除配置
                    var configs = businessContext.FtConfig.Where(c => c.Name.Contains(configName));
                    businessContext.FtConfig.RemoveRange(configs);
                    bool resultConfig = businessContext.SaveChanges() > 0;
                    responseModalX.meta.Message += $"<br>{Lang.GeneralUI_Delete}{Lang.GeneralUI_Config} = {resultConfig}";

                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{device.DeviceName}{Lang.DEVICE_DELETE_FAIL}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"{DeviceErrorCode.DEVICE_NOT_EXIST.GetEnumDesc()}" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.DeviceDelete]" +
                    $"[DeviceId.:{input.DeviceId}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 獲取對應設備的配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("[controller]/[action]/{deviceId}")]
        [Route("[controller]/[action]")]
        public IActionResult ReturnDeviceConfigJson(int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            object obj = DeviceBusiness.ReturnDeviceConfigObject(deviceId);
            if (obj == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Device_NotExistByDeviceServialNo };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ReturnDeviceConfigJson]" +
                    $"[deviceId.:{deviceId}]\n[ReturnDeviceConfigObject][return obj = null ]");

                return Ok(responseModalX);
            }
            responseModalX.data = obj;
            return Ok(responseModalX);
        }

        /// <summary>
        /// 變更設備序列號
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeDeviceSerialNo(DeviceSerialNoUpd input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Where(c => c.DeviceId == input.DeviceId).FirstOrDefault();
            if (device != null)
            {
                if (input.MaincomId != device.MaincomId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = GeneralReturnCode.NO_MATCH_MAINCOMID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                device.DeviceSerialNo = input.DeviceSerialNo;

                DeviceType deviceType = (DeviceType)device.DeviceType;

                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceController.ChangeDeviceSerialNo]" +
                   $"[Device SerialNo.:{device.DeviceSerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}{deviceType}");

                businessContext.FtDevice.Update(device);
                bool result = businessContext.SaveChanges() > 0;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.Device_DeviceSerialNo}{Lang.GeneralUI_SUCC}" },
                        data = device
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Device_DeviceSerialNo}{Lang.GeneralUI_Fail}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = $"{Lang.Device_DeviceSerialNo}:{Lang.Device_NotExistByDeviceServialNo}" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ChangeDeviceSerialNo]" +
                    $"[Device SerialNo:{input.DeviceSerialNo}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 通过设备ID，獲取對應設備的配置
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [HttpGet]
        [Route("[controller]/[action]/{deviceId}")]
        [Route("[controller]/[action]")]
        public IActionResult ReturnDeviceCamerScheduleConfigJson(int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext businessContext = new BusinessContext();

            FtDevice device = new FtDevice();
            if (deviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Device_IlleggleDeviceId } OR { Lang.Device_IlleggleDeviceId}" };
                return Ok(responseModalX);
            }
            else
            {
                device = businessContext.FtDevice.Find(deviceId);
                if (device == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_NoRecord };
                    return Ok(responseModalX);
                }
            }
            List<object> listData = new List<object>();
            var camListOfDevice = businessContext.FtCamera.Select(c => new { c.Id, c.DeviceId }).Where(c => c.DeviceId == deviceId);

            foreach (var cam in camListOfDevice)
            {
                object obj = CameraBusiness.ReturnCameraConfigObject(cam.Id);
                if (obj == null)
                {
                    Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ReturnDEviceCamerScheduleConfigJson] [device.:{deviceId}][{Lang.GeneralUI_NoRecord}]");
                }
                else
                {
                    listData.Add(obj);
                }
            }

            if (listData == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Camera_NotExistSettingNScheduleConfg };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ReturnDEviceCamerScheduleConfigJson][{Lang.GeneralUI_NoRecord}]");
            }
            if (listData.Count() == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Camera_NotExistSettingNScheduleConfg };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.ReturnDEviceCamerScheduleConfigJson][{Lang.GeneralUI_NoRecord}]");
            }
            responseModalX.data = listData;
            return Ok(responseModalX);
        }

        /// <summary>
        /// 改变设备入口模式 Change 的  Device.DeviceEntryMode
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeDeviceEntry(DeviceEntryModeInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            BusinessContext businessContext = new BusinessContext();

            if (input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.DEVICE_DEVICE_ID_UNFORMAT };
                return Ok(responseModalX);
            }

            try
            {
                var device = businessContext.FtDevice.Find(input.DeviceId);

                if (device == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_NoRecord };
                    return Ok(responseModalX);
                }

                if (device.MaincomId != input.MainComId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetEnumDesc() };
                    return Ok(responseModalX);
                }

                device.DeviceEntryMode = (int)input.DeviceEntryMode;
                businessContext.FtDevice.Update(device);
                bool res = businessContext.SaveChanges() > 0;
                responseModalX.data = device;

                if (res == false)
                {
                    string logMsg = $"[FUNC::DeviceController1.ChangeDeviceEntry][{Lang.GeneralUI_Fail} - {GeneralReturnCode.EXCEPTION}]";
                    CommonBase.OperateDateLoger(logMsg, CommonBase.LoggerMode.FATAL);
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = logMsg };
                    return Ok(responseModalX);
                }

                return Ok(responseModalX);
            }
            catch (Exception e)
            {
                string logMsg = $"[FUNC::DeviceController1.ChangeDeviceEntry][{Lang.GeneralUI_Fail} - {e.Message}]";
                CommonBase.OperateDateLoger(logMsg, CommonBase.LoggerMode.FATAL);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = logMsg };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 為設備分配人員群組 並且同步插入設備用戶表中,以提供後台作業同步到設備.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ChangeDeviceLibrary(DeviceLibInput input)
        {
            string loggerline;
            try
            {
                string deviceLibInputJson = JsonConvert.SerializeObject(input);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceController.ChangeDeviceLibrary][ENTRY JSON]\n{deviceLibInputJson}";
                Logger.LogWarning(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DeviceController.ChangeDeviceLibrary][JsonConvert.SerializeObject(entry)][EXCEPTION]\n{ex.Message}";
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline);
            }
            ResponseModalX responseModalX = new ResponseModalX();

            if (input.DeviceId == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.DEVICE_DEVICE_ID_UNFORMAT };
                return Ok(responseModalX);
            }

            BusinessContext businessContext = new BusinessContext();

            #region 更新lib_id字段 以及更新設備人員列表
            try
            {
                var device = businessContext.FtDevice.Find(input.DeviceId);

                if (device == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_NoRecord };
                    return Ok(responseModalX);
                }

                if (device.MaincomId != input.MainComId)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetEnumDesc() };
                    return Ok(responseModalX);
                }
                bool chkLibId = PersonBusiness.ChkValidOfLibIdExist(input.LibId, out FtLibrary library, out responseModalX);
                if (library == null)
                {
                    return Ok(responseModalX);
                }
                device.LibId = (int)input.LibId;
                device.LibName = library.Name;
                try
                {
                    businessContext.FtDevice.Update(device);
                    bool res = businessContext.SaveChanges() > 0;
                    responseModalX.data = device;

                    if (res == false)
                    {
                        string logMsg = $"[FUNC::DeviceController1.ChangeDeviceLibrary][DATABASE SERVER FAIL - {GeneralReturnCode.EXCEPTION}]";
                        CommonBase.OperateDateLoger(logMsg, CommonBase.LoggerMode.FATAL);
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = logMsg };
                        return Ok(responseModalX);
                    }
                    //更新設備用戶到設備表 
                    var ftPersons = businessContext.FtPerson.Where(c => c.Visible == (sbyte)PersonErrorCode.PERSON_IS_VISIBLE
                        && c.MaincomId.Contains(input.MainComId)
                        && c.Category == (int)PersonCategory.UNBLOCKED);

                    ftPersons = ftPersons.Where(c => c.LibId == input.LibId || c.LibIdGroups.Contains(input.LibId.ToString())); // 非與條件

                    //ft_device_person
                    List<DevicePerson> devicePersons = new List<DevicePerson>();

                    DefaultContractResolver contractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ContractResolver = contractResolver,
                        Formatting = Formatting.None
                    };

                    DateTime dt = DateTime.Now;
                    foreach (var item in ftPersons)
                    {
                        //PassKey和FingerPrint 暫時沒有提供這兩項功能,所以恆定為false
                        SynchronizedStatus synchronizedStatus = new SynchronizedStatus();
                        bool chkPicId = PersonBusiness.ChkValidOfPersonPicture(item.Id, out FtPicture ftPicture, out responseModalX);
                        synchronizedStatus.FingerPrintNeedToSync = chkPicId;
                        synchronizedStatus.AccessCardNeedToSync = !string.IsNullOrEmpty(item.CardNo);

                        string synchronizedStatusRemark = JsonConvert.SerializeObject(synchronizedStatus, jsonSetting); //介質同步狀態存與這裡

                        DevicePerson devicePerson = new DevicePerson
                        {
                            DeviceId = device.DeviceId,
                            DeviceName = device.DeviceName,
                            PersonId = item.Id,
                            PersonName = item.Name,
                            LibId = library.Id,
                            LibName = library.Name,
                            MaincomId = item.MaincomId,
                            DownInsertStatus = (int)DevicePersonDownInsertStatus.DEVICE_PERSON_DOWN_INSERT_NO_OPERATE,
                            DownInsertStatusDt = dt,
                            DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_NO_OPERATE,
                            DownUpdateStatusDt = dt,
                            DownDelStatus = (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_NO_OPERATE,
                            DownDelStatusDt = dt,
                            SynchronizedStatusRemark = synchronizedStatusRemark,
                            SynchronizedStatus = synchronizedStatus
                        };

                        devicePersons.Add(devicePerson);
                    }
                    //插入或更新設備人員名單數據
                    DevManageBusiness.DevicePersonAddUpdate(devicePersons);

                    //重新检查群组库已经不存在的人员，如果有，设为删除状态 等待删除
                    int delRec = DevManageBusiness.DevicePersonCheckDelete(input.DeviceId);

                    int record = devicePersons?.Count() ?? 0;
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.DevicePerson_AddUpdateSuccess} Person = {record}/Reset DeletedStatus={delRec}" };
                    return Ok(responseModalX);
                }
                catch (Exception e)
                {
                    string logMsg = $"[FUNC::DeviceController1.ChangeDeviceLibrary][DATABASE EXCEPTION] - {e.Message}]";
                    CommonBase.OperateDateLoger(logMsg, CommonBase.LoggerMode.FATAL);
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = logMsg };
                    return Ok(responseModalX);
                }
            }
            catch (Exception e)
            {
                string logMsg = $"[FUNC::DeviceController1.ChangeDeviceLibrary][EXCEPTION] - {e.Message}]";
                CommonBase.OperateDateLoger(logMsg, CommonBase.LoggerMode.FATAL);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = logMsg };
                return Ok(responseModalX);
            }
            #endregion 
        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]/{deviceId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetDeviceBearToken(int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string deviceToken = DeviceBusiness.GetDeviceBearToken(deviceId);
            if (!string.IsNullOrEmpty(deviceToken))
            {
                responseModalX.data = new { deviceToken };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.GENERALUI_DEVICE_TOKEN_ILLEGGLE, Message = Lang.GENERALUI_DEVICE_TOKEN_ILLEGGLE };
                string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetDeviceBearToken][deviceId.:{deviceId}]\n[ReturnDeviceConfigObject][return obj = null ]";
                Logger.LogInformation(loggerline);
                LogHelper.Error(loggerline);
                return Ok(responseModalX);
            }
        }
        /// <summary>
        /// 取得設備下的所有位置關聯鏡頭查詢
        /// </summary>
        /// <param name="maincomId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Language}/[controller]/[action]/{maincomId}/{deviceId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetDeviceCameraNodeOfSites(string maincomId, int deviceId)
        {
            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::GetDeviceCameraNodeOfSites][{maincomId}][{deviceId}]";
            Logger.LogInformation(loggerline);
            List<DeviceSiteTreeModel> deviceSiteTrees = DeviceBusiness.GetDeviceSites(deviceId);
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            DeviceBusiness.GetDeviceCameraNodeOfSites(deviceSiteTrees, maincomId, device);
            string siteCamerasJson = DeviceBusiness.Result.ToString();
            siteCamerasJson = $"[{{\"nodeid\": {device.DeviceId},\"text\": \"{device.DeviceName}\",\"nodes\":{siteCamerasJson}}}]";
            DeviceBusiness.Result = DeviceBusiness.Result.Clear();
            return Ok(siteCamerasJson);
        }


        [HttpGet]
        [Route("{Language}/[controller]/[action]/{maincomId}/{deviceId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult ScreenFour(string maincomId, int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::ScreenFour][{maincomId}][{deviceId}]";
            Logger.LogInformation(loggerline);

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.DEVICE_NOT_EXIST };
                return SwitchToApiOrView(responseModalX);
            }
            else
            {
                DeviceDVRModel deviceDVRModel = new DeviceDVRModel();
                deviceDVRModel.ToInstant(device.DeviceId);
                deviceDVRModel.MaincomId = device.MaincomId;
                deviceDVRModel.Password = DeviceBusiness.GetDeviceBearToken(device.DeviceId);   //輸出的密碼轉為 token
                ViewBag.Title = device.DeviceName;
#if DEBUG
                ViewBag.Title = $"TEST {device.DeviceName} {DateTime.Now:F}";
#endif
                if (deviceDVRModel.DeviceId == 0)
                    deviceDVRModel.DeviceId = deviceId;

                responseModalX.data = deviceDVRModel;
                return SwitchToApiOrView(responseModalX);
            }
        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]/{maincomId}/{deviceId}/{cameraId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult RequestCameraUrlHls(string maincomId, int deviceId, int cameraId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::RequestCameraHls][{maincomId}][{deviceId}{cameraId}]";
            Logger.LogInformation(loggerline);

            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.DEVICE_NOT_EXIST };
                return Ok(responseModalX);
            }

            var camera = businessContext.FtCamera.Find(cameraId);
            if (camera == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.CAM_CAMERA_NOT_EXIST };
                return Ok(responseModalX);
            }
            DeviceDVRModel deviceDVRModel = new DeviceDVRModel();

            deviceDVRModel.ToInstant(device.DeviceId);

            string CameraHlsUrl = DeviceBusiness.RequestCameraHlsUrlFormat(deviceDVRModel, camera.Id);
            responseModalX.data = new { CameraHlsUrl };
            return Ok(responseModalX);

        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]/{maincomId}/{deviceId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetDeviceCameraUrlHlsWithToken(string maincomId, int deviceId)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string deviceToken = DeviceBusiness.GetDeviceBearToken(deviceId);

            using BusinessContext businessContext = new BusinessContext();

            var cameras = businessContext.FtCamera.Where(c => c.DeviceId == deviceId && c.Visible == (int)CameraErrorCode.CAM_IS_VISIBLE).ToList();
            if (cameras == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = Lang.LIST_NO_RECORD };
                return Ok(responseModalX);
            }
            DeviceDVRModel deviceDVRModel = new DeviceDVRModel();
            deviceDVRModel.ToInstant(deviceId);

            if (deviceDVRModel.MaincomId != maincomId)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return Ok(responseModalX);
            }
            if (!string.IsNullOrEmpty(deviceToken))
            {
                List<CameraOnLiveModel> list = new List<CameraOnLiveModel>();
                foreach (var item in cameras)
                {
                    string onliveUrl = DeviceBusiness.RequestCameraHlsUrlFormat(deviceDVRModel, item.Id);
                    string playUrl = DeviceBusiness.RequestCameraPlayUrlFormat(deviceDVRModel, item.Id);
                    CameraOnLiveModel cameraOnLive = new CameraOnLiveModel { DeviceId = deviceId, CameraId = item.Id, OonliveUrl = onliveUrl, PlayUrl = playUrl, DeviceToken = deviceToken };
                    list.Add(cameraOnLive);
                }
                responseModalX.data = list;
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.GENERALUI_DEVICE_TOKEN_ILLEGGLE, Message = Lang.GENERALUI_DEVICE_TOKEN_ILLEGGLE };
                string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceController.GetDeviceCameraUrlHlsWithToken][deviceId.:{deviceId}][message:{Lang.GENERALUI_DEVICE_TOKEN_ILLEGGLE}]";
                Logger.LogInformation(loggerline);
                LogHelper.Error(loggerline);
                return Ok(responseModalX);
            }
        }
        /// <summary>
        /// DESTOP DEPLOY（MULTI） 都是用这个接口
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult AttendancePost([FromBody] AttendancePostEntry entry)
        {
            string loggerline;
            try
            {
                string attendancePostLog = JsonConvert.SerializeObject(entry);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceController.AttendancePost][ENTRY JSON]\n{attendancePostLog}";
                Logger.LogWarning(loggerline);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceController.AttendancePost][JsonConvert.SerializeObject(entry)][EXCEPTION]\n{ex.Message}");
                CommonBase.OperateDateLoger($"[FUNC::DeviceController.AttendancePost][JsonConvert.SerializeObject(entry)][EXCEPTION]\n{ex.Message}");
            }

            if (entry != null)
            {
                if (!string.IsNullOrEmpty(entry.EmployeeNo))
                {
                    entry.EmployeeNo = entry.EmployeeNo == "0" ? string.Empty : entry.EmployeeNo;
                }
            }

            using BusinessContext dbContext = new BusinessContext();
            AttendanceMode attendanceMode = new AttendanceMode();
            ResponseModalX responseModalX = new ResponseModalX();

            //排除三无情况(无工号\卡号\人脸)
            if (string.IsNullOrEmpty(entry.EmployeeNo) && string.IsNullOrEmpty(entry.PhysicalId) && string.IsNullOrEmpty(entry.Face))
            {
                loggerline = $"[3 NONE (FACE,CARD,EMPLOYEENO)][{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceController.AttendancePost][{JsonConvert.SerializeObject(entry)}][RETURN AND NOT HANDLE DATA]";
                Logger.LogInformation(loggerline);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "[3 NONE (FACE,CARD,EMPLOYEENO)]" };
                return Ok(responseModalX);
            }
            //验证合法的设备 驗證傳入的設備id和序列號
            FtDevice device = new FtDevice();
            if (string.IsNullOrEmpty(entry.DeviceId) || string.IsNullOrEmpty(entry.DeviceSerialNo))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.ILLEGAL_DEVICE_SERIAL_NUMBER } OR { Lang.Device_IlleggleDeviceId}" };
                return Ok(responseModalX);
            }
            else
            {
                if (int.TryParse(entry.DeviceId, out int intDeviceId))
                {
                    device = dbContext.FtDevice.Find(intDeviceId);
                    if (device == null)
                    {
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"{Lang.Device_IlleggleDeviceId } OR {Lang.GeneralUI_NoRecord}" };
                        return Ok(responseModalX);
                    }
                    else if (device.DeviceSerialNo.Trim() != entry.DeviceSerialNo.Trim())
                    {
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.ILLEGAL_DEVICE_SERIAL_NUMBER}[{entry.DeviceSerialNo}]" };
                        return Ok(responseModalX);
                    }
                    //通過了序列號比對和設備ID查詢,那說明設備驗證成功,則 賦值MainComId給入口MainComId (entry.mainComId) 功能是對那些沒有傳入MainComId有用.
                    entry.MainComId = device.MaincomId;

                    ////設備模式和考勤模式的轉換 改由直接从入口数据
                    //entry.AttendanceMode = (int)DeviceBusiness.DeviceTypeToAttendanceMode((DeviceType)device.DeviceType);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = Lang.Device_IlleggleDeviceId };
                    return Ok(responseModalX);
                }
            }

            try
            {
                //判斷 無論機器是以卡位中心還是以人為中心 都執行規則: 卡號少於10位則為工號
                if (!string.IsNullOrEmpty(entry.PhysicalId))
                {
                    if (entry.PhysicalId.Length < 8 && string.IsNullOrEmpty(entry.EmployeeNo))
                    {
                        entry.EmployeeNo = entry.PhysicalId;
                        entry.PhysicalId = string.Empty;  //工號和卡號置換過來.(可能由於低級設備導致的)
                    }
                }

                responseModalX.data = entry;

                //不合规则的时间
                if (entry.Occur == 0)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.GENERALUI_DEVICE_NOT_SYNC_UNIXTIME, Message = $"entry.Occur = 0 It is not aloowed!" };
                    return Ok(responseModalX);
                }
                //判斷是否10位的秒時間戳,否則轉為毫秒
                long occur = DateTimeHelp.ConvertToMillSecond(entry.Occur);

                HistoryRecordsDefault historyRecordsDefault = new HistoryRecordsDefault();
                historyRecordsDefault.MaincomId = device.MaincomId;
                historyRecordsDefault.Id = occur;
                historyRecordsDefault.DeviceId = device.DeviceId;
                historyRecordsDefault.DeviceName = device.DeviceName;

                if (Math.Abs(entry.AttendanceMode) > 127)
                {
                    entry.AttendanceMode = (int)AttendanceMode.STANDARD_CARD; //超出数值范围使用默认值
                }
                historyRecordsDefault.Mode = (sbyte)entry.AttendanceMode;



                historyRecordsDefault.OccurDatetime = entry.Occur;
                historyRecordsDefault.UpdateTime = DateTimeHelp.ConvertToDateTime(entry.Occur);
                historyRecordsDefault.CreateTime = DateTime.Now;

                //人員ID優先
                bool retOfPersonByOuterId = false;
                FtPerson ftPerson = new FtPerson();
                if (!string.IsNullOrEmpty(entry.EmployeeNo))
                {
                    if (entry.EmployeeNo.Length > 20)
                    {
                        entry.EmployeeNo = entry.EmployeeNo.Substring(0, 19);
                    }
                    //傳入 entry.EmployeeNo 工號 查詢 規則: 以工號代替person.Id 作為外部內部的工號查詢
                    string picPath = string.Empty;
                    retOfPersonByOuterId = PersonBusiness.CheckExistPersonByOuterId(device.MaincomId, entry.EmployeeNo, ref ftPerson, ref responseModalX, ref picPath);
                    if (retOfPersonByOuterId)
                    {
                        //如果被锁定的人员不能拍卡/人臉識別等<<== 此规则取消
                        //规则：如果被锁定的人员不能拍卡/人臉識別等  不判断是否锁定要上存 锁定仅for 设备用户同步使用 [Lang.PERSON_IS_BLOCKED_TIPS] 2022年8月27日 

                        historyRecordsDefault.PersonId = ftPerson.Id;
                        historyRecordsDefault.PersonName = ftPerson.Name ?? string.Empty;
                        historyRecordsDefault.CardNo = ftPerson.CardNo;
                        historyRecordsDefault.Category = (sbyte)ftPerson.Category; //PersonCategory.UNBLOCKED,
                        historyRecordsDefault.Classify = (sbyte)CLASSIFY.IS_PERSON; //不是陌生人
                        historyRecordsDefault.PicPath = picPath;
                    }
                    else if (!string.IsNullOrEmpty(entry.PhysicalId)) //否則如果有咔號也同樣保存在記錄中
                    {
                        historyRecordsDefault.CardNo = entry.PhysicalId;
                    }
                }
                PersonCardInfo personCardInfo = new PersonCardInfo();
                bool retOfPersonByCard = false;
                //卡號優先
                if (!string.IsNullOrEmpty(entry.PhysicalId))
                {
                    retOfPersonByCard = DeviceBusiness.IsCardNumberRelatePerson(device.MaincomId, entry.PhysicalId, ref personCardInfo);

                    if (retOfPersonByCard)
                    {
                        //如果被锁定的人员不能拍卡/人臉識別等<<== 此规则取消
                        //规则：如果被锁定的人员不能拍卡/人臉識別等  不判断是否锁定要上存 锁定仅for 设备用户同步使用 [Lang.PERSON_IS_BLOCKED_TIPS] 2022年8月27日 

                        historyRecordsDefault.PersonId = personCardInfo.PersonId;
                        historyRecordsDefault.CardNo = personCardInfo.CardNo;
                        historyRecordsDefault.PersonName = personCardInfo.Name;
                        historyRecordsDefault.Classify = (sbyte)CLASSIFY.IS_PERSON; //不是陌生人
                        historyRecordsDefault.Classify = (sbyte)personCardInfo.Category;
                    }
                }

                if (!string.IsNullOrEmpty(entry.Face) && (entry.AttendanceMode != (int)AttendanceMode.FACE || entry.AttendanceMode != (int)AttendanceMode.FACE_CARD_VERIFY))
                {
                    attendanceMode = AttendanceMode.FACE;

                    entry.AttendanceMode = (int)attendanceMode;
                }
                if (!string.IsNullOrEmpty(entry.Face))
                    historyRecordsDefault.CapturePath = entry.Face;

                bool result = HistRecBusiness.AddNewHistRecordForAttendancePost(historyRecordsDefault, device, ref responseModalX);
                if (responseModalX.meta.ErrorCode == (int)GeneralReturnCode.GENERALUI_EXIST_RECORD)
                {
                    loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][EXIST][ERROR CODE::GENERALUI_EXIST_RECORD)][FUNC::DeviceController.AttendancePost][{responseModalX.meta.Message}]";
                    Logger.LogWarning(loggerline);
                }

                if (responseModalX.meta.Success)
                {
                    Logger.LogWarning("----------------------------------------------------------");
                    loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][SUCCESS][FUNC::DeviceController.AttendancePost][{responseModalX.meta.Message}][FACE={entry.Face}][EmployeeNo={entry.EmployeeNo}][OCCUR={entry.Occur}][FingerPrint={entry.FingerPrint}][MAIN COM ID={entry.MainComId}]";
                    Logger.LogWarning(loggerline);
                    Logger.LogWarning("----------------------------------------------------------");
                }
                return Ok(responseModalX);
            }
            catch (Exception e)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = e.Message };
                return Ok(responseModalX);
            }
        }
    }
}
