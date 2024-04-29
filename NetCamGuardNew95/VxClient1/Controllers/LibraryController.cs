using System;
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
using VideoGuard.ApiModels.LibraryApiModel;
using X.PagedList;
using VxGuardClient.Context;
using VxClient.Models;
using LogUtility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VxGuardClient.Controllers
{ 
    public partial class LibraryController : BaseController
    {
        public LibraryController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement, ILogger<BaseController> logger)
           : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }
        [Authorize]
        [HttpGet]
        public IActionResult AddLibrary()
        { 
            LibraryApiModelInput libraryApiModelInput = new LibraryApiModelInput { MainComId = WebCookie.MainComId,LibraryTypeCode = LibraryTypeCode.LIB_FIX_GROUP };
            return View(libraryApiModelInput); 
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult AddLibrary(LibraryApiModelInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.Name))
            {
                input.Name = string.Empty;
            }else
            {
                if (ChkTheSameLibraryName(0, input.Name, input.MainComId, ref responseModalX))
                {
                    return Ok(responseModalX);
                }
            }
            if (string.IsNullOrEmpty(input.Remark))
            {
                input.Remark = string.Empty;
            }

           
            MetaModalX metaModalX = new MetaModalX();

            MainCom mainCom = new MainCom();
            input.MainComId ??= WebCookie.MainComId ?? mainCom.MainComId; //沒有傳入MainComId,則看看cookie有沒有,沒有則使用默認值

            using (BusinessContext businessContext = new BusinessContext())
            {
                DateTime dt = DateTime.Now;
                int maxId = 1000;
                if (businessContext.FtLibrary.Count() > 0)
                {
                    maxId = businessContext.FtLibrary.Max(c => c.Id) + 1;
                }
                
                FtLibrary ftLibrary = new FtLibrary
                {
                    Id = maxId,
                    MaincomId = input.MainComId,  
                    LibId = 0, //perantsId  暫不以樹狀圖形式顯示,如果太龐大人群時使用
                    Name = input.Name,
                    Remark = input.Remark,
                    PersonCount = 0,
                    Type = Convert.ToSByte(input.LibraryTypeCode),
                    Visible = (int)GeneralVisible.VISIBLE,
                    CreateTime = dt,
                    UpdateTime = dt
                };
                businessContext.FtLibrary.Add(ftLibrary);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = ftLibrary
                    }; 
                    return Ok(responseModalX);
                }
                else
                {
                    metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)LibraryErrorCode.LIB_ADD_FAIL,
                        Success = false,
                        Message = LibraryErrorCode.LIB_ADD_FAIL.GetDescriptionX()
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
        public IActionResult UpdateLibrary(int libId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            LibraryItem libraryItem = GetLibraryDetails(libId);
            if (libraryItem == null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_GET_DETAILS_FAIL, Success = false, Message = Lang.LIB_GET_DETAILS_FAIL },
                    data = null
                };
                return SwitchToApiOrView(responseModalX);
            }
            else
            {
                LibraryTypeCode libraryTypeCode = (LibraryTypeCode)libraryItem.Type;
                LibraryApiModelUpateInput libraryApiModelUpateInput = new LibraryApiModelUpateInput { LibId = libraryItem.LibId, Name = libraryItem.Name, Remark = libraryItem.Remark, LibraryTypeCode = libraryTypeCode, session = WebCookie.ApiSession };
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                    data = libraryApiModelUpateInput
                };
                return SwitchToApiOrView(responseModalX);
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult UpdateLibrary(LibraryApiModelUpateInput input)
        {
            string loggerline;
            try
            { 
                loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::UpdateLibrary][INPUT][{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::UpdateLibrary][PARSE INPUT JSON][EXCEPTION::{ex.Message}]";
                Logger.LogError(loggerline);
            }

            ResponseModalX responseModalX = new ResponseModalX();

            if (string.IsNullOrEmpty(input.Name))
            {
                input.Name = string.Empty;
            }
           

            if (string.IsNullOrEmpty(input.Remark))
            {
                input.Remark = string.Empty;
            }
             
            using (BusinessContext businessContext = new BusinessContext())
            {
                DateTime dt = DateTime.Now;
                FtLibrary ftLibrary = businessContext.FtLibrary.Where(c => c.Id == input.LibId).FirstOrDefault();

                //LIB_UPDATE_FAIL
                if(ftLibrary==null)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_UPDATE_FAIL, Success = false, Message = Lang.LIB_UPDATE_FAIL },
                        data = null
                    };
                    return Ok(responseModalX);
                }
                
                ftLibrary.Name = input.Name;
                ftLibrary.Type = Convert.ToSByte(input.LibraryTypeCode);//Convert.ToSByte();
                ftLibrary.Remark = input.Remark;
                ftLibrary.UpdateTime = dt;

                if (ChkTheSameLibraryName(input.LibId, input.Name, ftLibrary.MaincomId, ref responseModalX))
                {
                    return Ok(responseModalX);
                }
                businessContext.FtLibrary.Update(ftLibrary);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.LIB_UPDATE_SUCCESS, Success = true },
                        data = ftLibrary
                    }; 
                    return Ok(responseModalX);
                }
                else
                { 
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_ADD_FAIL, Success = false, Message = Lang.GeneralUI_Fail },
                        data = null
                    }; 
                    return Ok(responseModalX);
                }
            }
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult DeleteLibrary(LibraryDelete libraryDelete)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MetaModalX metaModalX = new MetaModalX();
            using BusinessContext businessContext = new BusinessContext();
            DateTime dt = DateTime.Now;
            FtLibrary ftLibrary = businessContext.FtLibrary.Where(c => c.Id == libraryDelete.LibId).FirstOrDefault();
            if (ftLibrary != null)
            {
                bool existAnyPerson = businessContext.FtPerson.Any(c => c.LibId == libraryDelete.LibId);
                if(existAnyPerson)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.LIB_EXIST_PERSON_DEL_NOT_ALLOW},
                        data = ftLibrary
                    };
                    return Ok(responseModalX);
                }
                
                ftLibrary.Visible = (sbyte)GeneralVisible.INVISIBLE;
                businessContext.FtLibrary.Update(ftLibrary);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.LIB_DELETE_SUCCESS, Success = true },
                        data = ftLibrary
                    }; 
                    return Ok(responseModalX);
                }
                else
                {
                    metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)LibraryErrorCode.LIB_DELETE_FAIL,
                        Success = false,
                        Message = Lang.LIB_DELETE_FAIL
                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = metaModalX,
                        data = null
                    };
                    OkObjectResult okObjectResult = Ok(responseModalX);
                    return okObjectResult;
                }
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_DELETE_FAIL, Message = string.Format("{0} | {1}", Lang.LIB_DELETE_FAIL, Lang.GeneralUI_NoRecord), Success = false },
                    data = null
                };
                OkObjectResult okObjectResult = Ok(responseModalX);
                return okObjectResult;
            }
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult QueryLibraryList([FromQuery] QueryLibraryList queryLibraryList)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (!string.IsNullOrEmpty(queryLibraryList.Name))
            {
                queryLibraryList.Name = Uri.UnescapeDataString(queryLibraryList.Name).Trim();
            }
            ViewBag.QueryLibraryList = queryLibraryList;

            QueryLibraryListReturn queryLibraryListReturn = new QueryLibraryListReturn();
            List<LibraryItem> items = new List<LibraryItem>();

            string mainComId = WebCookie.MainComId;

            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                {
                    //var ftLibrarys = businessContext.FtLibrary.AsNoTracking();
                    var ftLibrarys = businessContext.FtLibrary.Where(c => c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(mainComId));
                    if (!string.IsNullOrEmpty(queryLibraryList.Name))
                    { 
                        ftLibrarys = ftLibrarys.Where(c => c.Name.Contains(queryLibraryList.Name)); 
                    }
                    if (queryLibraryList.LibraryTypeCode != null)
                    {
                        ViewBag.LibraryTypeCode = queryLibraryList.LibraryTypeCode.ToString();
                        ftLibrarys = ftLibrarys.Where(c => c.Type == (int)queryLibraryList.LibraryTypeCode);
                    }
                    else
                    {
                        ViewBag.LibraryTypeCode = "";
                    }

                    foreach (var item in ftLibrarys)
                    {
                        LibraryItem libraryItem = new LibraryItem
                        {
                            LibId = item.Id,
                            Name = item.Name,
                            Type = (sbyte)item.Type,
                            PersonCount = (sbyte)item.PersonCount,
                            Remark = item.Remark,
                            Visible = (sbyte)item.Visible.GetValueOrDefault(),
                            CreateTime = string.Format("{0:yyyy-MM-ddTHH:mm:ss fff}", item.CreateTime)
                        };
                        items.Add(libraryItem);
                    }
                    var newItems = items.ToPagedList(queryLibraryList.PageNo, queryLibraryList.PageSize);
                    queryLibraryListReturn.PageCount = newItems.PageCount;
                    queryLibraryListReturn.PageNo = newItems.PageNumber;
                    queryLibraryListReturn.PageSize = queryLibraryList.PageSize;
                    queryLibraryListReturn.TotalCount = items.Count();
                    queryLibraryListReturn.Items = newItems.ToList();
                     
                    responseModalX.data = queryLibraryListReturn;
                     
                    return SwitchToApiOrView(responseModalX);
                };
            }
            catch
            {
                MetaModalX metaModalX = new MetaModalX { ErrorCode = (int)CameraErrorCode.QUERY_CAMERA_LIST_FAIL, Success = false, Message = Lang.QUERY_CAMERA_LIST_FAIL };
                responseModalX.meta = metaModalX;
                responseModalX.data = null;
                return SwitchToApiOrView(responseModalX);
            }
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpGet]
        public IActionResult LibraryDetails(int libId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            LibraryItem libraryItem = GetLibraryDetails(libId);
            if (libraryItem == null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_GET_DETAILS_FAIL, Success = false, Message = Lang.LIB_GET_DETAILS_FAIL },
                    data = null
                }; 
                return SwitchToApiOrView(responseModalX);
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                    data = libraryItem
                }; 
                return SwitchToApiOrView(responseModalX);
            }
        }
        private LibraryItem GetLibraryDetails(int libId)
        { 
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtLibrary ftLibrary = businessContext.FtLibrary.Find(libId);

                if (ftLibrary != null)
                {
                    LibraryItem libraryItem = new LibraryItem { LibId = ftLibrary.Id, Name = ftLibrary.Name, Type = (int)ftLibrary.Type, Visible = (int)ftLibrary.Visible, PersonCount = (int)ftLibrary.PersonCount, Remark =ftLibrary.Remark, CreateTime =string.Format("{0:yyyy-MM-dd HH:mm:ss}", ftLibrary.CreateTime)}; 
                    return libraryItem;
                }
                else
                {
                    return null;  
                }
            }
        }

        private bool ChkTheSameLibraryName(int libId,string LibraryName,string maincomId, ref ResponseModalX responseModalX)
        {
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtLibrary ftLibrary = new FtLibrary(); 

                if(libId==0)
                    ftLibrary = businessContext.FtLibrary.Where(c => c.Name == LibraryName && c.MaincomId.Contains(maincomId)).FirstOrDefault();
                else
                    ftLibrary = businessContext.FtLibrary.Where(c =>c.Id != libId && c.Name == LibraryName && c.MaincomId.Contains(maincomId)).FirstOrDefault();

                if (ftLibrary != null)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)LibraryErrorCode.LIB_EXIST_THE_SAME_NAME, Success = false, Message = Lang.LIB_EXIST_THE_SAME_NAME },
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
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]/{mainComId}")]
        [HttpGet]
        public IActionResult GetLibraryList(string mainComId)
        {
            Logger.LogInformation($"[FUNC::LibraryController::GetLibraryList({mainComId})]");
            ResponseModalX responseModalX = new ResponseModalX();
            try
            {
                using (BusinessContext businessContext = new BusinessContext())
                { 
                    var ftLibrarys = businessContext.FtLibrary.Where(c => c.Visible == (int)GeneralVisible.VISIBLE && c.MaincomId.Contains(mainComId)).ToList();

                    List<QueryLibraryListSelect> queryLibraryListSelects = new List<QueryLibraryListSelect>();
                    foreach (var item in ftLibrarys)
                    {
                        QueryLibraryListSelect queryLibraryListSelect = new QueryLibraryListSelect { label = item.Name, value = item.Id.ToString() };
                        queryLibraryListSelects.Add(queryLibraryListSelect);
                    }
                    responseModalX.data = queryLibraryListSelects;
                    return Ok(responseModalX);
                }
            }
            catch (Exception ex)
            {
                responseModalX.meta = new MetaModalX
                {
                    Success = false,
                    Message = string.Format("{0}-{1}", Lang.GeneralUI_Fail, ex.Message),
                    ErrorCode = (int)GeneralReturnCode.EXCEPTION
                };
                return Ok(responseModalX);
            }
        }
    }
}