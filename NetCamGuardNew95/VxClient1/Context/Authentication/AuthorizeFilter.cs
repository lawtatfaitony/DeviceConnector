using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks; 
using VxGuardClient.Context;

namespace VxGuardClient.Context
{
    //public class AuthorizeFilter : IAsyncAuthorizationFilter, IFilterFactory https://www.cnblogs.com/TianFang/p/12869800.html  //https://github.com/dotnet/aspnetcore/blob/bd65275148abc9b07a3b59797a88d485341152bf/src/Mvc/Mvc.Core/src/Authorization/AuthorizeFilter.cs#L236
    public class AuthFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool _IsFormApi = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer ") ?? false;
            if (context.HttpContext.User.Identity.IsAuthenticated == false)  
            {
                //未通过验证则跳转 
                RedirectToActionResult content = new RedirectToActionResult("login", "Account", new { returnUrl = context.RouteData.Values["returnUrl"] });  //http://localhost:64842/en-us/Exception/NoAuth
                context.Result = content;
            }
        }
    } 
}