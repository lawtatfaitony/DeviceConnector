using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using VxGuardClient.Context;
using VxGuardClient.Models;
using LogUtility;
using DataBaseBusiness.Models;
using VideoGuard.Business.V;
using Encryption;
using EnumCode;
using LanguageResource;
using VxClient;
using Microsoft.AspNetCore.Cors;
using FunctionProtected;
using System.Text;

namespace VxGuardClient.Controllers
{ 
    public partial class HomeController : BaseController
    { 
        public HomeController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,IOptions<TokenManagement> tokenManagement, ILogger<BaseController> logger)
           : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }
        [Authorize]
        public IActionResult Index()
        { 
            return View();
        }
        public IActionResult Index1()
        {
            return View();
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)] 
        public IActionResult Test()
        {
            ResponseModalX responseModalX = new ResponseModalX(); 
            
            return SwitchToApiOrView(responseModalX);
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult Test2()
        {
            ResponseModalX responseModalX = new ResponseModalX();

            return SwitchToApiOrView(responseModalX);
        }
        [Authorize]
        public IActionResult Menu()
        {
            return View();
        }
        [Authorize(AuthenticationSchemes = AuthSchemes)] 
        public IActionResult Privacy()
        { 
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult SoftWareAuthorized()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            BusinessContext dataBaseContext = new BusinessContext();

            #region 版权检测注册经镜头的的数量上限
            int qtyOfCameras = dataBaseContext.FtCamera.Count();

#if DEBUG
            qtyOfCameras = qtyOfCameras - 15; //調試需要
#endif
            //FunctionList
            string funListSignData = FunctionList.GetSignDataOfFunctionList();  ///FunctionProtected 項目 來自 F:\软件保护V3\源码\EncryptionREBUILD
            string funList = EncryptionRSA.RsaDecryptWithPrivate(funListSignData); //public加密，private解码
            FunctionList v2 = FunctionList.Get(funList);  //轉為整個功能對象
            if(v2.M1>=3)
            {
                v2 = FunctionList.FromJson(WebCookie.MainComId); //From Json
            }

            Logger.LogInformation(funList);
            //S1;A1;A2;A3;A4;D1;C1;M1;C2;H1
            string funlisttext = $"S1:{v2.S1};A1:{v2.A1};A2:{v2.A2};A3:{v2.A3};A4:{v2.A4};D1:{v2.D1};C1:{v2.C1};M1:{v2.M1};C2:{v2.C2};H1:{v2.H1}";
            Logger.LogInformation(funlisttext);

            string funlisttextDesc = $"{v2.S1.GetDesc()} S1:{v2.S1};{v2.A1.GetDesc()} A1:{v2.A1};{v2.A2.GetDesc()} A2:{v2.A2};{v2.A3.GetDesc()} A3:{v2.A3};{v2.A4.GetDesc()} A4:{v2.A4};{v2.D1.GetDesc()}  D1:{v2.D1};{v2.C1.GetDesc()} C1:{v2.C1};{v2.M1.GetDesc()} M1:{v2.M1};{v2.C2.GetDesc()} C2:{v2.C2};{v2.H1.GetDesc()} H1:{v2.H1}";
            Logger.LogWarning(funlisttextDesc);

            try
            {
                ServerHub serverHub = new ServerHub();
                string logSinalR = funlisttext;

                Task.Factory.StartNew(() =>
                {
                    serverHub.SendMdeiaMessage("ReceiveMediaMessage", logSinalR).GetAwaiter();
                    serverHub.SendMdeiaMessage("admin", logSinalR).GetAwaiter();
                }, TaskCreationOptions.LongRunning);
            }
            catch
            {
                Console.WriteLine("Task.Run Exception (FUNC::SoftWareAuthorized)");
            }


            if (qtyOfCameras > v2.C1) //符合条件
            {
                //由于授权限制,镜头注册数量超过上限
                //Due to authorization restrictions, the number of Camera registrations exceeds the upper limit | Camera |Camera
                string limitedMsg = $"{Lang.QTY_OF_AUTHORIZED_CAMERA} qty Of Cameras {qtyOfCameras} > {v2.C1}";
                Logger.LogInformation(limitedMsg);

                ResponseModalX responseModalX = new ResponseModalX();
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"{limitedMsg}<br>{funlisttext}" };
                responseModalX.data = null;
                return View("ResponseModal", responseModalX);
            }
            else
            {
                ResponseModalX responseModalX = new ResponseModalX();

                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = false, Message = $"OK PASS THE SOFTWARE AUTHORIZED QUANTITY PASS ;<br> CAMERAS = {qtyOfCameras} < MAX={v2.C1}<br>{funlisttext}" };
                responseModalX.data = null;
                return View("ResponseModal", responseModalX);
            }
            #endregion
        }

        // <summary>
        /// api test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("any")]
        [Route("[action]")]
        public IActionResult Hello()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.Now);
            ResponseModalX responseModalX = new ResponseModalX();
            responseModalX.meta.Message = $"Hello.......  {DateTime.Now:yyyy-MM-dd HH:mm:ss fff}";
            responseModalX.data = dateTimeOffset.ToUnixTimeMilliseconds();
            return Ok(responseModalX);
        }
    }
}
