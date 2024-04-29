using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace VxGuardClient.Context
{
    public class GlobalResultFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("GlobalResultFilter:OnResultExecuted");
        }
         
        public void OnResultExecuting(ResultExecutingContext context)
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
