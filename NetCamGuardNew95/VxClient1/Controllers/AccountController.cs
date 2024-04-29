using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.Account;
using VxGuardClient.Context;
using VxGuardClient.ModelView;
using LogUtility;
namespace VxGuardClient.Controllers
{ 
    public partial class AccountController : BaseController
    {
        private TokenManagement tokenManagement1 { get; set; }
        public AccountController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement)
          : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            tokenManagement1 = tokenManagement.Value;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        public ActionResult SignOut()
        {
            WebCookie.RemoveCookieAccessToken();
            WebCookie.RemoveCookieApiSession();
            WebCookie.RemoveCookieUserName();
            HttpContext.SignOutAsync("AccessToken");
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult AccessDenied()
        {
            return View();
        }
        //-------------------------------------------------------------------------------------------------------- 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login([FromQuery] string ReturnUrl)
        { 
            ResponseModalX responseModalX = new ResponseModalX();
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                ReturnUrl = new PathString(HttpUtility.UrlDecode(ReturnUrl));
            }
            ViewData["Language"] = LanguageCode;
            //----------------------------------------------------------
            if (!string.IsNullOrEmpty(ReturnUrl))
                ViewData["ReturnUrl"] = ReturnUrl;
            else
                ViewData["ReturnUrl"] = $"/{LanguageCode}/Home/Index";
            //----------------------------------------------------------
            Login login = new Login
            {
                UserName = string.Empty,
                Password = string.Empty
            }; 
            responseModalX.data = login; 
            return SwitchToApiOrView(responseModalX);
        }
         
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromForm] Login login, [FromQuery] string ReturnUrl)
        {
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                ReturnUrl = new PathString(HttpUtility.UrlDecode(ReturnUrl));
            }
            using (BusinessContext businessContext = new BusinessContext())
            {
                if(login.Password.Length != 32 && !CommonBase.IsMd5LowerCase(login.Password)) //For App 基於 DGX 系統 明文密碼登錄的情況
                {
                    login.Password = CommonBase.MD5Encrypt(login.Password);
                }
                var ftUser = businessContext.FtUser.Where(c => c.Name == login.UserName && c.Password == login.Password).FirstOrDefault();
                if (ftUser != null)
                {
                    //判斷MainComId
                    string maincomId = ftUser.MaincomId;
                    if (maincomId == "0")
                        maincomId = string.Empty;

                    if (!string.IsNullOrEmpty(ftUser.MaincomId))
                    {
                        WebCookie.MainComId = ftUser.MaincomId;
                    }
                    
                     ResponseModalX responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)LoginErrorCode.SUCCESS, Success = true, Message = Lang.GeneralUI_LoginSucc },
                        data = new { session = ftUser.Password, returnUrl = ReturnUrl,userName = ftUser.Name, userId=ftUser.Id,apisession = ftUser.Password}
                    };
                    TimeSpan timeSpan = new TimeSpan(7, 0, 0, 0); // 7days
                    long expires = (long)timeSpan.TotalMilliseconds;
                    LoginSuccessCookieModel loginSuccessCookieModel = new LoginSuccessCookieModel { Expires = expires, UserName = login.UserName, ApiSession = login.Password };
                    SwitchToLoginStatus(loginSuccessCookieModel);
                    return Ok(responseModalX);
                }
                else
                {
                    ResponseModalX responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = string.Format("{0} NOTE:DO NOT INPUT TEXT PASSWORD ", Lang.GeneralUI_LoginFail) },
                        data = string.Empty
                    };
                    return Ok(responseModalX);
                }
            }
        }
        private void SwitchToLoginStatus(LoginSuccessCookieModel loginSuccessCookieModel)
        {
            //System Cookie Info-------------------------------------------------------------------------------------------------
            WebCookie.Expires = loginSuccessCookieModel.Expires;
            WebCookie.ApiSession = loginSuccessCookieModel.ApiSession;
            WebCookie.UserName = loginSuccessCookieModel.UserName.ToLower();  
            //Claim--------------------------------------------------------------------------------------------------------------
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            Claim claimUserName = new Claim(ClaimTypes.Name, WebCookie.UserName);
            Claim claimApiSession = new Claim(ClaimTypes.Name, WebCookie.ApiSession);
            identity.AddClaim(claimUserName);
            identity.AddClaim(claimApiSession);
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin")); // for roles 
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                IsPersistent = true
            });
        }
        protected void SetCookies(string key, string value, int minutes = 30)
        {
            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(minutes)
            });
        }
        //-------------------------------------------------------------------------------------------c
        [HttpGet]
        public IActionResult Register()
        {
            AccountModelInput accountModelInput = new AccountModelInput
            {
                UserName = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                session = WebCookie.ApiSession,
                Remark = string.Empty
            };
            return SwitchToApiOrView(accountModelInput);
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult Register(AccountModelInput accountModelInput)
        {
            if(string.IsNullOrEmpty(accountModelInput.Remark))
            {
                accountModelInput.Remark = string.Empty;
            }
            ResponseModalX responseModalX = new ResponseModalX();
            if(string.IsNullOrEmpty(accountModelInput.UserName)|| string.IsNullOrEmpty(accountModelInput.Password))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.User_UserNamePasswordNotNull };
                responseModalX.data = null;
                return Json(responseModalX);
            }
            if (string.IsNullOrEmpty(accountModelInput.Password) || string.IsNullOrEmpty(accountModelInput.ConfirmPassword))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.Login_ModifyComfirmed_Inconsistent };
                responseModalX.data = null;
                return Json(responseModalX);
            }
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtUser ftUserExist = businessContext.FtUser.Where(c => c.Name.Contains(accountModelInput.UserName)).FirstOrDefault();
                if(ftUserExist!=null)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success =false, ErrorCode = (int)RegisterErrorCode.Register_ExistUserName, Message = Lang.Register_ExistUserName },
                        data = null
                    };

                    LogHelper.Info($"{(int)RegisterErrorCode.Register_ExistUserName}-{Lang.Register_ExistUserName}");
                    return Json(responseModalX);
                }
                DateTime dt = DateTime.Now;
                int maxId = 20000;  //以2萬開始
                if (businessContext.FtUser.Count() > 0)
                {
                    maxId = businessContext.FtUser.Max(c => c.Id) + 1;
                }
                MainCom mainCom = new MainCom(); //默認值 
                FtUser ftUser1 = new FtUser
                {
                    Id = maxId,
                    MaincomId = WebCookie.MainComId ?? mainCom.MainComId,
                    Name = accountModelInput.UserName,
                    Password = accountModelInput.Password,
                    Visible = 1,
                    Remark = accountModelInput.Remark,
                    CreateTime = dt,
                    UpdateTime = dt,
                };
                businessContext.FtUser.Add(ftUser1);
                bool result = businessContext.SaveChanges() > 0 ? true : false;
                if (result)
                {
                    MetaModalX metaModalX = new MetaModalX();

                    responseModalX = new ResponseModalX
                    {
                        meta = metaModalX,
                        data = ftUser1
                    };

                    LogHelper.Info(JsonConvert.SerializeObject(responseModalX));
                    return Json(responseModalX);
                }
                else
                {
                    MetaModalX metaModalX = new MetaModalX
                    {
                        ErrorCode = (int)AuthorizationErrorCode.LONGIN_FAIL,
                        Success = false,
                        Message = Lang.GeneralUI_LoginFail
                    };
                    responseModalX = new ResponseModalX
                    {
                        meta = metaModalX,
                        data = null
                    };
                    return Json(responseModalX);
                }
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize]
        [HttpGet]
        public IActionResult PasswordModify([FromQuery]string userName)
        {
            if(!string.IsNullOrEmpty(userName))
            {
                userName = HttpUtility.UrlDecode(userName);
            }else
            {
                userName = string.Empty;
            }
            PasswordModify passwordModify = new PasswordModify
            {
                UserName = userName,
                Password = string.Empty,
                ConfirmPassword = string.Empty, 
            };
            return SwitchToApiOrView(passwordModify);
        }

        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult PasswordModify(PasswordModify passwordModify)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if (passwordModify.OrginalPassword == passwordModify.Password)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.PasswordModify_PasswordNotAllowSame };
                responseModalX.data = null;
                return Json(responseModalX);
            }
            if (passwordModify.ConfirmPassword!=passwordModify.Password)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.Login_ModifyComfirmed_Inconsistent };
                responseModalX.data = null;
                return Json(responseModalX);
            }
            if (string.IsNullOrEmpty(passwordModify.UserName) || string.IsNullOrEmpty(passwordModify.Password))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.User_UserNamePasswordNotNull };
                responseModalX.data = null;
                return Json(responseModalX);
            }
            using (BusinessContext businessContext = new BusinessContext())
            {
                FtUser ftUserExist = businessContext.FtUser.Where(c => c.Name.Contains(passwordModify.UserName)).FirstOrDefault();
                if (ftUserExist != null)
                {
                    FtUser ftUserOrginalMatch = businessContext.FtUser.Where(c => c.Name.Contains(passwordModify.UserName) && c.Password.Equals(passwordModify.OrginalPassword)).FirstOrDefault();
                    if(ftUserOrginalMatch == null)
                    {
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = false, ErrorCode = (int)LoginErrorCode.OrginalPasswordNotMatch, Message = Lang.GeneralUI_OrginalPasswordNotMatch },
                            data = null
                        };
                        return Json(responseModalX);
                    }
                    ftUserExist.Password = passwordModify.Password;
                    businessContext.FtUser.Update(ftUserExist);
                    bool result = businessContext.SaveChanges() > 0 ? true : false;
                    if (result)
                    { 
                        responseModalX = new ResponseModalX
                        {
                            meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_OK },
                            data = null
                        };  
                        return Json(responseModalX);
                    }
                    else
                    {
                        MetaModalX metaModalX = new MetaModalX
                        {
                            ErrorCode = (int)AuthorizationErrorCode.LONGIN_FAIL,
                            Success = false,
                            Message = Lang.GeneralUI_LoginFail
                        };
                        responseModalX = new ResponseModalX
                        {
                            meta = metaModalX,
                            data = null
                        };
                        return Json(responseModalX);
                    } 
                }else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)RegisterErrorCode.Register_ExistUserName, Message = Lang.Register_ExistUserName },
                        data = null
                    };
                    LogHelper.Info($"{(int)RegisterErrorCode.Register_ExistUserName}-{Lang.Register_ExistUserName}"); 
                    return Json(responseModalX);
                }
            }
        }
         
    }
}
