using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
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
using X.PagedList;
using VxGuardClient.Context;
using VideoGuard.ApiModels.Task;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using LogUtility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyModel;
using VideoGuard.Business;
using System.Net;

namespace VxGuardClient.Controllers
{
    public class PersonController : BaseController
    {
        private IOptions<UploadSetting> _uploadSetting;
        private string HttpHost;
        public PersonController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<UploadSetting> uploadSetting, ILogger<BaseController> logger)
              : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }
       
        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [ApiExplorerSettings(IgnoreApi = true)] //对外隐藏该接口
        [HttpGet]
        public IActionResult AddPerson()
        {
            ResponseModalX responseModalX = new ResponseModalX();
            PersonModelInput personModelInput = new PersonModelInput {
                MaincomId = WebCookie.MainComId ,
                Sex = (int)Genders.UNKOWN, 
                Category = PersonCategory.UNBLOCKED,
                LibIdGroups = string.Empty,
                LibId=0
            };

            ViewBag.GenderList = GendersDropDownList(1);
            ViewBag.LibId = LibraryDropDownList(personModelInput.MaincomId);

            responseModalX.data = personModelInput;
            return SwitchToApiOrView(responseModalX);
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult AddPerson(PersonModelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            try {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::PersonController.AddPerson][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }catch(Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][FUNC::PersonController.AddPerson][INPUT][EXCEPTION:{ex.Message}][POST][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
                LogHelper.Fatal(loggerLine);
            }
             
            input.MaincomId ??= WebCookie.MainComId ;

            if (string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            //chk start---------------------------------------------------------------
            if (string.IsNullOrEmpty(input.Name))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.PERSON_INVALID_NAME };
                return Ok(responseModalX);
            }
            //chk start---------------------------------------------------------------
            if (!PersonBusiness.ChkValidOfLibId(input, out responseModalX))
            {
                return Ok(responseModalX);
            }

            if(input.Category == PersonCategory.GUEST)
            {
                input.Name += $"{DateTime.Now:MMddHHmm}";
            }
            //chk end-----------------------------------------------------------------
            using BusinessContext businessContext = new BusinessContext();

            DateTime dt = DateTime.Now;
            long maxId = 60000;  //以6萬開始,禁用個位數,個位數遇到contains查詢數組就會不準確
            if (businessContext.FtPerson.Count() > 0)
            {
                maxId = businessContext.FtPerson.Max(c => c.Id) + 1;
            }
            
            //判断OuterId是否存在 其他记录中
            if (!string.IsNullOrEmpty(input.OuterId))
            {
                input.OuterId = input.OuterId.Trim();
                var existPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(input.OuterId) && c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(input.MaincomId)).FirstOrDefault();
                if (existPerson != null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Exist {Lang.Person_OuterId} :{existPerson.Name}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                input.OuterId = maxId.ToString(); ////如果沒有定義則和主鍵id一樣
            }
            //檢測卡號是否被佔用 | 沒有=false, Add new 的case下,personId = 0
            if(PersonBusiness.CheckPersonCardNoOccupied(input.MaincomId,0,input.CardNo,ref responseModalX)==true)
            {
                return Ok(responseModalX);
            }
  
            FtPerson ftPerspon = new FtPerson
            {
                Id = maxId,
                OuterId = input.OuterId , 
                MaincomId = input.MaincomId,
                LibId = input.LibId,
                LibIdGroups = input.LibIdGroups,
                Name = input.Name,
                Sex = (sbyte)input.Sex,
                CardNo = string.IsNullOrEmpty(input.CardNo) ? "" : input.CardNo,
                PassKey = string.IsNullOrEmpty(input.PassKey) ? "" : input.PassKey,
                Phone = string.IsNullOrEmpty(input.Phone) ? "" : input.Phone,
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
                long ftPictureNewId = 1;
                if (businessContext.FtPicture.Count() > 0)
                {
                    ftPictureNewId = businessContext.FtPicture.Max(c => c.Id) + 1;
                }
                FtPicture ftPicture = new FtPicture
                {
                    Id = ftPictureNewId,
                    PersonId = ftPerspon.Id,
                    PicId = ftPictureNewId,
                    Visible = (sbyte)PictureErrorCode.PICTURE_IS_VISIBLE,
                    PicUrl = string.IsNullOrEmpty(input.PicUrl) ? "" : input.PicUrl,
                    PicClientUrl = string.IsNullOrEmpty(input.PicClientUrl) ? "0" : input.PicClientUrl,
                    PicClientUrlBase64 = string.Empty, //--------------------------------------------- need to supplement later
                    CreateTime = dt,
                    UpdateTime = dt
                };

                if (PersonBusiness.GetUrlPathFileNameId(input.PicClientUrl, out long fileNameId))
                {
                    ftPicture.PicClientUrlBase64 = GetPicClientUrlBase64(fileNameId);
                }
                try
                {
                    businessContext.FtPicture.Add(ftPicture);
                    bool resultUpdatePicture = businessContext.SaveChanges() > 0 ? true : false;
                    if (!resultUpdatePicture)
                    {
                        string logline = string.Format("{0} | {1}", Lang.PICTURE_UPDATE_FAIL, JsonConvert.SerializeObject(input));
                        CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
                    }
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = ftPerspon
                    };
                    return Ok(responseModalX);
                }
                catch (Exception ex)
                {
                    string logLine = $"[AddPerson][FtPicture][Add] [EXCEPTION][{ex.Message}]";
                    CommonBase.OperateDateLoger(logLine,CommonBase.LoggerMode.FATAL);
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = ex.Message };
                    return Ok(responseModalX);
                }
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
        /// 從設備添加人員
        /// 例如海康設備導出的人員json列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult AddPersonFromDevice([FromBody]PersonFromDevInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            try
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][INPUT][FUNC::PersonController.PersonFromDevInput][POST JSON][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
            }
            catch (Exception ex)
            {
                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss ff}][FUNC::PersonController.PersonFromDevInput][INPUT][EXCEPTION:{ex.Message}][POST JSON][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerLine);
                LogHelper.Fatal(loggerLine);
            }
          
            //chk start---------------------------------------------------------------
            if (string.IsNullOrEmpty(input.Name))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.PERSON_INVALID_NAME };
                return Ok(responseModalX);
            } 
            
            //chk end-----------------------------------------------------------------
            using BusinessContext businessContext = new BusinessContext();

            //LibId
            var defaultFixLib = businessContext.FtLibrary.Where(c => c.MaincomId.Contains(input.MaincomId) && c.Type == (int)LibraryTypeCode.LIB_FIX_GROUP).FirstOrDefault();

            DateTime dt = DateTime.Now;
            long maxId = 60000;  //以6萬開始,禁用個位數,個位數遇到contains查詢數組就會不準確
            if (businessContext.FtPerson.Count() > 0)
            {
                maxId = businessContext.FtPerson.Max(c => c.Id) + 1;
            }

            //判断OuterId是否存在 其他记录中
            if (!string.IsNullOrEmpty(input.OuterId))
            {
                input.OuterId = input.OuterId.Trim();
                var existPerson = businessContext.FtPerson.Where(c => c.OuterId.Contains(input.OuterId) && c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(input.MaincomId)).FirstOrDefault();
                if (existPerson != null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"Exist {Lang.Person_OuterId} :{existPerson.Name}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                input.OuterId = maxId.ToString(); ////如果沒有定義則和主鍵id一樣
            }
            //檢測卡號是否被佔用 | 沒有=false, Add new 的case下,personId = 0
            if (PersonBusiness.CheckPersonCardNoOccupied(input.MaincomId, 0, input.CardNo, ref responseModalX) == true)
            {
                return Ok(responseModalX);
            }
            //檢測是否存在相同的姓名
            if (!string.IsNullOrEmpty(input.Name))
            {
                input.Name = WebUtility.UrlDecode(input.Name);
                bool chkSameName = PersonBusiness.ChkValidOfPersonName(input.MaincomId,input.Name, out responseModalX);
                if(chkSameName)
                {
                    return Ok(responseModalX);
                }
            }
            else
            {
                input.Name = input.OuterId;  //如果沒有姓名 則使用外部工號替換掉
            }
               

            FtPerson ftPerspon = new FtPerson
            {
                Id = maxId,
                OuterId = input.OuterId,
                MaincomId = input.MaincomId,
                LibId = defaultFixLib.Id,
                LibIdGroups = string.Empty,
                Name = input.Name,
                Sex = (sbyte)Genders.UNKOWN,
                CardNo = string.IsNullOrEmpty(input.CardNo) ? string.Empty : input.CardNo,
                PassKey = string.IsNullOrEmpty(input.PassKey) ? "" : input.PassKey,
                Phone = string.Empty,
                Category = (sbyte)PersonCategory.UNBLOCKED,
                Remark = string.Empty,
                Visible = (sbyte)PersonErrorCode.PERSON_IS_VISIBLE,
                CreateTime = dt,
                UpdateTime = dt
            };
            businessContext.FtPerson.Add(ftPerspon);
            bool result = businessContext.SaveChanges() > 0 ? true : false;
            if (result)
            { 
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.PersonModelInput_SUCC, Success = true },
                    data = ftPerspon
                };
                return Ok(responseModalX); 
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message =Lang.PERSON_ADD_FAIL },
                    data = null
                };
                return Ok(responseModalX);
            }
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult UpdatePerson([FromQuery]long personId)
        { 
            PersonIdModeInput personIdModeInput = new PersonIdModeInput { PersonId = personId };
            ResponseModalX responsePerson  = PersonDetails(personIdModeInput);
            if (responsePerson.meta.Success ==false && responsePerson.meta.ErrorCode == (int)PersonErrorCode.PERSON_NOT_EXIST) //只有不存在才返回错误也，否则无法完善Person Details
            {
                ResponseModalX responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_NOT_EXIST, Message = Lang.PERSON_NOT_EXIST },
                    data = null
                };
                return RedirectToAction("~/Views/Base/ResponseModal.cshtml", responseModalX);

            }else
            {
                Person person = responsePerson.data as Person;
                int sex = person == null ? 0: person.Sex; 
                ViewBag.GenderList = GendersDropDownList(sex);
                if(person.LibId==0)
                {
                    ViewBag.LibId = LibraryDropDownList(person.MaincomId,null);
                }
                else
                {
                    ViewBag.LibId = LibraryDropDownList(person.MaincomId, person.LibId);
                }
                
                if (!string.IsNullOrEmpty(person.PicUrl))
                {
                    person.PicUrl = person.PicUrl.StartsWith("0") ? string.Empty : person.PicUrl;
                }

                if (!string.IsNullOrEmpty(person.PicClientUrl))
                {
                    person.PicClientUrl = person.PicClientUrl.StartsWith("0")?string.Empty:person.PicClientUrl;
                }
               
                EnumBusiness.Genders genders =(EnumBusiness.Genders)person.Sex;
                PersonUpdateInput personUpdateInput = new PersonUpdateInput
                {
                    PersonId = person.PersonId,
                    MaincomId = person.MaincomId??WebCookie.MainComId,
                    OuterId = person.OuterId?? person.PersonId.ToString(),
                    Category = (PersonCategory)person.Category,
                    LibId = person.LibId,
                    LibIdGroups = person.LibIdGroups,
                    CardNo = person.CardNo,
                    PassKey = person.PassKey,
                    Name = person.Name,
                    Phone = person.Phone,
                    PicClientUrl = person.PicClientUrl,
                    PicUrl = person.PicUrl,
                    Remark = person.Remark,
                    Sex = person.Sex,
                    Gender= genders
                };
                ResponseModalX responseModalX = new ResponseModalX();
                responseModalX.data = personUpdateInput;
                return View(responseModalX);
            } 
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult UpdatePerson(PersonUpdateInput input)
        {
            DateTime dt = DateTime.Now;
            ResponseModalX responseModalX = new ResponseModalX();

            bool chkIsSame = PersonBusiness.ChkValidOfPersonName(input.MaincomId, input.Name, out responseModalX);
            if (chkIsSame)
            {
                return Ok(responseModalX);
            }
            bool chkPicture = PersonBusiness.ChkValidOfPersonPicture(input.PersonId, out FtPicture ftPicture, out responseModalX); 
            
            using BusinessContext businessContext = new BusinessContext();

            FtPerson ftPerson = businessContext.FtPerson.Find(input.PersonId);

            //判断OuterId是否存在 其他记录中
            if (!string.IsNullOrEmpty(input.OuterId))
            {
                input.OuterId = input.OuterId.Trim();
                var existPerson = businessContext.FtPerson.Where(c => c.OuterId== input.OuterId && c.Id != input.PersonId && c.Visible == (int)GeneralVisible.VISIBLE).FirstOrDefault();
                if (existPerson != null)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_EXIST_OUTTER_ID, Message = $"{Lang.PERSON_EXIST_OUTTER_ID}:{existPerson.Name} {existPerson.Id}" };
                    return Ok(responseModalX);
                }
            }
            else
            {
                input.OuterId = ftPerson.Id.ToString();
            }

            if (input.LibIdGroups == "0")
                input.LibIdGroups = string.Empty;

            if (ftPerson != null)
            {
                ftPerson.OuterId = input.OuterId;
                ftPerson.LibId = input.LibId;
                ftPerson.LibIdGroups = input.LibIdGroups;
                ftPerson.Name = input.Name;
                ftPerson.Sex = (sbyte)input.Gender;
                ftPerson.CardNo = input.CardNo ?? string.Empty;
                ftPerson.PassKey = input.PassKey ?? string.Empty;
                ftPerson.Phone = input.Phone ?? string.Empty;
                ftPerson.Category = (sbyte)input.Category;
                ftPerson.Remark = input.Remark ?? string.Empty;

                //CardNo  Is occupied
                if (PersonBusiness.CheckPersonCardNoOccupied(input.MaincomId, ftPerson.Id, input.CardNo, ref responseModalX))
                {
                    return Ok(responseModalX);
                }

                businessContext.FtPerson.Update(ftPerson);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    //如果是BLOCKED/INVISIBLE 则要更新 对应的设备中已经下行的用户
                    //删除下行到设备的有关此person的所有数据 并更状态= 等待删除设备用户的操作(作业任务等待)
                    string devperosnTips = "";
                    if (ftPerson.Category==(int)PersonCategory.BLOCKED)
                    {
                        bool devPersonResult = DevManageBusiness.DevicePersonChangeDeleteToWait(ftPerson.Id, out int qtyOfDevice);
                        if (devPersonResult)
                            devperosnTips = $"{Lang.DeletePerson_DevicePersonDelWait_Tips} DEV QTY={qtyOfDevice}";
                    } 

                    long fileNameId;
                    if (ftPicture != null && !string.IsNullOrEmpty(input.PicUrl) && !string.IsNullOrEmpty(input.PicClientUrl))
                    {
                        ftPicture.PicUrl = input.PicUrl;
                        ftPicture.PicClientUrl = input.PicClientUrl;
                        ftPicture.PicClientUrlBase64 = string.Empty; //--------------------------------------------- need to supplement later
                        ftPicture.Visible = (sbyte)PictureErrorCode.PICTURE_IS_VISIBLE;
                        ftPicture.UpdateTime = dt;

                        if (PersonBusiness.GetUrlPathFileNameId(input.PicClientUrl, out fileNameId))
                        {
                            ftPicture.PicClientUrlBase64 = GetPicClientUrlBase64(fileNameId);
                        }
                        businessContext.FtPicture.Update(ftPicture);
                    }
                    else if (!string.IsNullOrEmpty(input.PicUrl) && !string.IsNullOrEmpty(input.PicClientUrl))
                    {
                        ftPicture = new FtPicture();
                        ftPicture.Id = businessContext.FtPicture.Max(c => c.Id) + 1;
                        ftPicture.PicUrl = input.PicUrl;
                        ftPicture.PicClientUrl = input.PicClientUrl;
                        ftPicture.PicClientUrlBase64 = string.Empty;  //--------------------------------------------- need to supplement later
                        ftPicture.Visible = (sbyte)GeneralVisible.VISIBLE;
                        ftPicture.PicId = ftPicture.Id;
                        ftPicture.UpdateTime = dt;
                        ftPicture.CreateTime = dt;

                        if (PersonBusiness.GetUrlPathFileNameId(input.PicClientUrl, out fileNameId))
                        {
                            ftPicture.PicClientUrlBase64 = GetPicClientUrlBase64(fileNameId);
                        }
                        businessContext.FtPicture.Add(ftPicture);
                    }
                    bool resultPicUpd = businessContext.SaveChanges() > 0;

                    if (resultPicUpd)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.AddPerson_Update}{Lang.GeneralUI_SUCC}<br>{devperosnTips}", Success = true },
                            data = ftPerson
                        };
                        return Ok(responseModalX);
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.AddPerson_Update}{Lang.GeneralUI_SUCC}<br>{Lang.PICTURE_UPDATE_FAIL}<br>{devperosnTips}" },
                            data = null
                        };
                        return Ok(responseModalX);
                    }
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_UPDATE_FAIL, Success = false, Message = Lang.PERSON_UPDATE_FAIL },
                        data = null
                    };
                    return Ok(responseModalX);
                }
            }
            else
            {
                responseModalX.meta.Success = false;
                responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                responseModalX.data = null;
                return Ok(responseModalX);
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public ResponseModalX PersonDetails(PersonIdModeInput personIdInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();
            //ChkValidOfLibIdExist--------------------------------------------------------
            bool lChkValidOfPersonExist = PersonBusiness.ChkValidOfPersonExist(personIdInput.PersonId, out FtPerson ftPerson, out responseModalX);
            if (!lChkValidOfPersonExist) //不存在人员记录的情况下 必须返回
            {
                return responseModalX;
            }
            //ChkValidOfLibIdExist--------------------------------------------------------
            bool lChkValidOfLibIdExist = PersonBusiness.ChkValidOfLibIdExist(ftPerson.LibId, out FtLibrary ftLibrary, out responseModalX);
            
            //ChkValidOfPersonPicture--------------------------------------------------------
            bool lChkValidOfPersonPicture = PersonBusiness.ChkValidOfPersonPicture(ftPerson.Id, out FtPicture ftPicture, out responseModalX);
             
            Person person = new Person
            {
                MaincomId = ftPerson.MaincomId,
                LibId = ftLibrary == null ?0: ftLibrary.LibId,
                LibIdGroups = ftPerson.LibIdGroups == "0" ? string.Empty : ftPerson.LibIdGroups,
                LibName = ftLibrary?.Name??String.Empty,
                PersonId = ftPerson.Id,
                OuterId = ftPerson.OuterId ?? ftPerson.Id.ToString(),
                Name = ftPerson.Name,
                Sex = (int)ftPerson.Sex,
                CardNo = ftPerson.CardNo,
                PassKey = ftPerson.PassKey,
                Phone = ftPerson.Phone,
                Category = ftPerson.Category,
                PicUrl = ftPicture?.PicUrl??String.Empty,
                PicClientUrl = ftPicture?.PicClientUrl ?? String.Empty,   //$"{HttpHost}/{ftPicture.PicClientUrl.TrimStart('/').TrimStart('/')}",
                Remark = ftPerson.Remark,
                CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftPerson.CreateTime)
            };
            responseModalX = new ResponseModalX
            {
                meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC },
                data = person
            };
            return responseModalX;
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public ResponseModalX GetPersonByEmployeeNoOrId([FromBody]EmployeeNoOrIdInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();
            //ChkValidOfLibIdExist--------------------------------------------------------
            bool lChkValidOfPersonExist = PersonBusiness.ChkValidOfPersonExist(input.EmployeeId, out FtPerson ftPerson, out responseModalX);
            if (!lChkValidOfPersonExist)
            {
                return responseModalX;
            }
            //ChkValidOfLibIdExist--------------------------------------------------------
            bool lChkValidOfLibIdExist = PersonBusiness.ChkValidOfLibIdExist(ftPerson.LibId, out FtLibrary ftLibrary, out responseModalX);
            if (!lChkValidOfLibIdExist)
            {
                return responseModalX;
            }
            //ChkValidOfPersonPicture--------------------------------------------------------
            bool lChkValidOfPersonPicture = PersonBusiness.ChkValidOfPersonPicture(ftPerson.Id, out FtPicture ftPicture, out responseModalX);
            if (!lChkValidOfPersonPicture)
            {
                return responseModalX;
            }
            List<LibraryItemX> libraryItemXs = new List<LibraryItemX>();
            Person person = new Person
            {
                MaincomId = ftPerson.MaincomId,
                LibId = ftPerson.LibId,
                LibIdGroups = ftPerson.LibIdGroups == "0" ? string.Empty : ftPerson.LibIdGroups,
                LibName = ftLibrary.Name,
                LibIdGroupsList = string.IsNullOrEmpty(ftPerson.LibIdGroups)? libraryItemXs : PersonBusiness.GetLibIdGroupsList(ftPerson.LibIdGroups).Select(s=> new LibraryItemX{ Id =s.Id , LibId = s.LibId, Name =s.Name }).ToList(),
                PersonId = ftPerson.Id,
                OuterId = ftPerson.OuterId ?? ftPerson.Id.ToString(),
                Name = ftPerson.Name,
                Sex = (int)ftPerson.Sex,
                CardNo = ftPerson.CardNo,
                PassKey = ftPerson.PassKey,
                Phone = ftPerson.Phone,
                Category = ftPerson.Category,
                PicUrl = ftPicture.PicUrl,
                PicClientUrl = ftPicture.PicClientUrl,
                Remark = ftPerson.Remark,
                CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftPerson.CreateTime)
            };
            responseModalX = new ResponseModalX
            {
                meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_SUCC}[{person.Name}]" },
                data = person
            };
            return responseModalX;
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult DeletePerson(PersonDeleteModeInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            using BusinessContext businessContext = new BusinessContext();
            FtPerson ftPerson = businessContext.FtPerson.Find(input.PersonId);

            //只有被BLOCKED和訪客才會被直接刪除
            if(ftPerson.Category == (int)PersonCategory.GUEST || ftPerson.Category == (int)PersonCategory.BLOCKED)
            { 
                using var transaction = businessContext.Database.BeginTransaction();

                try
                {
                    businessContext.FtPerson.Remove(ftPerson);
                    businessContext.SaveChanges();
                   
                    var pictures = businessContext.FtPicture.Where(c=>c.PersonId == ftPerson.Id);
                    businessContext.FtPicture.RemoveRange(pictures);
                      
                    transaction.Commit();

                    //删除下行到设备的有关此person的所有数据 并更状态= 等待删除设备用户的操作(作业任务等待)
                    bool devPersonResult = DevManageBusiness.DevicePersonChangeDeleteToWait(input.PersonId, out int qtyOfDevice);
                    string devperosnTips = "";
                    if (devPersonResult)
                        devperosnTips = $"{Lang.DeletePerson_DevicePersonDelWait_Tips} DEV QTY={qtyOfDevice}";

                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_DELTET_SUCCESS, Success = false, Message = $"{Lang.PERSON_DELTET_SUCCESS} INDICATE DELETE DEVICE PERSON :{devPersonResult}" },
                        data = null
                    };
                    return Ok(responseModalX); 
                }
                catch (Exception ex)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_DELETE_FAIL, Success = false, Message = $"{Lang.PERSON_DELETE_FAIL}({ex.Message})"},
                        data = null
                    };
                    return Ok(responseModalX);
                }
            }
            else
            {
                //一般情況是邏輯刪除
                if (ftPerson != null)
                {
                    ftPerson.Visible = (sbyte)PersonErrorCode.PERSON_NOT_VISIBLE;
                    businessContext.FtPerson.Update(ftPerson);
                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    {
                        //删除下行到设备的有关此person的所有数据 并更状态= 等待删除设备用户的操作(作业任务等待)
                        bool devPersonResult = DevManageBusiness.DevicePersonChangeDeleteToWait(ftPerson.Id, out int qtyOfDevice);
                        string devperosnTips = "";
                        if (devPersonResult)
                            devperosnTips = $"{Lang.DeletePerson_DevicePersonDelWait_Tips} DEV QTY={qtyOfDevice}";
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_DELTET_SUCCESS, Message = $"{Lang.PERSON_DELTET_SUCCESS} <br>{devperosnTips}", Success = true },
                            data = ftPerson
                        };
                        return Ok(responseModalX);
                    }
                    else
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_DELETE_FAIL, Success = false, Message = Lang.GeneralUI_Fail },
                            data = null
                        };
                        return Ok(responseModalX);
                    }
                }
                else
                {
                    responseModalX.meta.Success = false;
                    responseModalX.meta.ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD;
                    responseModalX.meta.Message = Lang.GeneralUI_NoRecord;
                    responseModalX.data = null;
                    return Ok(responseModalX);
                }
            } 
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult QueryPersonList(QueryPersonListInput input)
        {
            string loggerline;
            try
            {
                string attendancePostLog = JsonConvert.SerializeObject(input);
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::PersonController.QueryPersonList][INPUT]\n{attendancePostLog}";
                Logger.LogWarning(attendancePostLog);
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::PersonController.QueryPersonList][INPUT][EXCEPTION]\n{ex.Message}";
                Logger.LogError(loggerline);
                CommonBase.OperateDateLoger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][{loggerline}]");
            }
            ResponseModalX responseModalX = new ResponseModalX();
            if (!string.IsNullOrEmpty(input.Name))
            {
                input.Name = Uri.UnescapeDataString(input.Name).Trim();
            }

            input.MaincomId = input.MaincomId??WebCookie.MainComId ?? string.Empty;
            if (string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX.meta = new MetaModalX { Success=false,ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID,Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }
            
            ViewBag.QueryPersonListInput = input;
             
            responseModalX = PersonBusiness.GetPersonList(input);

            if(responseModalX.meta.Success==false && responseModalX.meta.ErrorCode == (int)GeneralReturnCode.LIST_NO_RECORD) //无记录情况下才返回
            {
                return SwitchToApiOrView(responseModalX);
            }
             
            if (input.RequiredPic)
            { 
                ViewBag.IsReqUiredPic = "checked";
            }

            ViewBag.QueryPersonListInput = input;
            List<Person> personLists = responseModalX.data as List<Person>;
            try
            { 
                var newItems = personLists.ToPagedList(input.PageNo, input.PageSize);

                QueryPersonListInfoReturn queryPersonListInfoReturn = new QueryPersonListInfoReturn();
                queryPersonListInfoReturn.PageCount = newItems.PageCount;
                queryPersonListInfoReturn.PageNo = newItems.PageNumber;
                queryPersonListInfoReturn.PageSize = newItems.PageSize;
                queryPersonListInfoReturn.TotalCount = personLists.Count();
                queryPersonListInfoReturn.Items = newItems.ToList();
                responseModalX = new ResponseModalX();
                responseModalX.data = queryPersonListInfoReturn;
                return SwitchToApiOrView(responseModalX);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"[QueryPersonList :: {Lang.PERSON_LIST_FAIL}] [ErrorCode = {PersonErrorCode.PERSON_LIST_FAIL}] [Exception][{ex.Message}][line 76678uidm]");
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)PersonErrorCode.PERSON_LIST_FAIL, Success = false, Message = $"{Lang.PERSON_LIST_FAIL} [Exception][{ex.Message}]" };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
        }
         
        private bool ChkValidPersonPageNo(int pageNo, out ResponseModalX responseModalX)
        {
            if (pageNo == 0)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_PAGE_NO_ERR, Message = Lang.GeneralUI_PAGE_NO_ERR },
                    data = null
                };

                return false;
            }
            else
            {
                responseModalX = new ResponseModalX();
                return true;
            }
        }

        private string GetPicClientUrlBase64(long fileNameId)
        {
            string uploadFolder = _uploadSetting.Value.TargetFolder;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(fileNameId);
            string monthFolder = string.Format("{0:yyyyMM}", dateTimeOffset.UtcDateTime);
            string targetPath = Path.Combine(webHostEnvironment.ContentRootPath, uploadFolder,"Person", monthFolder);

            string FileName = string.Format("{0}.jpg", fileNameId);
            string pahtFileName = Path.Combine(targetPath, FileName);

            if (System.IO.File.Exists(pahtFileName))
            {
                using FileStream fsForRead = new FileStream(pahtFileName, FileMode.Open);
                string picClientUrlBase64 = string.Empty;
                try
                {
                    fsForRead.Seek(0, SeekOrigin.Begin);
                    byte[] bs = new byte[fsForRead.Length];
                    int log = Convert.ToInt32(fsForRead.Length);
                    fsForRead.Read(bs, 0, log);
                    picClientUrlBase64 = Convert.ToBase64String(bs);
                    return picClientUrlBase64;
                }
                catch (Exception ex)
                {
                    string logLine = $"[CONVERT TO BASE64] [FUN][GetPicClientUrlBase64][long::ID={fileNameId}] [EXCEPTION:{ex.Message}]";
                    LogHelper.Error(logLine);
                    return string.Empty;
                }
                finally
                {
                    fsForRead.Flush();
                    fsForRead.Close();
                }
            }
            else
            {
                return string.Empty;
            }
        }
         
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public SelectList GendersDropDownList(int Gender = 2)
        {
            SelectListItem item1 = new SelectListItem { Text = Lang.Genders_MALE, Value = "0" };
            SelectListItem item2 = new SelectListItem { Text = Lang.Genders_FEMALE, Value = "1" };
            SelectListItem item3 = new SelectListItem { Text = Lang.Genders_UNKNOWN, Value = "2" };
            switch (Gender)
            {
                case 0:
                    item1.Selected = true;
                    break;
                case 1:
                    item2.Selected = true;
                    break;
                default:
                    item3.Selected = true;
                    break;
            }
            List<SelectListItem> selectListItems = new List<SelectListItem> { item1, item2, item3 };

            var GetSelectList = new SelectList(selectListItems, "Value", "Text", selectListItems);
            return GetSelectList;
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public SelectList LibraryDropDownList(string MainComId,object objValue = null)
        { 
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            using BusinessContext businessContext = new BusinessContext();
              
            var libLists = businessContext.FtLibrary.Where(c=>c.MaincomId.Contains(MainComId) && c.Visible == (int)GeneralVisible.VISIBLE).ToList();

            int selValue = 0;
            if (objValue != null)
                selValue = (int)objValue;
            foreach (var item in libLists)
            {
                SelectListItem listItem = new SelectListItem { Text = item.Name, Value = item.Id.ToString(),Selected = item.Id == selValue };
                selectListItems.Add(listItem);
            }
            var GetSelectList = new SelectList(selectListItems, "Value", "Text", objValue);
            return GetSelectList;
        }
    }
}
