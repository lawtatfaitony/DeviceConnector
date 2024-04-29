using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VideoGuard.ApiModels;
using static EnumCode.EnumBusiness;

namespace VideoGuard.Business
{
    public partial class DoorBusiness
    { 
        public static StringBuilder SiteResult = new StringBuilder();
        public static StringBuilder SiteSb = new StringBuilder();
        /// <summary>
        /// 獲取門禁的樹狀結構圖
        /// </summary>
        /// <param name="ftSites"></param>
        /// <param name="parentsId">默認值=0</param>
        public static void GetDoorTreeJson(List<FtSite> ftSites, int parentsId)
        {
            using BusinessContext businessContext = new BusinessContext();
            var allList = businessContext.FtSite.ToList();
            if(parentsId!=0)
            {
                allList = allList.Where(s => s.ParentsId == parentsId).ToList();
            }
            SiteResult.Append(SiteSb);
            SiteSb.Clear();
            if (ftSites?.Count() > 0)
            {
                SiteSb.Append("\n[");

                foreach (var row in ftSites)
                {
                    SiteSb.Append("{\"nodeid\":\"" + row.SiteId + "\",\"text\":\"" + row.SiteName + "\",\"parentsId\":\"" + row.ParentsId + "\"");

                    //附加門禁列表 OK 2022-9-21
                    List<DoorSiteTreeModel> doorSiteTreeModels = GetDoorListBySite(row.SiteId);
                    if (doorSiteTreeModels?.Count > 0)
                    {
                        if(SiteSb?.Length>0)
                        {
                            string strSiteSB = SiteSb.ToString();
                            if (strSiteSB.Substring(SiteSb.Length - 1, 1) == ",")
                            {
                                SiteSb = SiteSb.Remove(SiteSb.Length - 1, 1);
                            }
                            SiteSb.Append(",\"nodes\":");
                        }

                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        settings.Formatting = Formatting.Indented; 

                        string jsonDoors = JsonConvert.SerializeObject(doorSiteTreeModels, settings);
                        SiteSb.Append(jsonDoors); 
                    }

                    var subOfFtSites = allList.Where(c => c.ParentsId == row.SiteId);
                    if (subOfFtSites?.Count() > 0)
                    {
                        SiteSb.Append(",\"nodes\":");
                        GetDoorTreeJson(subOfFtSites.ToList(), row.SiteId);
                        SiteResult.Append(SiteSb);
                        SiteSb.Clear();
                    }
                    SiteResult.Append(SiteSb);
                    SiteSb.Clear();
                    SiteSb.Append("},"); 
                }
                SiteSb = SiteSb.Remove(SiteSb.Length - 1, 1);

                SiteSb.Append("]");
                SiteResult.Append(SiteSb);
                SiteSb.Clear();
            }
        }

        /// <summary>
        /// 獲取位置下的門禁列表
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static List<DoorSiteTreeModel> GetDoorListBySite(int siteId)
        {
            using BusinessContext dbContext = new BusinessContext();
            var doors = dbContext.FtDoor.Where(c => c.SiteId == siteId);
            List<DoorSiteTreeModel> doorSiteTreeModels = new List<DoorSiteTreeModel>();

            if(doors?.Count()>0)
            {
                foreach (var item in doors)
                {
                    DoorSiteTreeModel doorSiteTreeModel = new DoorSiteTreeModel
                    {
                        NodeId = item.DoorId,
                        Text = item.DoorName,
                        DeviceId = item.DeviceId.GetValueOrDefault(),
                        DeviceName = item?.DeviceName??string.Empty,
                        DoorId = item.DoorId,
                        DoorName = item.DoorName,
                        MainComId = item.MaincomId,
                        ParentsId = siteId,
                        SiteId = item.SiteId,
                        SiteName = item.SiteName    
                    };

                    doorSiteTreeModels.Add(doorSiteTreeModel);
                }
                return doorSiteTreeModels;
            }
            else
            {
                return null;
            } 
        }
        /// <summary>
        /// 獲取位置相關的門列表
        /// </summary>
        /// <param name="mainComId"></param>
        /// <returns></returns>
        public static List<DoorSiteTreeModel> GetDoorModelList(string mainComId)
        {
            using BusinessContext dbContext = new BusinessContext();
            var sitesDoor = dbContext.FtDoor.Where(c => c.MaincomId == mainComId).Select(s=>s.SiteId).Distinct();
            List<DoorSiteTreeModel> doorSiteTreeModels = new List<DoorSiteTreeModel>();

            foreach(var item in sitesDoor)
            {
                List<DoorSiteTreeModel> doorListBySite = GetDoorListBySite(item);
                if(doorListBySite?.Count()>0)
                {
                    doorSiteTreeModels.AddRange(doorListBySite);
                }
            }
            Console.WriteLine(JsonConvert.SerializeObject(doorSiteTreeModels));

            return doorSiteTreeModels;
        }

        /// <summary>
        /// 檢測是否同名
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="personName"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool ChkValidOfDoorName(string mainComId,int doorId, string doorName, out ResponseModalX responseModalX)
        {
            if (!string.IsNullOrEmpty(doorName))
            {
                responseModalX = new ResponseModalX();

                using BusinessContext businessContext = new BusinessContext();

                bool checkSame;
                if (doorId!=0)
                {
                    checkSame = businessContext.FtDoor.Where(c => c.MaincomId.Contains(mainComId) && c.DoorId!= doorId && c.DoorName == doorName).Any();
                }
                else
                {
                    checkSame = businessContext.FtDoor.Where(c => c.MaincomId.Contains(mainComId) && c.DoorName == doorName).Any();
                }

                //不存在,則沒有同名,
                if (checkSame == false)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = "NOT EXIST THE DOOR NAME" },
                        data = null
                    };
                }
                else
                {
                    //PERSON_EXIST_THE_NAME
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = true, ErrorCode = (int)DoorErrorCode.DOOR_EXIST_THE_SAME_NAME, Message = Lang.DOOR_EXIST_THE_SAME_NAME },
                        data = null
                    };
                }
            }
            else
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)PersonErrorCode.PERSON_INVALID_NAME, Message = Lang.PERSON_INVALID_NAME },
                    data = null
                };
            }
            return responseModalX.meta.Success;
        }
    }

    public class DoorSiteTreeModel
    {
        [JsonProperty("nodeid")]
        public int NodeId { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        public string MainComId { get; set; }
        public int DoorId { get; set; }
        public string DoorName { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int ParentsId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }

    }
    public partial class Door
    {
        [DefaultValue(0)]
        public int DoorId { get; set; }
        public string DoorName { get; set; }
        public string MaincomId { get; set; }
        [DefaultValue(0)]
        public int DeviceId { get; set; } 

        [DefaultValue(0)]
        public int SiteId { get; set; } 
    }
   
    public partial class DelDoorInput
    {
        public string MaincomId { get; set; }
        [DefaultValue(0)]
        public int DoorId { get; set; }
        
    }
}
