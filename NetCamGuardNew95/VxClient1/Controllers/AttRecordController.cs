using Common;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using EnumCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VideoGuard.ApiModels;
using VideoGuard.Business;
using VxGuardClient;
using VxGuardClient.Controllers;

namespace VxClient.Controllers
{
    public class AttRecordController : BaseController
    {
        private string HttpHost;
        private IOptions<UploadSetting> _uploadSetting;
        private string WebRootPath { get; set; }

        private BusinessContext businessContext { get; set; }
        private HistoryContext historyContext { get; set; }

        public AttRecordController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, ILogger<BaseController> logger, IHttpContextAccessor httpContextAccessor,
            IOptions<BusinessContext> busiContext, IOptions<HistoryContext> histContext, IOptions<UploadSetting> uploadSetting)
             : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            Logger = logger;
            WebRootPath = webHostEnvironment.WebRootPath;
            businessContext = busiContext.Value;
            historyContext = histContext.Value;
        }
        //为了能对接STARX而迁移过来的以下两个函数=======================================
        //CIC 手机安卓端拍卡专项对接
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        public IActionResult AddDeviceEntryOfNFCx([FromBody] DeviceEntryX entry)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            string entryJson = JsonConvert.SerializeObject(entry);
            Logger.LogInformation($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC:AttRecord.AddDeviceEntryOfNFCx] [CIC CARDINFO] [{entryJson}]");
            try
            {
                responseModalX.data = entry;
                 
                
                if (businessContext.Database.CanConnect())
                {
                    Logger.LogInformation($"[{ DateTime.Now:yyyy-MM-dd HH:mm:ss}][FUNC:AttRecord.AddDeviceEntryOfNFCx][DATABASE CONNECTION EVENT..OK] [DeviceSerialNo = " + entry.DeviceSerialNo + "]");
                }

                FtDevice device = new FtDevice();
                MainCom mainCom = new MainCom(); //默認值
               
                if (string.IsNullOrEmpty(entry.DeviceSerialNo))
                {
                    Logger.LogInformation("[FUNC:AttRecord.AddDeviceEntryOfNFCx][EXIST] [DeviceSerialNo = " + entry.DeviceSerialNo + "]");
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "No Device SerialNo" };
                    return Ok(responseModalX);
                }
                else
                {
                    entry.DeviceSerialNo = entry.DeviceSerialNo.Trim();
                    device = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(entry.DeviceSerialNo)).OrderByDescending(c => c.UpdateTime).FirstOrDefault();
                    if (device == null)
                    {
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "Device SerialNo Not EXIST!!!" };
                        string responseModalXJson11 = JsonConvert.SerializeObject(responseModalX);
                        Logger.LogInformation(responseModalXJson11);
                        return Ok(responseModalX);
                    }

                    //MainComId 不一致
                    if (device.MaincomId != entry.MainComId)
                    {
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "MainComId Not Consistency!!!" };
                        string responseModalXJson11 = JsonConvert.SerializeObject(responseModalX);
                        Logger.LogInformation(responseModalXJson11);
                        return Ok(responseModalX);
                    }
                }
                 
                string personName;
                FtPerson person = new FtPerson(); 
                if (string.IsNullOrEmpty(entry.AccesscardId))
                {
                    personName = string.Empty;
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "No AccessCardId" };
                    string responseModalXJson11 = JsonConvert.SerializeObject(responseModalX);
                    Logger.LogInformation(responseModalXJson11);
                    return Ok(responseModalX);
                }
                else
                {
                    entry.AccesscardId = entry.AccesscardId.Trim();
                    person = businessContext.FtPerson.Where(c => c.CardNo.Contains(entry.AccesscardId)).FirstOrDefault();
                    if (person == null)
                    {
                        personName = string.Empty;
                    }
                    else
                    {
                        personName = person.Name;
                    }
                }
              
                DateTime occur = DateTimeHelp.ConvertToDateTime(entry.OccurDateTime);
                DateTimeOffset dtf = new DateTimeOffset(occur); 
                long lOccurDatetime = dtf.ToUnixTimeMilliseconds();

                bool recordIsExist = false;
                if (person!=null)
                {
                    recordIsExist = historyContext.HistRecognizeRecord.Select(s => new { s.OccurDatetime, s.PersonId,s.CardNo})
                                               .Where(c => c.OccurDatetime == lOccurDatetime && c.PersonId == person.Id && c.CardNo.Contains(entry.AccesscardId)).Any();
                    if (recordIsExist)
                    {
                        responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"Exist this record:{occur:yyyy-MM-dd HH:mm:ss fff}" };
                        return Ok(responseModalX);
                    }
                }
                HistoryRecordsDefault recordsDefault = new HistoryRecordsDefault();
                DataBaseBusiness.ModelHistory.HistRecognizeRecord histRecognize = new DataBaseBusiness.ModelHistory.HistRecognizeRecord
                {
                        Mode = recordsDefault.Mode,
                        MaincomId = entry.MainComId,
                        OccurDatetime = lOccurDatetime,
                        DeviceId = recordsDefault.DeviceId,
                        DeviceName = recordsDefault.DeviceName,
                        TaskId = recordsDefault.TaskId,
                        TaskName = recordsDefault.TaskName,
                        CameraId = recordsDefault.CameraId,
                        CameraName = recordsDefault.CameraName,
                        PersonId = recordsDefault.PersonId,
                        PersonName = recordsDefault.PersonName,
                        Sex = recordsDefault.Sex,
                        CardNo = entry.AccesscardId,
                        Category = recordsDefault.Category,
                        LibId = recordsDefault.LibId,
                        LibName = recordsDefault.LibName,
                        Classify = recordsDefault.Classify,
                        PicPath = recordsDefault.PicPath,
                        CapturePath = recordsDefault.CapturePath,
                        Similarity = recordsDefault.Similarity,
                        Remark = recordsDefault.Remark,
                        Visible = recordsDefault.Visible,
                        CaptureTime = occur,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                };

                if (person != null && !string.IsNullOrEmpty(personName))
                { 
                    FtLibrary ftLibrary = new FtLibrary(); 
                    if (person.LibId > 0)
                    {
                        ftLibrary = businessContext.FtLibrary.Find(person.LibId);
                    } 
                    histRecognize.PersonId = person.Id;
                    histRecognize.PersonName = personName;
                    histRecognize.Sex = person.Sex;
                    histRecognize.LibId = person.LibId;
                    histRecognize.LibName = ftLibrary.Name ?? recordsDefault.LibName; 
                }

                try
                {
                    historyContext.HistRecognizeRecord.Add(histRecognize);
                    bool result = historyContext.SaveChanges() > 0;

                    if (result)
                    {
                        Logger.LogInformation(string.Format("[{0:F}]SUCCESS:{1}", DateTime.Now, entryJson));
                        responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"SERVER SAVE SUCCESSFULLY!{DateTime.Now:t}({entry.Mode})" };
                        responseModalX.data = entry; 
                        string responseModalXJson = JsonConvert.SerializeObject(responseModalX);
                        Logger.LogInformation(responseModalXJson);
                        return Ok(responseModalX);
                    }
                    else
                    {
                        Logger.LogInformation(string.Format("FAILURE:{0}", entryJson));
                        responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = "DATABASE ERROR" };
                        return Ok(responseModalX);
                    }
                }
                catch (Exception e)
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = e.Message };
                    string responseModalXJson11 = JsonConvert.SerializeObject(responseModalX);
                    Logger.LogInformation(responseModalXJson11);
                    return Ok(responseModalX);
                }
            }
            catch (Exception e)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = e.Message };
                string responseModalXJson11 = JsonConvert.SerializeObject(responseModalX);
                Logger.LogInformation(responseModalXJson11);
                return Ok(responseModalX);
            }
        }
    }
}
