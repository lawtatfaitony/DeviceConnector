using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.ApiModels;
using VxGuardClient;
using VxGuardClient.Context;
using VxGuardClient.Controllers;
using VxGuardClient.Models;
namespace VxClient.Controllers
{
    public class SiteController : BaseController
    {
        public SiteController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor,  ILogger<BaseController> logger)
             : base(webHostEnvironment, httpContextAccessor)
        {
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger; 
        }
        [HttpGet]
        [Authorize]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult AddSite()
        {
            MainCom mainCom = new MainCom(); //default value 
            SiteInputEntry siteInputEntry = new SiteInputEntry
            {
                MaincomId = WebCookie.MainComId ?? mainCom.MainComId,
                SiteId = 0,
                ParentsId =0,
                ParentsSiteName= Lang.GeneralUI_Root,
                SiteName = string.Empty,
                Address = string.Empty
            };
            return View(siteInputEntry);
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult AddSite(SiteInputEntry input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            MainCom mainCom = new MainCom(); //default value 

            input.MaincomId ??= WebCookie.MainComId ?? mainCom.MainComId;
            if (string.IsNullOrEmpty(input.MaincomId))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return SwitchToApiOrView(responseModalX);
            }

            //chk start---------------------------------------------------------------
            if (string.IsNullOrEmpty(input.SiteName))
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Site_SiteName}{Lang.GeneralUI_Optional}" };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            int maxId = 2000; //以200開始
            if (businessContext.FtSite.Count() > 0)
            {
                maxId = businessContext.FtSite.Max(c => c.SiteId) + 1;
            }

            FtSite ftSite = new FtSite {
                MaincomId = input.MaincomId,
                SiteId = maxId,
                ParentsId = input.ParentsId,
                SiteName = input.SiteName,
                CameraCount = 0,
                PersonCount =0,
                Address = input.Address,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            businessContext.FtSite.Add(ftSite);
            bool res = businessContext.SaveChanges() > 0;
            if(res)
            {
                responseModalX.data = ftSite; 
                return Ok(responseModalX);
            }
            else
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = Lang.GeneralUI_Fail };
                return Ok(responseModalX);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("{Language}/[controller]/[action]/{siteId}")]
        public IActionResult UpdateSite(int siteId)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();
            FtSite ftSite = businessContext.FtSite.Find(siteId);
            if(ftSite==null)
            {
                responseModalX.meta = new MetaModalX {Success=false, ErrorCode=(int)GeneralReturnCode.GENERALUI_NO_RECORD, Message=Lang.GeneralUI_NoRecord };
                responseModalX.data = null;
                return SwitchToApiOrView(ftSite);
            }
            string parentsSiteName = Lang.GeneralUI_Root;
            if(ftSite.ParentsId!=0)
            {
                parentsSiteName = businessContext.FtSite.Find(ftSite.ParentsId)?.SiteName??string.Empty;
            }
            SiteInputEntry siteInputEntry = new SiteInputEntry
            {
                MaincomId = ftSite.MaincomId, //临时
                SiteId = ftSite.SiteId,
                ParentsId = ftSite.ParentsId.GetValueOrDefault(),
                ParentsSiteName = parentsSiteName, 
                SiteName = ftSite.SiteName,
                Address = ftSite.Address
            };
            responseModalX.data = siteInputEntry;

            return SwitchToApiOrView(responseModalX);
        }

        [HttpGet]
        [Route("{Language}/[controller]/[action]/{MaincomId}/{parentsSiteId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult GetNodeOfSites(string maincomId,int parentsSiteId = 0)  // 所以节点 :  ParentsSiteId="0"
        {
            using BusinessContext businessContext = new BusinessContext();
            List<FtSite> ftSites = businessContext.FtSite.Where(c =>c.MaincomId.Contains(maincomId) && c.ParentsId == parentsSiteId).ToList();
            if(parentsSiteId!=0&& ftSites?.Count()==0)  //預防節點下無數據
            {
                ftSites = businessContext.FtSite.Where(c => c.MaincomId.Contains(maincomId)).ToList();
            }
            GetTreeJsonByTable(ftSites, parentsSiteId);

            string strResult = result.ToString();
           // JObject jo = JObject.Parse(strResult); 
            return Ok(strResult); 
        }
        /// <summary>
        /// 根据DataTable生成Json树结构
        /// </summary>

        StringBuilder result = new StringBuilder();
        StringBuilder sb = new StringBuilder();
        private void GetTreeJsonByTable(List<FtSite> ftSites,int parentsId)
        {
            using BusinessContext businessContext = new BusinessContext();
            var allList = businessContext.FtSite.ToList();
            result.Append(sb);
            sb.Clear();
            if (ftSites?.Count() > 0)
            {
                sb.Append("\n[");

                foreach (var row in ftSites)
                {
                    sb.Append("{\"nodeid\":\"" + row.SiteId + "\",\"text\":\"" + row.SiteName + "\",\"parentsId\":\"" + row.ParentsId + "\"");

                    var subOfFtSites = allList.Where(c => c.ParentsId == row.SiteId);
                    if (subOfFtSites?.Count() > 0)
                    {
                        sb.Append(",\"nodes\":");
                        GetTreeJsonByTable(subOfFtSites.ToList(), row.SiteId);
                        result.Append(sb);
                        sb.Clear();
                    }
                    result.Append(sb);
                    sb.Clear();
                    sb.Append("},");
                }
                sb = sb.Remove(sb.Length - 1, 1);

                sb.Append("]");
                result.Append(sb);
                sb.Clear();
            }
        }
         
    }
}
