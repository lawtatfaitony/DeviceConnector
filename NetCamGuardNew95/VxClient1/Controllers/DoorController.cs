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
    public class DoorController : BaseController
    {
        private string HttpHost;
        private IOptions<UploadSetting> _uploadSetting;

        private string wwwRoot { get; set; }
        public DoorController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, ILogger<BaseController> logger ,IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement, IOptions<UploadSetting> uploadSetting)
             : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            Logger = logger;
            wwwRoot = webHostEnvironment.WebRootPath;
        }
         
        /// <summary>
        ///  獲取門禁的JSON TREE 樹狀圖列表 
        ///  http://localhost:5002/zh-HK/door/GetDoorsTreeNodeOfSite/6000014/0
        /// </summary>
        /// <param name="maincomId"></param>
        /// <param name="parentsId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Language}/[controller]/[action]/{maincomId}/{parentsId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetDoorsTreeNodeOfSite(string maincomId,int? parentsId)
        {
            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::GetDoorsTreeNodeOfSite][{maincomId}]";
            Logger.LogInformation(loggerline);

            using BusinessContext businessContext = new BusinessContext();

            List<FtSite> doorModelList = businessContext.FtSite.Where(c=>c.MaincomId==maincomId).ToList();
            int pId = parentsId ??= 0;
            DoorBusiness.GetDoorTreeJson(doorModelList, pId);
            string jsonTree = DoorBusiness.SiteResult.ToString();
            DoorBusiness.SiteResult.Clear();
            return Ok(jsonTree);
        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]/{sDoorId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DoorTreePanel(string sDoorId)
        {
            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC::DoorTreePanel]";
            Logger.LogInformation(loggerline);

            ResponseModalX responseModalX = new ResponseModalX();
            Door door = new Door
            {
                DeviceId =0,
                MaincomId = WebCookie.MainComId,
                DoorId = 0,
                DoorName = string.Empty,
                SiteId =0
            };
            responseModalX.data = door;
            if (!string.IsNullOrEmpty(sDoorId))
            {
                if(int.TryParse(sDoorId, out int doorId))
                {
                    using BusinessContext businessContext = new BusinessContext();

                    FtDoor ftDoor = businessContext.FtDoor.Find(doorId);
                    if(ftDoor!=null)
                    {
                        door = new Door
                        {
                            DeviceId = ftDoor.DeviceId.GetValueOrDefault(),
                            MaincomId = ftDoor.MaincomId,
                            DoorId = ftDoor.DoorId,
                            DoorName = ftDoor.DoorName,
                            SiteId = ftDoor.SiteId
                        };
                        responseModalX.data = door;
                    } 
                }
            }

            return SwitchToApiOrView(responseModalX);
        }

        /// <summary>
        /// 新增或更新門的資料
        /// </summary>
        /// <param name="door"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult AddOrUpdateDoor(Door door)
        {
            string loggerline;
            try
            {
                string doorJson = JsonConvert.SerializeObject(door);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DoorController.AddOrUpdateDoor][DOOR INPUT JSON]\n{doorJson}";
                Logger.LogWarning(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DoorController.AddOrUpdateDoor][DOOR INPUT][EXCEPTION]{ex.Message}";
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline);
            }

            ResponseModalX responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();

            if (string.IsNullOrEmpty(door.DoorName))
            {
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode = (int)DoorErrorCode.DOOR_RUQIRED_SITE,
                    Success = false,
                    Message = $"[{Lang.Door_DoorName}] {Lang.GeneralUI_Required} "
                };
                return Ok(responseModalX);
            }

            if (door.SiteId==0)
            {
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode=(int)DoorErrorCode.DOOR_RUQIRED_SITE, Success =false, Message = $"{Lang.GeneralUI_Fail} [{Lang.DOOR_RUQIRED_SITE}]"
                };
                return Ok(responseModalX);
            }
            var site = businessContext.FtSite.Find(door.SiteId);

            FtDevice device = new FtDevice(); 
            if (door.DeviceId == 0)
            { 
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode = (int)DoorErrorCode.DOOR_RUQIRED_DEVICE,
                    Success = false,
                    Message = $"{Lang.GeneralUI_Fail} [{Lang.DOOR_RUQIRED_DEVICE}]"
                };
                return Ok(responseModalX);
            }
            else
            {
                device = businessContext.FtDevice.Find(door.DeviceId);

                if (device == null)
                {
                    responseModalX.meta = new MetaModalX
                    {
                        ErrorCode = (int)DoorErrorCode.DOOR_RUQIRED_DEVICE,
                        Success = false,
                        Message = $"{Lang.GeneralUI_Fail} [{Lang.DOOR_RUQIRED_DEVICE}]"
                    };
                    return Ok(responseModalX);
                }
            }

            //檢查重名
            bool chkIsSame = DoorBusiness.ChkValidOfDoorName(door.MaincomId, door.DoorId, door.DoorName, out responseModalX);
            if (chkIsSame)
            {
                return Ok(responseModalX);
            }

            if (door.DoorId==0)  //Addnew 
            {
                int maxId = businessContext.FtDoor.Max(c=>c.DoorId); 
                maxId = maxId == 0 ? 62020: maxId+1;
                  
                FtDoor ftDoor = new FtDoor
                {
                    DoorId = maxId,
                    DoorName = door.DoorName,
                    DeviceId = door.DeviceId,
                    DeviceName = device?.DeviceName??string.Empty,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    MaincomId = door.MaincomId,
                    Status = (int)DoorStatus.DOOR_UNKOWN,
                    SiteId = door.SiteId,
                    SiteName = site?.SiteName ?? string.Empty
                };

                businessContext.FtDoor.Add(ftDoor);
                bool ret = businessContext.SaveChanges() > 0;
                if(ret)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = $"{Lang.GeneralUI_AddNew }{ Lang.GeneralUI_SUCC }" };
                    responseModalX.data = door;
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = Lang.GeneralUI_Fail };
                    responseModalX.data = door;
                    return Ok(responseModalX);
                }
            }else
            {
                var ftDoor = businessContext.FtDoor.Find(door.DoorId);

                if(ftDoor==null)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = $"{ Lang.GeneralUI_NoRecord }{Lang.GeneralUI_Fail }" };
                    responseModalX.data = door;
                    return Ok(responseModalX); 
                }
                
                ftDoor.DoorName = door.DoorName;
                ftDoor.DeviceId = door.DeviceId;

                if(!string.IsNullOrEmpty(device?.DeviceName))
                {
                    ftDoor.DeviceName = device?.DeviceName ;
                }

                if (!string.IsNullOrEmpty(site?.SiteName))
                {
                    ftDoor.SiteName = site?.SiteName;
                }
                 
                ftDoor.SiteId = door.SiteId;
                ftDoor.UpdateTime = DateTime.Now;
                 
                businessContext.FtDoor.Update(ftDoor);
                bool ret = businessContext.SaveChanges() > 0;

                if (ret)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = $"{ Lang.GeneralUI_Update }{ Lang.GeneralUI_SUCC }" };
                    responseModalX.data = door;
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{ Lang.GeneralUI_Update }{Lang.GeneralUI_Fail }" };
                    responseModalX.data = door;
                    return Ok(responseModalX);
                }
            }
        }

        /// <summary>
        /// 新增或更新門的資料
        /// </summary>
        /// <param name="door"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeleteDoor([FromBody]DelDoorInput input)
        {
            string loggerline;
            try
            {
                string doorJson = JsonConvert.SerializeObject(input);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DoorController.DeleteDoor][DelDoorInput JSON]\n{doorJson}";
                Logger.LogWarning(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DoorController.DeleteDoor][DelDoorInput JSON][EXCEPTION]{ex.Message}";
                Logger.LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline);
            }
            

            ResponseModalX responseModalX = new ResponseModalX();

            if (input == null || string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID,
                    Success = false,
                    Message = $"[{Lang.GeneralUI_MainComIdRequired}] "
                };
                return Ok(responseModalX);
            }
            if (input.DoorId==0 || input==null)
            {
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode = (int)GeneralReturnCode.FAIL,
                    Success = false,
                    Message = $"[DOOR ID (SELECT) {Lang.GeneralUI_Required}] "
                };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();

            var ftDoor = businessContext.FtDoor.Find(input.DoorId);
             
            if(ftDoor != null)
            {
                businessContext.FtDoor.Remove(ftDoor);
                bool ret = businessContext.SaveChanges() > 0;

                if (ret)
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = true, Message = $"{ Lang.GeneralUI_Delete } - { Lang.GeneralUI_SUCC }" };
                    responseModalX.data = ftDoor;
                    return Ok(responseModalX);
                }
                else
                {
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{ Lang.GeneralUI_Delete } - {Lang.GeneralUI_Fail }" };
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta = new MetaModalX
                {
                    ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD,
                    Success = false,
                    Message = $"{Lang.GeneralUI_Fail} [{Lang.GeneralUI_NoRecord}]"
                };
                return Ok(responseModalX);
            }
             
        }
    }
}
