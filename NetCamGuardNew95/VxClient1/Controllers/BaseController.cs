using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caching;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using FunctionProtected;
using LanguageResource;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.LibraryApiModel;
using VxGuardClient.Context;
using VxGuardClient.ModelView;

namespace VxGuardClient.Controllers
{
    public partial class BaseController : Controller
    {
        public const string AuthSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme;
         
        public BaseController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            //_logger = LogManager.GetLogger(typeof(BaseController));

            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
              
            WebCookie.httpContextAccessor = httpContextAccessor;
            LangExtend.httpContextAccessor = httpContextAccessor;
             
            _IsFormApi = httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer ") ?? false;
            _HasLanguageInHeader = httpContextAccessor.HttpContext.Request.Headers["Language"].FirstOrDefault()?.StartsWith("Bearer ") ?? false;
            InitializeLanguageCode(httpContextAccessor);

            if(!string.IsNullOrEmpty(WebCookie.MainComId))
            {
                ViewBag.FunctionList = GetFunctionList(WebCookie.MainComId);
            }  
        }
        public ILogger<BaseController> Logger { get; set; }

        public bool IsLoginPage()
        {
            if (httpContextAccessor.HttpContext.GetRouteData().Values["action"].ToString().ToLower() == "login" && httpContextAccessor.HttpContext.GetRouteData().Values["controller"].ToString().ToLower() == "account")
            {
                return true;
            }else
            {
                return false;
            }
        }
         
        
        public static readonly FIFOCache<string, byte[]> cache = RunTimeCache.FIFOCache();
        //--------------------------------------------------------------------------------
        
        private IWebHostEnvironment _webHostEnvironment; 

        public IWebHostEnvironment webHostEnvironment
        {
            get
            {
                return _webHostEnvironment;
            }
            set
            {
                _webHostEnvironment = value;
            }
        }
         
        private IHttpContextAccessor _httpContextAccessor;
        public IHttpContextAccessor httpContextAccessor
        {
            get
            {
                return _httpContextAccessor;
            }
            set
            {
                _httpContextAccessor = value;
            }
        }

        //private ILog _logger;
        //public  ILog logger
        //{
        //    get
        //    {
        //        return _logger;
        //    }
        //    set
        //    {
        //        _logger = value;
        //    }
        //}

        private bool _IsFormApi;
        public bool IsFormApi
        {
            get
            {
                return _IsFormApi;
            }
            set
            {
                _IsFormApi = value;
            }
        }

        private bool _HasLanguageInHeader;
        public bool HasLanguageInHeader
        {
            get
            {
                return _HasLanguageInHeader;
            }
            set
            {
                _HasLanguageInHeader = value;
            }
        }
        public IActionResult SwitchToApiOrView(object obj)
        {
            if(IsFormApi)
            {
                return Ok(obj);
            }else
            {
                return View(obj);
            }
        }
        
        private  string _LanguageCode;
        public  string LanguageCode
        {
            get
            {
                return _LanguageCode;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    _LanguageCode = Thread.CurrentThread.CurrentCulture.Name;
                }
                else
                {
                    _LanguageCode = value;
                }
            }
        }

        private void InitializeLanguageCode(IHttpContextAccessor httpContextAccessor)
        { 
            if (HasLanguageInHeader)
            {
                _LanguageCode = httpContextAccessor.HttpContext.Request.Headers["Language"].FirstOrDefault();
                _LanguageCode = LangUtilities.StandardLanguageCode(_LanguageCode);
                LangExtend.LanguageCode = _LanguageCode;
                ViewBag.LanguageCode = _LanguageCode;
                ViewData["LanguageCode"] = _LanguageCode;
                ViewBag.Language = _LanguageCode;
                ViewData["Language"] = _LanguageCode;
                LangUtilities.LanguageCode = _LanguageCode; 
            }
            else
            {
                _LanguageCode = LangExtend.LanguageCode;
                ViewBag.LanguageCode = _LanguageCode;
                ViewData["LanguageCode"] = _LanguageCode;
                ViewBag.Language = _LanguageCode;
                ViewData["Language"] = _LanguageCode;
                LangUtilities.LanguageCode = _LanguageCode; 
            } 
        }
        
        private FunctionList GetFunctionList(string mainComId)
        {
            FunctionList v2  = FunctionList.FromJson(WebCookie.MainComId);
            return v2;
        }
    }
}