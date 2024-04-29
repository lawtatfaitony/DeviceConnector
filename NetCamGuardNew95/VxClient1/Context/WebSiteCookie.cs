using Common;
using EnumCode;
using LanguageResource;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http; 
using System.Text; 
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.Account;

namespace VxGuardClient.Context
{
    public class WebCookie
    {
        private static ILog _logger;
        public static ILog logger
        {
            get
            {
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }
        private static DateTime dtNow = DateTime.Now;
        
        private static IHttpContextAccessor _httpContextAccessor;
        public static IHttpContextAccessor httpContextAccessor
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
        public WebCookie(IHttpContextAccessor httpContextAccessor)
        {
            _logger = LogManager.GetLogger(typeof(WebCookie));

            _httpContextAccessor = httpContextAccessor;

            //default value
            TimeSpan ts = new TimeSpan(7, 0, 0, 0);
            _Expires = ts.TotalMinutes;
        }
        private static double _Expires;
        /// <summary>
        /// Mminute
        /// </summary>
        public static double Expires
        {
            get
            {
                return _Expires;
            }
            set
            {
                _Expires = value;
            }
        }

        private static HttpResponse Response
        {
            get
            {
                return httpContextAccessor.HttpContext.Response;
            }
        }
        private static HttpRequest Request
        {
            get
            {
                return httpContextAccessor.HttpContext.Request;
            }
        }

        public static string MainComId
        {
            get
            {
                if (Request.Cookies["MainComId"] == null)
                {
                    return string.Empty;
                }else
                {
                    return Request.Cookies["MainComId"].Trim();
                }
            }
            set
            {
                CookieOptions cookieOptions = new CookieOptions { Expires = DateTime.Now.AddYears(1)}; 
                Response.Cookies.Append("MainComId", value, cookieOptions); 
            }
        }
        /// <summary>
        /// Remove Cookie of MainComId
        /// </summary>
        public static void RemoveCookieMainComId()
        {
            Response.Cookies.Delete("MainComId");
        }

        public static string ApiSession
        {
            get
            {
                if (Request.Cookies["ApiSession"] == null)
                {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Request.Cookies["ApiSession"].ToString()))
                {
                    return string.Empty;
                }
                return Request.Cookies["ApiSession"].ToString();
            }
            set
            { 
                DateTime expireDate = dtNow.AddMinutes(_Expires); 
                CookieOptions cookieOptions = new CookieOptions { Expires = expireDate };
                Response.Cookies.Append("ApiSession", value, cookieOptions);
            }
        } 
        public static string AccessToken
        {
            get
            {
                bool chkValidCookie = true;
                if (Request.Cookies["AccessToken"] == null)
                {
                    chkValidCookie = false; 
                }
                else
                {
                    if (string.IsNullOrEmpty(Request.Cookies["AccessToken"].ToString()))
                     {
                        chkValidCookie = false;
                     }
                }
                if(chkValidCookie == false)
                {
                    return string.Empty; 
                }else
                {
                    return Request.Cookies["AccessToken"].Trim(); 
                }
            } 
        }
        public static void RemoveCookieAccessToken()
        {
            Response.Cookies.Delete("AccessToken");
        }
        public static string UserName
        {
            get
            {
                if (Request.Cookies["UserName"] == null)
                {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Request.Cookies["UserName"].ToString()))
                {
                    return string.Empty;
                }
                return Request.Cookies["UserName"].ToString().ToLower();
            }
            set
            { 
                DateTime expireDate = dtNow.AddMinutes(Expires);
                CookieOptions cookieOptions = new CookieOptions { Expires = expireDate };
                Response.Cookies.Append("UserName", value, cookieOptions);
            }
        }
         
        ///<summary>
        /// Get the current client's request cookie [transfer to LangUtilities.LanguageCode]]
        /// If not, get and write to the session
        /// Rule: If the session does not get the cookie, or not, then get the current client's system request language.
        /// </summary>  
        public static string Language
        {
            get
            {
                if (Request.Cookies["Language"] == null)
                {
                    return LangExtend.LanguageCode;
                }
                if (string.IsNullOrEmpty(Request.Cookies["Language"].ToString()))
                {
                    return LangExtend.LanguageCode;
                }
                return Request.Cookies["Language"].ToString(); 
            }
            set
            {  
                DateTime expireDate = dtNow.AddMinutes(Expires);
                CookieOptions cookieOptions = new CookieOptions { Expires = expireDate };
                Response.Cookies.Append("Language", value, cookieOptions);
            }
        }
         
        public static void RemoveCookieApiSession()
        {
            Response.Cookies.Delete("ApiSession");
        }
        public static void RemoveCookieUserName()
        {
            Response.Cookies.Delete("UserName");
        }
        public static void RemoveCookieLanguage()
        {
            Response.Cookies.Delete("Language");
        }
    }
}