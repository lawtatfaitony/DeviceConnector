using Common;
using DataBaseBusiness.ModelHistory;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VideoGuard.ApiModels;
using VxClient.Models;
using VxGuardClient;
using VxGuardClient.Context;
using VxGuardClient.Controllers;
using VxGuardClient.Extensions;
using X.PagedList;

namespace VxClient.Controllers
{
    public class HistAlarmController : BaseController
    {
        readonly IHubContext<HistAlarmHub> _HistAlarmHub;
        private string HttpHost;
        private string UpDownPictureApiHost;
        private UploadSetting _UploadSetting { get; set; }
        //private string _SubFolderEntries { get; set; } = "EntriesLogImages";
        private string AIFolderEntries { get; set; } = "AIAlarm";
        private string AIUriPath { get; set; }
        public HistAlarmController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, ILogger<BaseController> logger, IHubContext<HistAlarmHub> histAlarmHub, IOptions<TokenManagement> tokenManagement, IOptions<UploadSetting> uploadSetting)
            : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
            _HistAlarmHub = histAlarmHub;

            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            _UploadSetting = uploadSetting.Value;
            UpDownPictureApiHost = string.Format("http://{0}:{1}", uploadSetting.Value.UploadServerIp, uploadSetting.Value.UploadServerPort);

            AIUriPath = $"/{_UploadSetting.TargetFolder}/{AIFolderEntries}/"; //AIFolderEntries未賦值之前

            AIFolderEntries = Path.Combine(webHostEnvironment.WebRootPath, _UploadSetting.TargetFolder, AIFolderEntries);
            if (!Directory.Exists(AIFolderEntries))
            {
                Directory.CreateDirectory(AIFolderEntries);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ADD NEW 警報 HistAlarm（Table camera_guard_history.hist_alarm）
        /// </summary>
        /// <param name="histAlarm"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public async Task<IActionResult> AddHistAlarm([FromBody] HistAlarm histAlarm)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            try
            {
                HistAlarmBusiness histAlarmBusines = new HistAlarmBusiness();
                bool result = histAlarmBusines.AddHistAlarm(histAlarm).Result;

                if (result == true)
                {
                    await _HistAlarmHub.Clients.All.SendAsync("HistAlarmHub", histAlarm);
                }
            }
            catch (Exception ex)
            {
                string logDoorCatch = $"[FUNC:HistAlarmController::AddHistAlarm][_HistAlarmHub.Clients.All.SendAsync（）] [EXCEPTION] [{ex.Message}]";
                Logger.LogWarning(logDoorCatch);

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Message = "ADD HIST ALARM FAIL [EXCEPTION]", Success = false };

                return Ok(responseModalX);
            }
            responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = "ADD HIST ALARM SUCCESS", Success = true };
            return Ok(responseModalX);
        }

        /// <summary>
        /// ADD NEW 警報 HistAlarm（Table camera_guard_history.hist_alarm）
        /// Hub Name = HistAlarmHub
        /// </summary> 
        ///<param>MaincomId</param>
        ///<param>int? TaskId</param>
        ///<param>int? AlarmLevel</param>
        ///<param>TaskName</param>
        ///<param>int? TaskType</param>
        ///<param>string TaskTypeDesc</param>
        ///<param>int? CameraId</param>
        ///<param>string CameraName</param>
        ///<param>string ObjName</param>
        ///<param>ObjShortDesc</param>
        ///<param>ObjJsonData</param>
        ///<param>OccurDatetime</param> 
        ///<param>Threshold</param>
        ///<param>Remark</param>
        ///<param>Base64Picture</param>
        ///<returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public async Task<IActionResult> AddHistAlarmWithBase64Pict([FromBody] HistAlarmEntriesModel histAlarmModel)
        {
            string fineName = $"{DateTime.Now:yyyyMMddHHmmssfff}.jpg";
            ResponseModalX responseModalX = new ResponseModalX();
            if (histAlarmModel == null)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = "Input params null!!![EXCEPTION]", Success = false };
                return Ok(responseModalX);
            }

            try
            {
                DateTime dt = histAlarmModel.OccurDatetime == 0 ? DateTime.Now : DateTimeHelp.ConvertToDateTime(histAlarmModel.OccurDatetime.GetValueOrDefault());
                HistAlarmBusiness histAlarmBusines = new HistAlarmBusiness();
                HistAlarm histAlarm = new HistAlarm
                {
                    HistAlarmId = 0,
                    MaincomId = histAlarmModel.MaincomId,
                    TaskId = histAlarmModel.TaskId,
                    AlarmLevel = histAlarmModel.AlarmLevel,
                    TaskName = histAlarmModel.TaskName,
                    TaskType = histAlarmModel.TaskType,
                    TaskTypeDesc = histAlarmModel.TaskTypeDesc,
                    CameraId = histAlarmModel.CameraId,
                    CameraName = histAlarmModel.CameraName,
                    ObjName = histAlarmModel.ObjName,
                    ObjShortDesc = histAlarmModel.ObjShortDesc,
                    ObjJsonData = histAlarmModel.ObjJsonData,
                    OccurDatetime = histAlarmModel.OccurDatetime,
                    CaptureTime = dt,
                    CreateTime = DateTime.Now,
                    CapturePath = string.Empty,
                    Threshold = histAlarmModel.Threshold,
                    Remark = histAlarmModel.Remark
                };

                if (histAlarmModel.Base64Picture != null)
                {
                    string AIAlarmCameraPath = Path.Combine(AIFolderEntries, histAlarm.CameraId.ToString(), $"{DateTime.Now:yyyyMM}");
                    if (!Directory.Exists(AIAlarmCameraPath))
                    {
                        Directory.CreateDirectory(AIAlarmCameraPath);
                    }
                    //保存BASE64圖片
                    Image image = Util.Base64ToImage(histAlarmModel.Base64Picture);
                    Image img60x60 = Util.ResizeImage(image, 60, 60);

                    string savePathFile = Path.Combine(AIAlarmCameraPath, fineName);
                    string savePathFileSmall = Path.Combine(AIAlarmCameraPath, fineName);
                    savePathFileSmall = $"{savePathFileSmall}s60X60.jpg";

                    //保存大圖
                    bool saveResult = Util.Base64Save(histAlarmModel.Base64Picture, savePathFile);
                    //保存小圖
                    string base64FromImg60x60 = Util.ImageToBase64(img60x60, ImageFormat.Jpeg);
                    bool saveSmallResult = Util.Base64Save(base64FromImg60x60, savePathFileSmall);

                    //----------------------------------------------------------------------------------------------------
                    histAlarm.CapturePath = $"{AIUriPath}{histAlarm.CameraId.ToString()}/{DateTime.Now:yyyyMM}/{fineName}";
                }
                bool result = histAlarmBusines.AddHistAlarm(histAlarm).Result;
                if (result == true)
                {
                    await _HistAlarmHub.Clients.All.SendAsync("HistAlarmHub_NewRec", histAlarm);
                }

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = "ADD HIST ALARM SUCCESS", Success = true };
                responseModalX.data = histAlarm;
                return Ok(responseModalX);
            }
            catch (Exception ex)
            {
                string logDoorCatch = $"[FUNC:HistAlarmController::AddHistAlarm][_HistAlarmHub.Clients.All.SendAsync（）] [EXCEPTION] [{ex.Message}]";
                Logger.LogWarning(logDoorCatch);

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Message = "ADD HIST ALARM FAIL [EXCEPTION]", Success = false };

                return Ok(responseModalX);
            }
        }

        public bool SaveImage(Image image, string filename)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    ms.Position = 0;


                    string appPath = Path.Combine(webHostEnvironment.ContentRootPath, "img");

                    if (!Directory.Exists(appPath))
                    {
                        Directory.CreateDirectory(appPath);
                    }
                    string date = Util.GetDateFolderName();
                    string datePath = Path.Combine(webHostEnvironment.ContentRootPath, "img", date);
                    if (!Directory.Exists(datePath))
                    {
                        Directory.CreateDirectory(datePath);
                    }

                    var pathFilename = Path.Combine(appPath, filename);
                    if (System.IO.File.Exists(pathFilename))
                    {
                        System.IO.File.Delete(pathFilename);
                    }

                    using (System.IO.FileStream output = new System.IO.FileStream(pathFilename, FileMode.Create))
                    {
                        ms.CopyTo(output);
                        ms.Flush();
                        ms.Close();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 警報列表 （FOR ALARM HISTORY）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult List([FromQuery] HistAlarmListInput input)
        {
            //ViewBag.EntriesLogImagesPath = $"{HttpHost}/{_UploadSetting.TargetFolder}/{_SubFolderEntries}/";

            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.MaincomId) && !string.IsNullOrEmpty(WebCookie.MainComId))
                input.MaincomId = WebCookie.MainComId;

            DateTime dt = DateTime.Now;
            DateTimeRangeObj dateRange = new DateTimeRangeObj();

            if (!string.IsNullOrEmpty(input.OccurDateTimeRange))
            {
                input.OccurDateTimeRange = WebUtility.UrlDecode(input.OccurDateTimeRange);
                dateRange = CommonBase.DateTimeRangeParse(input.OccurDateTimeRange);
                if (dateRange.Start.Date == dateRange.End.Date)
                    dateRange.Start = dateRange.Start.AddDays(-7);
            }
            else
            {
                dateRange.Start = DateTimeHelp.FirstDayOfMonth(dt);
                dateRange.End = DateTimeHelp.LastDayOfMonth(dt);
            }

            if (dateRange.Start.Date == dateRange.End)
            {
                dateRange.Start = dateRange.Start.AddMonths(-3);
            }
            input.OccurDateTimeRange = string.Format("{0:yyyy-MM-ddTHH:mm}-{1:yyyy-MM-ddTHH:mm}", dateRange.Start, dateRange.End);

            if (string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetDescriptionX() }
                };
                return RedirectToAction("ResponseModal", responseModalX);
            }

            ViewBag.HistAlarmListInput = input;

            if (!string.IsNullOrEmpty(input.Search))
            {
                input.Search = WebUtility.UrlDecode(input.Search).Trim();
            }
            using HistoryContext historyContext = new HistoryContext();

            var histAlarmList = historyContext.HistAlarm.AsNoTracking()
                    .Where(c => c.MaincomId.Contains(input.MaincomId) && c.CreateTime > dateRange.Start && c.CreateTime < dateRange.End).OrderByDescending(s => s.CreateTime).Take(5000);

            if (input.CameraId != 0)
                histAlarmList = histAlarmList.Where(c => c.CameraId == input.CameraId);

            if (!string.IsNullOrEmpty(input.Search))
            {
                histAlarmList = histAlarmList.Where(s => s.CameraName.Contains(input.Search)
                                            || s.TaskName.Contains(input.Search));

                if (long.TryParse(input.Search, out long keyId) && input.Search.Length == 13)
                {
                    histAlarmList.Where(s => s.HistAlarmId == (ulong)keyId);
                }
            }

            if (histAlarmList == null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = Lang.GeneralUI_ListNoRecord },
                    data = null
                };
                RedirectToAction("ResponseModal", responseModalX);
            }
            var newListItems = histAlarmList.ToPagedList(input.Page, input.PageSize);
            return SwitchToApiOrView(newListItems);
        }


        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{histAlarmId}")]
        [HttpGet]
        public IActionResult ItemDetails(ulong histAlarmId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using HistoryContext historyContext = new HistoryContext();
            HistAlarm histAlarm = historyContext.HistAlarm.Find(histAlarmId);

            return View(histAlarm);
        }
    }
}

