using LanguageResource;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VxGuardClient.Context;

namespace VxGuardClient.Context
{
    /// <summary>
    /// //Create new routeValues based on routeData above  //https://www.coder.work/article/6205155   //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-3.1
    /// ... code removed for brevity
    /// </summary>
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private IUrlHelperFactory _helper;
        private IActionContextAccessor _accessor;
        public CustomCookieAuthenticationEvents(IUrlHelperFactory helper, IActionContextAccessor accessor)
        {
            _helper = helper;
            _accessor = accessor;
        }
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            string language = LangUtilities.StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name);
              
            Uri url = new Uri(System.Uri.UnescapeDataString(context.RedirectUri));

            if (context.Request.RouteValues["Language"] != null)
            {
                language = context.Request.RouteValues["Language"].ToString();
            } 

            string redirectUrl = $"{url.Scheme}://{url.Host}:{url.Port}/{language}{url.PathAndQuery}";
            context.RedirectUri = redirectUrl;
            
            return base.RedirectToLogin(context);
        }
    }
}