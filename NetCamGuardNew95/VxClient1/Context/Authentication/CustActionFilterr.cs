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
using System.Diagnostics;
using System.Reflection;
namespace VxGuardClient.Context
{
    //https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-3.1  filter
    public class MySampleActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Do something before the action executes.
            MyDebug.Write(MethodBase.GetCurrentMethod(), context.HttpContext.Request.Path);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
            MyDebug.Write(MethodBase.GetCurrentMethod(), context.HttpContext.Request.Path);
        }
    }
    public class SampleAsyncActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Do something before the action executes.

            // next() calls the action method.
            var resultContext = await next();
            // resultContext.Result is set.
            // Do something after the action executes.
        }
    }
    public static class MyDebug
    {
        public static void Write(MethodBase m, string path)
        {
            Debug.WriteLine(m.ReflectedType.Name + "." + m.Name + " " +
                path);
        }
    }
    public class CustActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                //未通过验证则跳转到无权限提示页
                RedirectToActionResult content = new RedirectToActionResult("login", "Account", new { returnUrl = context.RouteData.Values["returnUrl"] });  //http://localhost:64842/en-us/Exception/NoAuth
                context.Result = content;
            }
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {

            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                //未通过验证则跳转到无权限提示页
                RedirectToActionResult content = new RedirectToActionResult("login", "Account", new { returnUrl = context.RouteData.Values["returnUrl"] });  //http://localhost:64842/en-us/Exception/NoAuth
                context.Result = content;
            }
        }
    }
}



 