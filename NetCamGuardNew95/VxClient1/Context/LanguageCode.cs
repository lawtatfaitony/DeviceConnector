using LanguageResource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing; 
using System.Threading; 

namespace VxGuardClient.Context
{
    public class LangExtend
    {
        public LangExtend(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
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
        private static string _language;
        public static string LanguageCode
        {
            get
            {
                try
                { 
                    //var routeData = httpContextAccessor.HttpContext.GetRouteData();
                    if(httpContextAccessor.HttpContext.Request.RouteValues["Language"]!=null)
                    {
                        _language = httpContextAccessor.HttpContext.Request.RouteValues["Language"].ToString();
                    }
                     
                    if (!string.IsNullOrEmpty(_language))
                    {
                        _language = LangUtilities.StandardLanguageCode(_language);
                        return _language;
                    }
                    else
                    {
                        _language = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name); // Templary to use this method.
                        return _language;
                    }
                }
                catch
                {
                    _language = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name);
                    return _language;
                }
            }
            set
            {
                _language = LangUtilities.StandardLanguageCode(value);
            }
        }
    }
}