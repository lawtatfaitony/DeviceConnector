using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FastConnector
{
    public class GlobalResultFilter : IResultFilter
    {
         
        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("AddHeaderResultFilter:OnResultExecuted");
        }
        /// <summary>
        /// 在结果过滤器中刷新jwt的token
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            //获取当前请求的Token
            string tokenOld = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault().Split(' ')[1];

            var tokens = new JwtSecurityTokenHandler().ReadJwtToken(tokenOld);
            var temp = tokens.Claims;

            var claims = new List<Claim>();
            claims.AddRange(temp.Where(t => t.Type != JwtRegisteredClaimNames.Iat));
            //重置token的发布时间为当前时间
            string time = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, time, ClaimValueTypes.Integer64));

            var now = DateTime.UtcNow;
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: "111",
                audience: "222",
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromDays(1)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("333")), SecurityAlgorithms.HmacSha256)
            );

            string tokenNew = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            //在响应头中返回新的Token
            context.HttpContext.Response.Headers.Add("TokenNew", tokenNew); 
        }
    }

    //And then add the code in STARTUP
    //services.AddMvc(options =>
    //{
    //    options.Filters.Add<GlobalResultFilter>(); //配置过滤器
    //});

}
