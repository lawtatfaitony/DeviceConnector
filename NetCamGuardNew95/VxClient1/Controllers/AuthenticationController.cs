using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnumCode;
using LanguageResource;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoGuard.ApiModels;
using LogUtility;
namespace VxGuardClient.Controllers
{ 
    public partial class AuthenticationController : Controller
    {
        private IAuthenticateService authService; 
        private TokenManagement tokenManagement;
        public AuthenticationController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenM)
        {
            authService = service;
            tokenManagement = tokenM.Value;
        }
        /// <summary>
        /// Get Access Token
        /// </summary>
        /// <param name="request">Password is MD5 Format like "admin123 = 0192023a7bbd73250516f069df18b500 "</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("[controller]/[action]")]
        [HttpPost]
        public IActionResult RequestToken([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request ");
            }
            string token;                      
            var authTime = DateTime.UtcNow;  
            if (authService.IsAuthenticated(request, out token))
            {
                RequestToken requestToken = new RequestToken
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    AuthProfile = new AuthProfile
                    {
                        UserName = request.UserName,
                        ApiSession = authService.GetPasswordMD5(request.Password),
                        AuthTime = new DateTimeOffset(authTime).ToUnixTimeMilliseconds(),
                        AccessExpiration = new DateTimeOffset(authTime.AddMinutes(tokenManagement.AccessExpiration)).ToUnixTimeMilliseconds(),
                        TokenExpiredTimeOut = new DateTimeOffset(authTime.AddMinutes(tokenManagement.RefreshExpiration)).ToUnixTimeMilliseconds()
                    }
                };
                MetaModalX metaModalX = new MetaModalX();
                ResponseModalX responseModalX = new ResponseModalX
                {
                    meta = metaModalX,
                    data = requestToken
                }; 
                return Ok(responseModalX);
            }
            else
            {
                MetaModalX metaModalX = new MetaModalX { 
                     ErrorCode = (int)AuthorizationErrorCode.LONGIN_FAIL,
                     Success = false,
                     Message = string.Format("{0} NOTE:DO NOT INPUT TEXT PASSWORD ", Lang.GeneralUI_LoginFail)
                };
                ResponseModalX responseModalX = new ResponseModalX
                {
                    meta = metaModalX,
                    data = null
                };
                 
                return Ok(responseModalX);
            }
        }

        [AllowAnonymous]
        [Route("[controller]/[action]")]
        [HttpPost]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var principal = authService.GetPrincipalFromAccessToken(model.AccessToken);
            var userName = principal.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            //_authService
            bool CreateAccessTokenRlt = authService.TryCreateAccessToken(userName, out string  accessToken);
            var refreshToken = authService.GenerateRefreshToken();
            return Json(new { accessToken = accessToken, refreshToken = refreshToken }); 
        }
    }
}