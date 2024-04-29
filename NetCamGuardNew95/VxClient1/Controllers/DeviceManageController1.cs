using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AttModel.Device.DevSynchronize;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.LibraryApiModel;
using VideoGuard.Business;
using VideoGuard.Device;
using VxClient.Models;
using VxGuardClient;
using VxGuardClient.Context;
using VxGuardClient.Controllers;
using X.PagedList;
using static EnumCode.EnumBusiness;
using static VideoGuard.Business.HistRecBusiness;
using Genders = VideoGuard.ApiModels.Genders;

namespace VxClient.Controllers
{
    public partial class DeviceManageController : BaseController
    {
        public IWebHostEnvironment webHostEnv { get; set; }
        public IHttpContextAccessor httpContext { get; set; }
        private string HttpHost { get; set; }
        private UploadSetting _UploadSetting { get; set; }
    
        private string UploadFolderPath { get; set; }

        public DeviceManageController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,  IOptions<UploadSetting> uploadSetting,
          ILogger<BaseController> logger) : base(webHostEnvironment, httpContextAccessor)
        { 
            _UploadSetting = uploadSetting.Value;
            UploadFolderPath = Path.Combine(webHostEnvironment.WebRootPath, _UploadSetting.TargetFolder);
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;

            webHostEnv = webHostEnvironment;
            httpContext = httpContextAccessor;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
 
            Logger = logger;
        }

        #region 页面接口 列出设备的用户列表

        [HttpGet]
        [Route("{Language}/[controller]/[action]")]
        [Authorize]
        public IActionResult DevicePersonList(DevicePersonListInput input)
        {
            input.Page = input.Page == 0 ? 1 : input.Page;
            input.PageSize = input.PageSize == 0 ? 100 : input.PageSize;

#if DEBUG
            input.PageSize = 10;
#endif 

            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.MaincomId) && string.IsNullOrEmpty(WebCookie.MainComId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }
            input.MaincomId ??= WebCookie.MainComId;

            ViewBag.DevicePersonListInput = input;


            BusinessContext businessContext = new BusinessContext();

            var devicePersonList = businessContext.FtDevicePerson.Where(c => c.DeviceId == input.DeviceId).ToList();
             
            if (devicePersonList?.Count() == 0)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Message = Lang.GeneralUI_ListNoRecord };
                return SwitchToApiOrView(responseModalX);
            }

            //Relate to FtPerson

            var devicePersonModelRelateLists = from dp in devicePersonList
                                               join fp in businessContext.FtPerson
                                                    on dp.PersonId equals fp.Id
                                               select new
                                               {
                                                   DeviceId = dp.DeviceId,
                                                   DeviceName = dp.DeviceName,
                                                   PersonId = dp.PersonId,
                                                   PersonName = dp.PersonName,
                                                   DeviceLibId = dp.LibId,
                                                   DeviceLibName = dp.LibName,
                                                   DownInsertStatus = dp.DownInsertStatus,
                                                   DownInsertStatusDt = dp.DownInsertStatusDt,
                                                   DownUpdateStatus = dp.DownUpdateStatus,
                                                   DownUpdateStatusDt = dp.DownUpdateStatusDt,
                                                   DownDelStatus = dp.DownDelStatus,
                                                   DownDelStatusDt = dp.DownDelStatusDt,
                                                   SynchronizedStatusRemark = dp.SynchronizedStatusRemark,
                                                   //--------------------------------------------------------------------
                                                   MaincomId = fp.MaincomId,
                                                   OuterId = fp.OuterId,
                                                   LibId = fp.LibId,
                                                   LibName = fp.Name,
                                                   LibIdGroups = fp.LibIdGroups,
                                                   CardNo = fp.CardNo,
                                                   Category = fp.Category
                                               };

            
            try
            {
                List<LibraryItemX> libraryItemXs = new List<LibraryItemX>();

                List<DevicePersonModel> devicePersonModelList = new List<DevicePersonModel>();
                foreach (var item in devicePersonModelRelateLists)
                {
                    bool chkLibId = PersonBusiness.ChkValidOfLibIdExist(item.LibId, out FtLibrary ftLibrary, out responseModalX);
                    bool chkPicId = PersonBusiness.ChkValidOfPersonPicture(item.PersonId, out FtPicture ftPicture, out responseModalX);
                    SynchronizedStatus synchronizedStatus = JsonConvert.DeserializeObject<SynchronizedStatus>(item.SynchronizedStatusRemark);
                    DevicePersonModel devicePersonModel = new DevicePersonModel
                    {
                        DeviceId = item.DeviceId,
                        DeviceName = item.DeviceName,
                        PersonId = item.PersonId,
                        PersonName = item.PersonName,
                        DeviceLibId = item.DeviceLibId,
                        DeviceLibName = item.DeviceLibName,
                        DownInsertStatus = item.DownInsertStatus,
                        DownInsertStatusDt = item.DownInsertStatusDt,
                        DownUpdateStatus = item.DownUpdateStatus,
                        DownUpdateStatusDt = item.DownUpdateStatusDt,
                        DownDelStatus = item.DownDelStatus,
                        DownDelStatusDt = item.DownDelStatusDt,
                        SynchronizedStatus = synchronizedStatus,
                        //--------------------------------------------------------------------
                        MaincomId = item.MaincomId,
                        OuterId = item.OuterId,
                        LibId = item.LibId,
                        LibIdGroupsList = string.IsNullOrEmpty(item.LibIdGroups) ? libraryItemXs : PersonBusiness.GetLibIdGroupsList(item.LibIdGroups),
                        LibName = ftLibrary?.Name ?? string.Empty,
                        CardNo = item.CardNo,
                        Category = item.Category,
                        PicUrl = ftPicture?.PicUrl ?? string.Empty,
                        PicClientUrl = ftPicture?.PicClientUrl ?? string.Empty
                    };

                    devicePersonModelList.Add(devicePersonModel);
                }
                if (!string.IsNullOrEmpty(input.Search))
                { 
                    if (int.TryParse(input.Search, out int searchPersonId))
                    {
                        devicePersonModelList = devicePersonModelList.Where(c => c.PersonId == searchPersonId).ToList();
                    }
                    else
                    {
                        input.Search = WebUtility.UrlDecode(input.Search).Trim();
                        devicePersonModelList.Where(c => c.PersonName.Contains(input.Search));
                    }
                }
                
                DevicePersonListReturn devicePersonListReturn = new DevicePersonListReturn();
                var newItems = devicePersonModelList.ToPagedList(input.PageNo, input.PageSize);
                devicePersonListReturn.PageCount = newItems.PageCount;
                devicePersonListReturn.PageNo = newItems.PageNumber;
                devicePersonListReturn.PageSize = newItems.PageSize;
                devicePersonListReturn.TotalCount = newItems.Count();
                devicePersonListReturn.DevicePersonModelList = newItems.ToList();
                responseModalX = new ResponseModalX
                {
                    data = devicePersonListReturn
                };
                return SwitchToApiOrView(responseModalX);
            }
            catch (Exception ex)
            {
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_LIST_FAIL, Success = false, Message = $"{Lang.GeneralUI_Device}{Lang.PERSON_LIST_FAIL} [Exception][{ex.Message}]" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
        }
        #endregion

        /// <summary>
        /// POST LOG 记录 [API 验证 与 设备序列号 验证]
        /// http://81.71.74.135:5002/zh-HK/Admin/DeviceManage/AttendancePost
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/Admin/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult AttendancePost([FromBody] AttendancePostEntry entry)
        {
            string loggerline;
            try
            {
                string attendancePostLog = JsonConvert.SerializeObject(entry);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceManageController.AttendancePost][ENTRY JSON]\n{attendancePostLog}";
                Logger.LogWarning(attendancePostLog);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceManageController.AttendancePost][JsonConvert.SerializeObject(entry)][EXCEPTION]\n{ex.Message}");
                CommonBase.OperateDateLoger($"[FUNC::DeviceManageController.AttendancePost][JsonConvert.SerializeObject(entry)][EXCEPTION]\n{ex.Message}");
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
                loggerline = $"[3 NONE (FACE,CARD,EMPLOYEENO)][{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceManageController.AttendancePost][{JsonConvert.SerializeObject(entry)}][RETURN AND NOT HANDLE DATA]";
                Logger.LogInformation(loggerline);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "[3 NONE (FACE,CARD,EMPLOYEENO)]" };
                return Ok(responseModalX);
            }
            //验证合法的设备 驗證傳入的設備id和序列號
            FtDevice device = new FtDevice();
            if (string.IsNullOrEmpty(entry.DeviceId) || string.IsNullOrEmpty(entry.DeviceSerialNo))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message =$"{Lang.ILLEGAL_DEVICE_SERIAL_NUMBER } OR { Lang.Device_IlleggleDeviceId}" };
                return Ok(responseModalX);
            }
            else
            {
                if(int.TryParse(entry.DeviceId,out int intDeviceId))
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

                    //設備模式和考勤模式的轉換
                    entry.AttendanceMode = (int)DeviceBusiness.DeviceTypeToAttendanceMode((DeviceType)device.DeviceType);
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
                if(!string.IsNullOrEmpty(entry.PhysicalId))
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
                entry.Occur = DateTimeHelp.ConvertToMillSecond(entry.Occur);
                 
                HistoryRecordsDefault historyRecordsDefault = new HistoryRecordsDefault();
                historyRecordsDefault.MaincomId = device.MaincomId;
                historyRecordsDefault.Id = entry.Occur;
                historyRecordsDefault.DeviceId = device.DeviceId;
                historyRecordsDefault.DeviceName = device.DeviceName;
                historyRecordsDefault.Mode = (sbyte)entry.AttendanceMode; //考勤類型 可通過 DeviceType做個函數對比轉換過來,後續開發
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
                    retOfPersonByOuterId = PersonBusiness.CheckExistPersonByOuterId(device.MaincomId, entry.EmployeeNo, ref ftPerson, ref responseModalX,ref picPath);

                    if(retOfPersonByOuterId)
                    {
                        ////如果被锁定的人员不能拍卡/人臉識別等  不判断是否锁定要上存 锁定仅for 设备用户同步使用 [Lang.PERSON_IS_BLOCKED_TIPS]
                        
                        historyRecordsDefault.PersonId = ftPerson.Id;
                        historyRecordsDefault.PersonName = ftPerson.Name??string.Empty;
                        historyRecordsDefault.CardNo = ftPerson.CardNo;
                        historyRecordsDefault.Category = (sbyte)ftPerson.Category; //PersonCategory.UNBLOCKED,
                        historyRecordsDefault.Classify = (sbyte)CLASSIFY.IS_PERSON; //不是陌生人
                        historyRecordsDefault.PicPath = picPath;
                    }
                }
                PersonCardInfo personCardInfo = new PersonCardInfo();
                bool retOfPersonByCard = false;
                //卡號優先
                if(!string.IsNullOrEmpty(entry.PhysicalId))
                { 
                    retOfPersonByCard = DeviceBusiness.IsCardNumberRelatePerson(device.MaincomId, entry.PhysicalId, ref personCardInfo);
                     
                    if (retOfPersonByCard)
                    {
                        //如果被锁定的人员不能拍卡 ////如果被锁定的人员不能拍卡/人臉識別等  不判断是否锁定要上存 锁定仅for 设备用户同步使用 [Lang.PERSON_IS_BLOCKED_TIPS]
                         
                        historyRecordsDefault.PersonId = personCardInfo.PersonId;
                        //outerId
                        historyRecordsDefault.CardNo = personCardInfo.CardNo;
                        historyRecordsDefault.PersonName = personCardInfo.Name;
                        historyRecordsDefault.Classify = (sbyte)CLASSIFY.IS_PERSON; //不是陌生人
                        historyRecordsDefault.Classify = (sbyte)personCardInfo.Category; 
                    }else if(!string.IsNullOrEmpty(entry.PhysicalId))
                    {
                        historyRecordsDefault.CardNo = entry.PhysicalId;
                    }
                }

                if (!string.IsNullOrEmpty(entry.Face) && (entry.AttendanceMode != (int)AttendanceMode.FACE || entry.AttendanceMode != (int)AttendanceMode.FACE_CARD_VERIFY))
                {
                    attendanceMode = AttendanceMode.FACE;
                   
                    entry.AttendanceMode = (int)attendanceMode;
                }
                if (!string.IsNullOrEmpty(entry.Face))
                     historyRecordsDefault.CapturePath = entry.Face;

                bool result = HistRecBusiness.AddNewHistRecordForAttendancePost(historyRecordsDefault,device, ref responseModalX);
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
         
        #region MULTI_DEVICE_SYNC 前端软件的接口
        /// <summary>
        /// 獲取同步的設備用戶列表 以及狀態
        /// 通过验证模式来(VerifyMode(媒體介質模式))获得设备人员,用于同步下行到设备  // ACCESS_CARD | FINGERPRINT | FACE | PASSKEY 四种介質模式的人员
        /// 同時 要有 DeviceOperateMode 操作模式
        /// DEVICE_OPERATE_MODE_INSERT = 1, //同步增加 
        /// DEVICE_OPERATE_MODE_UPDATE = 2,//同步更新 
        /// DEVICE_OPERATE_MODE_DELETE = 3 //同步删除
        /// 同步完成後標記為完成 失敗 標記為FAIL
        /// </summary>
        /// <param name="DeviceId">設備ID下的用戶</param>
        /// <param name="Quantity">每次請求的用戶數量 定時輪詢操作就可以完成所有用戶列表</param>
        /// 增加分页对象接口
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/Admin/[controller]/[action]")]
        public IActionResult QueryDeviceUserList([FromBody] QueryDeviceUserInput input) //application/json
        {
            input.Page = input.Page == 0 ? 1 : input.Page;
            input.PageSize = input.PageSize == 0 ? 100 : input.PageSize;

#if DEBUG
            input.PageSize = 5;
#endif 

            ResponseModalX responseModalX = new ResponseModalX();
            DateTime dt1 = DateTime.Now;
            try
            {
                string loggerLine = $"[{dt1:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageControllerController.QueryDeviceUserList][GET][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }
            catch (Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][FUNC::DeviceManageControllerController.QueryDeviceUserList][INPUT][EXCEPTION:{ex.Message}][GET][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }

            using BusinessContext dbContext = new BusinessContext();

            //由於公司ID僅僅是與登錄用戶ID關聯,目前沒有MainCom這個表,不能以FtUser表來判斷,只能通過設備表\Camera\Person等表來做判斷
            //一下這個從DGX轉移過來的,在此不適用!
            //if (this.CheckMaincomConsistency(input.MainComId) == false)

            if (!int.TryParse(input.DeviceId, out int deviceId)) //要求Int,否則返回
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"{input.DeviceId} {Lang.Device_IlleggleDeviceId}{input.DeviceId}" };
                return Ok(responseModalX);
            }
            var deviceUsers = from s in dbContext.FtDevicePerson.Where(c => c.DeviceId == deviceId)
                              select s;

            if (!string.IsNullOrEmpty(input.Search))
            {
                input.Search = WebUtility.UrlDecode(input.Search).Trim();
                deviceUsers = deviceUsers.Where(c => c.PersonName.Contains(input.Search));
            }

            if (!Enum.TryParse<DeviceVerifyMode>(input.VerifyMode, out DeviceVerifyMode verifyMode))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Input.VerifyMode UNFORMAT" };
                return Ok(responseModalX);
            }
            if (!Enum.TryParse<DeviceOperateMode>(input.DeviceOperateMode, out DeviceOperateMode deviceOperateMode))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Input.DeviceOperateMode UNFORMAT" };
                return Ok(responseModalX);
            }

            QueryDeviceUserModel query = new QueryDeviceUserModel
            {
                DeviceId = input.DeviceId,
                MainComId = input.MainComId,
                VerifyMode = verifyMode,
                DeviceOperateMode = deviceOperateMode
            };

            //按照请求状态获得对应状态的用户任务（下行增加，下行更新，下行删除）
            switch (query.DeviceOperateMode)
            {
                case DeviceOperateMode.DEVICE_OPERATE_MODE_INSERT:
                    deviceUsers = deviceUsers.Where(c => c.DownInsertStatus == (int)DevicePersonDownInsertStatus.DEVICE_PERSON_DOWN_INSERT_WAIT);
                    break;
                case DeviceOperateMode.DEVICE_OPERATE_MODE_UPDATE:
                    deviceUsers = deviceUsers.Where(c => c.DownUpdateStatus == (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_WAIT);
                    break;
                case DeviceOperateMode.DEVICE_OPERATE_MODE_DELETE:
                    deviceUsers = deviceUsers.Where(c => c.DownDelStatus == (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_WAIT);
                    break;
                default:
                    break;
            }

            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2.Subtract(dt1);

            //沒必要轉換,返回一個對象,然前端判斷 只要返回一個SynchronizeStatus 對象回去讓前端判斷
            List<StandardDeviceUser> standardDeviceUserList = DevManageBusiness.GetDeviceUsersByNeedToSync(UploadFolderPath, query.VerifyMode, deviceUsers.ToList());
            if (standardDeviceUserList?.Count() > 0)
            {
                StandardDeviceUserListReturn standardReturn = new StandardDeviceUserListReturn();
                var newItems = standardDeviceUserList.ToPagedList(input.Page, input.PageSize);
                standardReturn.PageCount = newItems.PageCount;
                standardReturn.PageNo = newItems.PageNumber;
                standardReturn.PageSize = newItems.PageSize;
                standardReturn.TotalCount = newItems.Count();
                standardReturn.StandardDeviceUserList = newItems.ToList();

                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"SUCCESS (QUERY TIME : {ts.TotalSeconds}Seconds)" };
                responseModalX.data = standardReturn;
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.LIST_NO_RECORD };
                responseModalX.data = null;
            }

            string timelog = $"[{dt2:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageControllerController.QueryDeviceUserList][QUERY TIME : {ts.TotalSeconds} seconds]";
            Logger.LogInformation(timelog);

            return Ok(responseModalX);
        }

        /// <summary>
        /// 終端設備同步完成後,回傳操作的狀態 插入 刪除 更新
        /// 具體描述在 DeviceErrorAndStateCode.cs 
        /// </summary>
        /// <param name="input.EquipmentUserId">用於低級設備回傳 填入數字的EmployeeId</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/Admin/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult TerminalEquipmentCallBack(TerminalEquipmentInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            string loggerline;
            try
            {
                string inputJson = JsonConvert.SerializeObject(input);
                loggerline = $"[FUNC::DeviceManageController.TerminalEquipmentCallBack][INPUT][{inputJson}]";
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}]{loggerline}");
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DeviceManageController.TerminalEquipmentCallBack][JsonConvert.SerializeObject(INPUT)][EXCEPTION]\n{ex.Message}";
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);
            }

            if (!int.TryParse(input.DeviceId, out int deviceId)) //要求Int,否則返回
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"[AIG SYSTEM]{input.DeviceId} {Lang.Device_IlleggleDeviceId}" };
                return Ok(responseModalX);
            }

            if (!long.TryParse(input.EmployeeId, out long personId)) //要求Int,否則返回
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"[AIG SYSTEM]{input.EmployeeId}[EmployeeId Required Digital]" };
                return Ok(responseModalX);
            }
            else if (!long.TryParse(input.EquipmentUserId, out personId)) //要求Int,否則返回
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"[AIG SYSTEM][{input.EquipmentUserId}][EquipmentUserId Required Digital]" };
                return Ok(responseModalX);
            }

            if (!Enum.TryParse<DeviceOperateMode>(input.DeviceOperateMode, out DeviceOperateMode DeviceOperateMode))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Input.DeviceOperateMode UNFORMAT" };
                return Ok(responseModalX);
            }

            if (!Enum.TryParse<DeviceVerifyMode>(input.DeviceVerifyMode, out DeviceVerifyMode DeviceVerifyMode))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Input.DeviceVerifyMode UNFORMAT" };
                return Ok(responseModalX);
            }

            if (!Enum.TryParse<DevicePersonDownStatus>(input.DevicePersonDownStatus, out DevicePersonDownStatus devicePersonDownStatus))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Input.DevicePersonDownStatus UNFORMAT" };
                return Ok(responseModalX);
            }
            responseModalX = DevManageBusiness.ChangeDeviceUsersStatusSyncCallBack(UploadFolderPath, DeviceOperateMode, DeviceVerifyMode, devicePersonDownStatus, deviceId, personId);

            return Ok(responseModalX);
        }

        /// <summary>
        /// 通過設備ID獲取 設備配置信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/Admin/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetDeviceConfig(DeviceConfigInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (int.TryParse(input.DeviceId, out int deviceId) == false)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = DeviceErrorCode.ILLEGAL_DEVICE_ID.GetEnumDesc() };
                return Ok(responseModalX);
            }
            //檢測MainComId一致性
            using BusinessContext businessContext = new BusinessContext();
            var device = businessContext.FtDevice.Find(deviceId);

            if (device == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Message = $"{DeviceErrorCode.DEVICE_NOT_EXIST.GetEnumDesc()} {DeviceErrorCode.ILLEGAL_DEVICE_ID.GetEnumDesc()}" };
                return Ok(responseModalX);
            }
            if (device.MaincomId != input.MainComId)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = Lang.GeneralUI_MainComIdRequired };
                return Ok(responseModalX);
            }
            object obj = DeviceBusiness.ReturnDeviceConfigObject(deviceId);
            if (obj == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.Device_NotExistByDeviceServialNo };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceManageController.GetDeviceConfig]" +
                    $"[deviceId.:{deviceId}]\n[ReturnDeviceConfigObject][return obj = null ]");

                return Ok(responseModalX);
            }
            responseModalX.data = obj;
            return Ok(responseModalX);
        }

        /// <summary>
        /// 通過設備ID取得 device model
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="strDeviceId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Language}/Admin/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeviceDetails(string mainComId, string strDeviceId)  //GetDeviceDetails
        {
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::DeviceManageController.DeviceDetails][MainComId:{mainComId}][DEVICE Id:{strDeviceId}]\n");

            ResponseModalX responseModalX = new ResponseModalX();

            if (!int.TryParse(strDeviceId, out int deviceId)) //要求Int,否則返回
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"{strDeviceId} {Lang.Device_IlleggleDeviceId}" };
                return Ok(responseModalX);
            }

            DeviceModel deviceModel = new DeviceModel();

            BusinessContext dataBaseContext = new BusinessContext();
            var device = dataBaseContext.FtDevice.Find(deviceId);

            if (device.MaincomId != mainComId)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"{mainComId} {Lang.GeneralUI_NoMatchMainComId}" };
                return Ok(responseModalX);
            }

            if (device != null)
            {
                if (string.IsNullOrEmpty(device.DeviceConfig))
                {
                    object obj = DeviceBusiness.ReturnDeviceConfigObject(deviceId);
                    if (obj != null)
                    {
                        var serializerSettings = new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Formatting = Formatting.Indented
                        };
                        device.DeviceConfig = JsonConvert.SerializeObject(obj, serializerSettings);
                    }
                }

                MainCom mainCom = new MainCom(); //默認值 
                deviceModel.DeviceName = device.DeviceName ?? string.Empty;
                deviceModel.SiteId = mainCom.SiteId;//設備系統沒有地盤ID(位置ID) 
                deviceModel.DeviceId = device.DeviceId.ToString();
                deviceModel.SysModuleId = mainCom.SysModuleId; //設備系統沒有模塊ID
                deviceModel.DeviceEntryMode = (EnumBusiness.DeviceEntryMode)device.DeviceEntryMode;
                deviceModel.DeviceSerialNo = device.DeviceSerialNo;
                deviceModel.Status = device.Status;
                deviceModel.Config = device.DeviceConfig;
                deviceModel.MainComId = device.MaincomId;
                deviceModel.OperatedUser = mainCom.SystemDefaultUser;
                deviceModel.IsReverseHex = device.IsReverseHex > 0;

                responseModalX.data = deviceModel;

                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][SUCCESS][FUNC::DeviceManageController.DeviceDetails]" +
                    $"[strDeviceId.:{strDeviceId}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");

                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.DEVICE_SERIALNO_NOT_EXIST, Success = false, Message = $"{ Lang.DEVICE_DEVICE_ID_UNFORMAT} OR MAINCOM ID" };
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FAIL][FUNC::DeviceManageController.DeviceDetails][{Lang.DEVICE_SERIALNO_NOT_EXIST}]" +
                    $"[strDeviceId.:{strDeviceId}]\n[RESPONSE]{JsonConvert.SerializeObject(responseModalX)}");
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 对处于无操作状态下，重新提交， 请求下行操作，把无操作改为 WAIT
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/Admin/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DevicePersonDownSubmit(OperateStatusSubmitInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            string loggerline;
            try
            {
                string inputJson = JsonConvert.SerializeObject(input);
                loggerline = $"[FUNC::DeviceManageController.DevicePersonDownSubmit][INPUT][{inputJson}]";
                Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::DeviceManageController.DevicePersonDownSubmit][INPUT]{loggerline}");
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DeviceManageController.DevicePersonDownSubmit][JsonConvert.SerializeObject(INPUT)][EXCEPTION]\n{ex.Message}";
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);
            }

            using BusinessContext businessContext = new BusinessContext();

            var devicePerson = businessContext.FtDevicePerson.Where(c => c.DeviceId == input.DeviceId && c.PersonId == input.PersonId).FirstOrDefault();

            if (devicePerson == null)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GeneralUI_NoRecord };
                return Ok(responseModalX);
            }

            switch (input.DeviceOperateMode)
            {
                case DeviceOperateMode.DEVICE_OPERATE_MODE_INSERT:

                    devicePerson.DownInsertStatus = (int)DevicePersonDownInsertStatus.DEVICE_PERSON_DOWN_INSERT_WAIT;
                    devicePerson.DownInsertStatusDt = DateTime.Now;
                    break;

                case DeviceOperateMode.DEVICE_OPERATE_MODE_UPDATE:

                    devicePerson.DownUpdateStatus = (int)DevicePersonDownUpdateStatus.DEVICE_PERSON_DOWN_UPDATE_WAIT;
                    devicePerson.DownUpdateStatusDt = DateTime.Now;
                    break;

                case DeviceOperateMode.DEVICE_OPERATE_MODE_DELETE:

                    devicePerson.DownDelStatus = (int)DevicePersonDownDelStatus.DEVICE_PERSON_DOWN_DEL_WAIT;
                    devicePerson.DownDelStatusDt = DateTime.Now;
                    break;

                default:
                    break;
            }
            try
            {
                businessContext.FtDevicePerson.Update(devicePerson);
                businessContext.SaveChanges();

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = $"{Lang.GeneralUI_OK} [DEVICE USER STATUS UPDATE SUCCESS]" };
                string personName = devicePerson.PersonName;
                int deviceId = input.DeviceId;
                string personId = devicePerson.PersonId.ToString();
                responseModalX.data = new { personId, personName, deviceId };
                return Ok(responseModalX);
            }
            catch (Exception ex)
            {
                string err = $"[FUNC::DevManageBusiness.ChangeDeviceUsersStatusSyncCallBack][DATABASE SAVE][{ex.Message}]";
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{Lang.GeneralUI_Fail} {err}" };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 获得 群组列表
        /// 用于前端设备用户提交到云端时候，选择对应的分组
        /// 这是一个标准的分组接口，不涉及复杂逻辑。没有递归树状结构。
        /// </summary>
        /// <param name="mainComId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Language}/Admin/[controller]/[action]/{mainComId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetMainComGroup(string mainComId)
        {
            Logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:MM:ss fff}][FUNC::DeviceController.GetMainComGroup][GET GROUPS BY {mainComId}]\n");

            ResponseModalX responseModalX = new ResponseModalX();
              
            BusinessContext dataBaseContext = new BusinessContext(); 
            var grouplists = dataBaseContext.FtLibrary
                .Where(c=>c.MaincomId.Contains(mainComId))
                .Select(c=> new GroupsForApi { GroupId = c.Id.ToString(), ParentsGroupId = c.LibId.ToString(), GroupName = c.Name}).ToList() ;

            if(grouplists?.Count()>0)
            {
                responseModalX.data = grouplists;
            }
            else
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.LIST_NO_RECORD, Success = false, Message = Lang.LIST_NO_RECORD };
                responseModalX.data = null; 
            }
            return Ok(responseModalX);
        }

        /// <summary>
        /// 添加标准人员API入口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/Admin/[controller]/[action]")]
        [HttpPost]
        public IActionResult AddStandardPerson([FromBody]PersonStandardInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            try
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageController.AddStandardPerson][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }
            catch (Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][FUNC::DeviceManageController.AddStandardPerson][INPUT][EXCEPTION:{ex.Message}][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
                CommonBase.OperateDateLoger(loggerLine, CommonBase.LoggerMode.FATAL);
            }
             
            if (string.IsNullOrEmpty(input.MainComId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            if (string.IsNullOrEmpty(input.Name))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.PERSON_INVALID_NAME };
                return Ok(responseModalX);
            }

            if (input.Category == PersonCategory.GUEST)
            {
                input.Name += $"{DateTime.Now:MMddHHmm}";
            }
             
            using BusinessContext businessContext = new BusinessContext(); 
            if (int.TryParse(input.GroupId, out int libId))
            {
                if(libId != 0)
                {
                    FtLibrary ftLibrary = businessContext.FtLibrary.Find(libId);
                    if (ftLibrary == null)
                    {
                        ftLibrary = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(input.MainComId)).Take(1).FirstOrDefault();
                        libId = ftLibrary.Id;
                    }
                    else
                    {
                        libId = ftLibrary.Id;
                    }
                }
                else
                {
                    FtLibrary ftLibrary = businessContext.FtLibrary.Where(c=>c.MaincomId.Contains(input.MainComId)).Take(1).FirstOrDefault();
                    libId = ftLibrary.Id;
                }
            }
            else
            {
                FtLibrary ftLibrary = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(input.MainComId)).Take(1).FirstOrDefault();
                libId = ftLibrary.Id;
            }

            DateTime dt = DateTime.Now;
            long maxId = 60000;  //以6萬開始,禁用個位數,個位數遇到contains查詢數組就會不準確
            if (businessContext.FtPerson.Count() > 0)
            {
                maxId = businessContext.FtPerson.Max(c => c.Id) + 1;
            }

            //判断OuterId是否存在 其他记录中
            //在此情景下就是EmployeeNo (工号)
            if (!string.IsNullOrEmpty(input.OuterId))
            {
                input.OuterId = input.OuterId.Trim();
                var existPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(input.OuterId) && c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(input.MainComId)).FirstOrDefault();
                if (existPerson != null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Exist {Lang.Person_OuterId} :{existPerson.Name}" };
                    return Ok(responseModalX);
                }
            } //否则 OuterId就是保存 的是工号 EmployeeNo (海康命名工号)

            //檢測卡號是否被佔用 | 沒有=false, Add new 的case下,personId = 0
            if (!string.IsNullOrEmpty(input.CardNo))
            {
                if (PersonBusiness.CheckPersonCardNoOccupied(input.MainComId, 0, input.CardNo, ref responseModalX) == true)
                {
                    return Ok(responseModalX);
                }
            }

            //CheckPersonCardNoOccupied 檢測是否被別人佔據卡號 
            if(!string.IsNullOrEmpty(input.CardNo))
            {
                bool isOccupied = PersonBusiness.CheckPersonCardNoOccupied(input.MainComId, 0, input.CardNo, ref responseModalX);
                if (isOccupied)
                {
                    input.CardNo = string.Empty; //如果卡號被佔有,則不錄入卡號到此人員
                }
            }
           

            FtPerson ftPerspon = new FtPerson
            {
                Id = maxId,
                OuterId = input.OuterId,
                MaincomId = input.MainComId,
                LibId = libId,
                LibIdGroups = string.Empty,
                Name = input.Name,
                Sex = (sbyte)Genders.UNKOWN, 
                CardNo = string.IsNullOrEmpty(input.CardNo) ? "" : input.CardNo,
                PassKey = string.IsNullOrEmpty(input.PassKey) ? "" : input.PassKey,
                Phone = string.Empty,
                Category = (sbyte)input.Category,
                Remark = string.IsNullOrEmpty(input.Remark) ? "" : input.Remark,
                Visible = (sbyte)PersonErrorCode.PERSON_IS_VISIBLE,
                CreateTime = dt,
                UpdateTime = dt
            };

            businessContext.FtPerson.Add(ftPerspon);
            bool result = businessContext.SaveChanges() > 0 ? true : false;
            if (result)
            {

                SimpleStandardUserReturn simpleStandard  = new SimpleStandardUserReturn
                {
                    Id = ftPerspon.Id.ToString(),
                    EmployeeId = ftPerspon.Id.ToString(),
                    EmployeeNo = ftPerspon.OuterId,
                    OuterId = ftPerspon.OuterId,
                    MainComId = ftPerspon.MaincomId,
                    Name = ftPerspon.Name
                };
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC },
                    data = simpleStandard
                };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.GeneralUI_Fail },
                    data = null
                };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 人员頭像保存
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/Admin/[controller]/[action]")]
        [HttpPost]
        public IActionResult SaveEmployeePictRecord([FromBody] PersonStdPicInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            try
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageController.SaveEmployeePictRecord][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }
            catch (Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][FUNC::DeviceManageController.SaveEmployeePictRecord][INPUT][EXCEPTION:{ex.Message}][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
                CommonBase.OperateDateLoger(loggerLine, CommonBase.LoggerMode.FATAL);
            }

            if (string.IsNullOrEmpty(input.MainComId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            if (string.IsNullOrEmpty(input.OuterId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();

            var person = new FtPerson(); 
            
            //判断OuterId是否存在 其他记录中
            if (!string.IsNullOrEmpty(input.OuterId))
            {
                input.OuterId = input.OuterId.Trim();
                var existPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(input.OuterId) && c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(input.MainComId)).FirstOrDefault();
                if (existPerson == null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.GeneralUI_NoRecord} {Lang.Person_OuterId} :{existPerson.Name}" };
                    return Ok(responseModalX);
                } 
                person = existPerson;
            }

            DateTime dt = DateTime.Now;
            long ftPictureNewId = 1;
            if (businessContext.FtPicture.Count() > 0)
            {
                ftPictureNewId = businessContext.FtPicture.Max(c => c.Id) + 1;
            }

            FtPicture ftPicture = new FtPicture
            {
                Id = ftPictureNewId,
                PersonId = person.Id,
                PicId = ftPictureNewId,
                Visible = (sbyte)PictureErrorCode.PICTURE_IS_VISIBLE,
                PicUrl = input.PicturePath??String.Empty,
                PicClientUrl = input.PicturePath ?? String.Empty,
                PicClientUrlBase64 = string.Empty, //--------------------------------------------- remain to aplly other function
                CreateTime = dt,
                UpdateTime = dt
            };

            if (PersonBusiness.GetUrlPathFileNameId(input.PicturePath, out long fileNameId))
            {
                ftPicture.PicClientUrlBase64 = String.Empty; 
            }

            try
            {
                businessContext.FtPicture.Add(ftPicture);
                bool resultUpdatePicture = businessContext.SaveChanges() > 0 ? true : false;
                
                if(resultUpdatePicture)
                {
                    PicReturn picReturn = new PicReturn
                    {
                        MainComId = person.MaincomId,
                        EmployeeId = person.Id.ToString(),
                        EmployeeNo = person.OuterId,
                        EmployeeName = person.Name,
                        PicActuallyPath = $"{HttpHost}/{_UploadSetting.TargetFolder}/{fileNameId}.jpg",
                        PicturePath = input.PicturePath
                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = picReturn
                    };
                    return Ok(responseModalX);
                }
                else
                {
                    string logline = string.Format("[]FUNC:DeviceManageController.SaveEmployeePictRecord() {0} | {1}", Lang.PICTURE_UPDATE_FAIL, JsonConvert.SerializeObject(input));
                    CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO); 
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = logline };
                    return Ok(responseModalX);
                }
            }
            catch (Exception ex)
            {
                string logLine = $"[AddPerson][FtPicture][Add] [EXCEPTION][{ex.Message}]";
                CommonBase.OperateDateLoger(logLine, CommonBase.LoggerMode.FATAL);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = ex.Message };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 人員密碼保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="OuterId">OuterId 在上存設備人員的時候,等於FtPerson.Id一樣規則,不能重複,等同於海康的EmployeeNo工號,是專門For上載設備人員到平台使用的</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/Admin/[controller]/[action]")]
        [HttpPost]
        public IActionResult SaveEmployeePassKeyRecord([FromBody] PersonStdPassKeyInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageController.SaveEmployeePassKeyRecord][POST][{JsonConvert.SerializeObject(input)}]";
            Logger.LogInformation(loggerLine);

            if (string.IsNullOrEmpty(input.MainComId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            if (string.IsNullOrEmpty(input.OuterId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_UNFORMAT_OUTTER_ID, Message = Lang.PERSON_UNFORMAT_OUTTER_ID };
                return SwitchToApiOrView(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();
             
            //判断OuterId是否存在 其他记录中
            string picPath = "";
            FtPerson ftPerson = new FtPerson(); 
            bool checkOuterIdExist = PersonBusiness.CheckExistPersonByOuterId(input.MainComId, input.OuterId,ref ftPerson, ref responseModalX, ref picPath);
            if (checkOuterIdExist)
            {
                //檢測passkey的合法性
                if(string.IsNullOrEmpty(input.PassKey))
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_INVALID_PASSKEY, Message = $"{Lang.PERSON_INVALID_PASSKEY}" };
                    return Ok(responseModalX);
                }

                ftPerson.PassKey = input.PassKey.Trim();

                businessContext.FtPerson.Update(ftPerson);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                int errorCode = result ? (int)GeneralReturnCode.SUCCESS : (int)GeneralReturnCode.FAIL;
                string message = result ? "OK" : "FAIL";
                responseModalX.meta = new MetaModalX { Success = result, ErrorCode = errorCode, Message = message };
                return Ok(responseModalX); 
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = $"{PersonErrorCode.PERSON_NOT_EXIST.GetEnumDesc()} {Lang.Person_OuterId}" };
                return Ok(responseModalX);
            }
        }

        /// <summary>
        /// 人員卡號保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="OuterId">OuterId 在上存設備人員的時候,等於FtPerson.Id一樣規則,不能重複,等同於海康的EmployeeNo工號,是專門For上載設備人員到平台使用的</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/Admin/[controller]/[action]")]
        [HttpPost]
        public IActionResult SaveEmployeeCardNoRecord([FromBody] PersonStdCardNoInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::DeviceManageController.SaveEmployeeCardNoRecord][POST][{JsonConvert.SerializeObject(input)}]";
            Logger.LogInformation(loggerLine);

            if (string.IsNullOrEmpty(input.MainComId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            if (string.IsNullOrEmpty(input.OuterId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_UNFORMAT_OUTTER_ID, Message = Lang.PERSON_UNFORMAT_OUTTER_ID };
                return SwitchToApiOrView(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();

            //判断OuterId是否存在 其他记录中
            string picPath = "";
            FtPerson ftPerson = new FtPerson();
            bool checkOuterIdExist = PersonBusiness.CheckExistPersonByOuterId(input.MainComId, input.OuterId, ref ftPerson, ref responseModalX, ref picPath);
            if (checkOuterIdExist)
            {
                //檢測CardNo的合法性
                if (string.IsNullOrEmpty(input.CardNo))
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_CARD_NUMBER_UNFORMAT, Message = $"{Lang.PERSON_CARD_NUMBER_UNFORMAT}" };
                    return Ok(responseModalX);
                }
                input.CardNo = input.CardNo.Trim();
                //CheckPersonCardNoOccupied 檢測是否被別人佔據卡號
                bool isOccupied = PersonBusiness.CheckPersonCardNoOccupied(input.MainComId, ftPerson.Id, input.CardNo, ref responseModalX);
                if(isOccupied)
                {
                    responseModalX.data = "返回一個佔用情況的data對象"; 
                    return Ok(responseModalX);
                }
                ftPerson.CardNo = input.CardNo;

                businessContext.FtPerson.Update(ftPerson);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                int errorCode = result ? (int)GeneralReturnCode.SUCCESS : (int)GeneralReturnCode.FAIL;
                string message = result ? "OK" : "FAIL";
                responseModalX.meta = new MetaModalX { Success = result, ErrorCode = errorCode, Message = message };
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = $"{PersonErrorCode.PERSON_NOT_EXIST.GetEnumDesc()} {Lang.Person_OuterId}" };
                return Ok(responseModalX);
            }
        }

        #endregion MULTI_DEVICE_SYNC 前端软件的接口
    }
}
