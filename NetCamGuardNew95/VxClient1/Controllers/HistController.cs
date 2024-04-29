using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using EnumCode;
using FastConnector.HistRecognize;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using Newtonsoft.Json.Linq;
using LogUtility;
using Microsoft.Extensions.Logging;
using VxGuardClient.Context;
using DataBaseBusiness.ModelHistory;
using Microsoft.EntityFrameworkCore;
using LanguageResource;
using X.PagedList;
using VxClient.Models;

namespace VxGuardClient.Controllers
{
    public partial class HistController : BaseController
    {
        private string HttpHost;
        private string UpDownPictureApiHost;
        private string _SubFolderEntries { get; set; } = "EntriesLogImages";
        private UploadSetting _UploadSetting { get; set; }
        public HistController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<UploadSetting> uploadSetting, ILogger<BaseController> logger)
           : base(webHostEnvironment, httpContextAccessor)
        {  
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            _UploadSetting = uploadSetting.Value;
            UpDownPictureApiHost = string.Format("http://{0}:{1}", uploadSetting.Value.UploadServerIp, uploadSetting.Value.UploadServerPort);
            Logger = logger;
        }
        /// <summary>
        /// 最大5000條記錄
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult List([FromQuery] HistListInput input)
        {
            ViewBag.EntriesLogImagesPath = $"{HttpHost}/{_UploadSetting.TargetFolder}/{_SubFolderEntries}/";

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

            ViewBag.HistListInput = input;

            if (!string.IsNullOrEmpty(input.Search))
            {
                input.Search = WebUtility.UrlDecode(input.Search).Trim();
            }
            using HistoryContext historyContext = new HistoryContext();

            var histList = historyContext.HistRecognizeRecord.AsNoTracking()
                    .Where(c => c.MaincomId.Contains(input.MaincomId) && c.CreateTime > dateRange.Start && c.CreateTime < dateRange.End).OrderByDescending(s => s.CreateTime).Take(5000);

            if (input.DeviceId != 0)
                histList = histList.Where(c => c.DeviceId == input.DeviceId);

            if (!string.IsNullOrEmpty(input.Search))
            {
                histList = histList.Where(s => s.CameraName.Contains(input.Search) 
                                            || s.DeviceName.Contains(input.Search)
                                            || s.LibName.Contains(input.Search)
                                            || s.PersonName.Contains(input.Search)
                                            || s.TaskName.Contains(input.Search));
                if(long.TryParse(input.Search,out long keyId)&& input.Search.Length==13)
                {
                    histList.Where(s => s.Id== keyId);
                }
            }
            
            if (histList == null)
            { 
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = Lang.GeneralUI_ListNoRecord },
                    data = null
                };
                RedirectToAction("ResponseModal", responseModalX);
            } 

            var newListItems = histList.ToPagedList(input.Page, input.PageSize); 
            return SwitchToApiOrView(newListItems); 
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult Live()
        {
            ViewData["HttpHost"] = HttpHost;
            ViewData["UpDownPictureApiHost"] = UpDownPictureApiHost;

            ResponseModalX responseModalX = new ResponseModalX();
            HttpRequest request = httpContextAccessor.HttpContext.Request;
            LogHelper.Info(string.Format("[{0:yyyy:MM:dd HH:mm:ss fff}][IP:{1:2}] [DynamicLiveRoom][START]", DateTime.Now, request.Host.Host, request.Host.Port));
            LiveRoomInitializeSRV liveRoomInitialize = new LiveRoomInitializeSRV(); 
            try
            {
                liveRoomInitialize.ToInstance();
                responseModalX.data = liveRoomInitialize;
                return SwitchToApiOrView(responseModalX);
            }
            catch (Exception ex)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = ex.Message }
                };
                return SwitchToApiOrView(responseModalX);
            }
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult HistLiveRoomNextJsonItem(CurrentIndexTimeStampInput currentIndexTimeStampInput) 
        {
            ResponseModalX responseModalX = new ResponseModalX();
            HistoryLiveRoomSRV historyLiveRoom = ReturnNewHistoryLiveRoom.GetNextHistLiveRoomItem(currentIndexTimeStampInput.CurrentIndexTimeStamp);
            ReturnNewHistoryLiveRoom returnNewHistoryLiveRoom = new ReturnNewHistoryLiveRoom();
            returnNewHistoryLiveRoom.HistoryLiveRoom = historyLiveRoom;
            returnNewHistoryLiveRoom.CurrentIndexTimeStamp = historyLiveRoom.CurrentIndexTimeStamp;
            returnNewHistoryLiveRoom.CurrentIndexId = historyLiveRoom.Id;
            if(returnNewHistoryLiveRoom.CurrentIndexTimeStamp == currentIndexTimeStampInput.CurrentIndexTimeStamp)
            {
                returnNewHistoryLiveRoom.CurrentIndexId = 0;
            }

            try
            {
                responseModalX.data = returnNewHistoryLiveRoom;
                return Json(responseModalX);
            }
            catch (Exception ex)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = ex.Message }
                };
                return Json(responseModalX);
            }
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult HistLiveRoomNextJsonItem(long currentIndexTimeStamp) //[FromBody] 
        {
            ResponseModalX responseModalX = new ResponseModalX();
            HistoryLiveRoomSRV historyLiveRoom = ReturnNewHistoryLiveRoom.GetNextHistLiveRoomItem(currentIndexTimeStamp);
            ReturnNewHistoryLiveRoom returnNewHistoryLiveRoom = new ReturnNewHistoryLiveRoom();
            returnNewHistoryLiveRoom.HistoryLiveRoom = historyLiveRoom;
            returnNewHistoryLiveRoom.CurrentIndexTimeStamp = historyLiveRoom.CurrentIndexTimeStamp;
            returnNewHistoryLiveRoom.CurrentIndexId = historyLiveRoom.Id;

            try
            {
                responseModalX.data = returnNewHistoryLiveRoom;
                return Json(responseModalX);
            }
            catch (Exception ex)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = ex.Message }
                };
                return Json(responseModalX);
            }
        }
         
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult HistLiveRoomItem(HistRecognizeRecordIdInput HistRecognizeRecordIdInput)
        {
            ViewData["HttpHost"] = HttpHost;
            ViewData["UpDownPictureApiHost"] = UpDownPictureApiHost;
            ResponseModalX responseModalX = new ResponseModalX();
            HistoryLiveRoomSRV historyLiveRoom = ReturnNewHistoryLiveRoom.GetCurrentHistoryLiveRoomItem(HistRecognizeRecordIdInput.HistRecognizeRecordId);
            if (historyLiveRoom == null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "Nothing" },
                    data = "0"
                };
                return SwitchToApiOrView(responseModalX);
            }
            else
            {
                HistoryLiveRoomSRV returnNewHistoryLiveRoom = historyLiveRoom;
                returnNewHistoryLiveRoom.CurrentIndexTimeStamp = historyLiveRoom.CurrentIndexTimeStamp;
                responseModalX.data = returnNewHistoryLiveRoom;
                return SwitchToApiOrView(responseModalX);
            }
        }
    }
}