using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ubiety.Dns.Core;

namespace VxGuardClient.Context
{
    /// <summary>
    /// https://www.cnblogs.com/TianFang/p/12869800.html
    /// </summary>
    public class MyAuthHandler : IAuthenticationHandler
    {
        public const string SchemeName = "MyAuthScheme"; 
        AuthenticationScheme _scheme;
        HttpContext _context;
        bool _IsFormApi = false;
        bool _ExistsAccessTokenCookie = false;
        string _UserName = string.Empty;
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _IsFormApi = context.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer ") ?? false;
            _scheme = scheme;
            _context = context;
          
            if(context.Request.Cookies["AccessToken"]!=null)
            {
                if(context.Request.Cookies["AccessToken"].Length>0)
                {
                    _ExistsAccessTokenCookie = true;
                }
            }
            if (context.Request.Cookies["UserName"] != null)
            {
                if (context.Request.Cookies["UserName"].Length > 0)
                {
                    _UserName = context.Request.Cookies["UserName"];
                }
            }
            return Task.CompletedTask;
        }
         
        public Task<AuthenticateResult> AuthenticateAsync()
        {
            if (_context.Request.Cookies["AccessToken"] != null&& _IsFormApi == false)
            { 
                string ReturnUrl = _context.Request.GetEncodedUrl();
                string LanguageCode = LangExtend.LanguageCode;
                  
                 
                string resultUrl = string.Format("/{0}/Account/login?ReturnUrl={1}", LanguageCode, ReturnUrl);

                //RedirectToActionResult content = new RedirectToActionResult("login", "Account", new { returnUrl = resultUrl });
                
                if (!_ExistsAccessTokenCookie)
                {
                    return Task.FromResult(AuthenticateResult.Fail("not to login"));
                }
                else
                {
                    var ticket = GetAuthTicket(_UserName, "Admin");
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                } 
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("not to login"));
            }
        }

        AuthenticationTicket GetAuthTicket(string name, string role)
        {
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
            }, SchemeName);

            var principal = new ClaimsPrincipal(claimsIdentity);
            return new AuthenticationTicket(principal, _scheme.Name);
        }

        /// <summary>
        /// permission handle
        /// </summary>
        public Task ForbidAsync(AuthenticationProperties properties)
        {
            _context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }

        /// <summary>
        /// not to login, handle
        /// </summary>
        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            _context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }
        //ms
        public Func<RedirectContext<CookieAuthenticationOptions>, Task> OnRedirectToReturnUrl { get; set; } = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.Headers["Location"] = context.RedirectUri;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }
    }


    ////-----------------------------------------------------------------------------------------https://blog.csdn.net/ssjdoudou/article/details/107871053
    ///// <summary>
    ///// IExceptionFilter 异常拦截
    ///// ActionFilterAttribute 请求拦截器
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    //public class BasicAuthAttribute : ActionFilterAttribute
    //{
    //    /// <summary>
    //    /// 在控制器执行之前调用
    //    /// </summary>
    //    /// <param name="context">执行的上下文</param>
    //    public override void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        // 判断是否加上了不需要拦截
    //        string authHeader = context.HttpContext.Request.Headers["Authorization"];
    //        if (authHeader != null && authHeader.StartsWith("Basic"))
    //        {
    //            //Extract credentials
    //            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
    //            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
    //            string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
    //            int seperatorIndex = usernamePassword.IndexOf(':');
    //            var username = usernamePassword.Substring(0, seperatorIndex);
    //            var password = usernamePassword.Substring(seperatorIndex + 1);
                 
    //            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_]+$"))
    //            { 
    //                context.Result = new UnauthorizedResult();
    //                return;
    //            }

    //            //if (IsAuthorized(username, password))
    //            if (ValidateDomainUser(username, password))
    //            { 
    //                return;

    //            }
    //            else
    //            {
    //                Logger.Log.ErrorFormat("Unauthorized, user [] ", username);
    //                context.Result = new UnauthorizedResult();
    //                return;
    //            }
    //        }
    //        else
    //        {
    //            Logger.Log.ErrorFormat("No Basic Auth Info");
    //            context.Result = new UnauthorizedResult();
    //        }

    //    }

    //    public bool ValidateUser(string accountName, string password)
    //    {
    //        //验证用户名密码
    //    }
    //}

    ///// <summary>
    ///// 不需要登陆的地方加个这个空的拦截器
    ///// </summary>
    //public class NoSignAttribute : ActionFilterAttribute { }
}
