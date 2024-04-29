using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.CameraMpeg;
using VideoGuard.Business;
using VxGuardClient;
using VxGuardClient.Controllers;
using LogUtility;
using VxGuardClient.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.IO;
using System.Net.Http;
using Common;

namespace VxClient.Controllers
{
    public class CamMpegController : BaseController
    {
        private IOptions<UploadSetting> _uploadSetting;
        private string wwwRootPath;
        public CamMpegController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<UploadSetting> uploadSetting, ILogger<BaseController> logger)
            : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            wwwRootPath = webHostEnvironment.WebRootPath;
            WebCookie.httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }
        public IActionResult Index()
        {
            LogHelper.Info("CamMpegController::Hello World!");
            return View();
        }

        [Route("[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult AddCameraMpeg([FromBody] CamMpegInfoInput input)
        {
            string loggerline;
            try
            {

                loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::AddCameraMpeg][{input.RecordId}][{input.DeviceSerialNo}][INPUT::{JsonConvert.SerializeObject(input)}]";
                Logger.LogInformation(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::AddCameraMpeg][PARSE INPUT JSON][EXCEPTION::{ex.Message}]";
                Logger.LogError(loggerline);
            }

            ResponseModalX responseModalX = new ResponseModalX();
            var camera = CameraBusiness.CameraDetails(input.CameraId, ref responseModalX);
            if (camera == null)
            {
                return Ok(responseModalX);
            }

            #region 同一目錄下不存在相同文件名,所以不用判斷這個
            //bool IsTheSameFileName = CamMpegBusiness.IsTheSameOfMpegFilename(camera.DeviceId, input.MpegFileName, ref responseModalX);
            //if (IsTheSameFileName)
            //{
            //    loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::AddCameraMpeg][EXIST MEDIA THE SAME NAME][{responseModalX.meta.Message}]";
            //    Logger.LogInformation(loggerline);
            //    responseModalX.meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CamMpeg_EXIST_THE_NAME, Success = false, Message = $"[{Lang.CamMpeg_EXIST_THE_NAME}] [{input.RecordId} {input.MpegFileName}]" };
            //    return Ok(responseModalX);
            //}  
            #endregion

            if (input.StartTimestamp.ToString().Length == 10)
            {
                input.StartTimestamp = Common.DateTimeHelp.ConvertToMillSecond(input.StartTimestamp);
            }

            if (input.EndTimestamp.ToString().Length == 10)
            {
                input.EndTimestamp = Common.DateTimeHelp.ConvertToMillSecond(input.EndTimestamp);
            }

            using BusinessContext businessContext = new BusinessContext();

            bool chkSerialNo = DeviceBusiness.GetDeviceIdByDeviceSerialNo(input.DeviceSerialNo, out int deviceId);
            if (false == chkSerialNo)
            {
                loggerline = $"[{DateTime.Now}][FUNC::CamMpegController.AddCameraMpeg:GetDeviceIdByDeviceSerialNo]{Lang.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST}[PLS CHK DEVICE SERIAL No.:{input.DeviceSerialNo}][ERROR CODE:{(int)CamMpegErrorCode.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST}]";
                Logger.LogInformation(loggerline);
                responseModalX.meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST, Success = false, Message = Lang.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST };
                return Ok(responseModalX);
            }
            var device = businessContext.FtDevice.Find(deviceId);
            if (device == null)
            {
                loggerline = $"[{DateTime.Now}][FUNC::CamMpegController.AddCameraMpeg][DEVICE_NOT_EXIST][{Lang.DEVICE_NOT_EXIST}][{deviceId}]";
                Logger.LogInformation(loggerline);
                responseModalX.meta = new MetaModalX { ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Success = false, Message = $"[{Lang.DEVICE_NOT_EXIST}][{deviceId}]" };
                return Ok(responseModalX);
            }
            if (input.RecordId == 0)
            {
                loggerline = $"[{DateTime.Now}][FUNC::CamMpegController.AddCameraMpeg][Record Id = 0 ][{Lang.Device_IlleggleDeviceId}]";
                Logger.LogInformation(loggerline);
                responseModalX.meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CAMMPEG_RECORE_ID_ERROR, Success = false, Message = Lang.CAMMPEG_RECORE_ID_ERROR };
                return Ok(responseModalX);
            }
            loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::CamMpegController.AddCameraMpeg][INPUT DATA][DeviceId={deviceId}][{input.DeviceSerialNo}]";
            Logger.LogInformation(loggerline);
            DateTime dt = DateTime.Now;

            FtCameraMpeg ftCameraMpeg = new FtCameraMpeg
            {
                Id = input.RecordId,
                DeviceId = device.DeviceId,
                CameraId = input.CameraId,
                MpegFilename = input.MpegFileName,
                FileFomat = input.FileFormat,
                FileSize = input.FileSize,
                IsGroup = Convert.ToUInt64(false),
                GroupTimestamp = 0,
                StartTimestamp = input.StartTimestamp,
                EndTimestamp = input.EndTimestamp,
                Visible = (int)CamMpegErrorCode.CamMpeg_VISIBLE,
                IsFormatVirified = 0,
                IsUpload = (sbyte)CamMpegIsUpload.DVR_RECORD_DEFAULT,  //默認是DVR記錄  /初始增加一條錄像記錄下的默認值
                CreateTime = dt
            };
            bool result = CamMpegBusiness.AddCameraMpeg(ftCameraMpeg, ref responseModalX);

            string logSinalR;
            DateTime dtStarTtime = DateTimeHelp.ConvertToDateTime(input.StartTimestamp);
            DateTime dtEndTime = DateTimeHelp.ConvertToDateTime(input.EndTimestamp);
            if (result)
            {
                loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::CamMpegController.AddCameraMpeg][INSERT][SUCCESS][DeviceId={deviceId}][{input.DeviceSerialNo}]";
                Logger.LogInformation(loggerline);
            }
            else
            {
                loggerline = $"[{DateTime.Now:yyyy:MM-dd HH:mm:ss fff}][FUNC::CamMpegController.AddCameraMpeg][INSERT][FAIL][DeviceId={deviceId}][{input.DeviceSerialNo}]";
                Logger.LogInformation(loggerline);

                loggerline = $"[FUNC::CamMpegController.AddCameraMpeg][INSERT][FAIL][DeviceId={deviceId}][{input.DeviceSerialNo}]";
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);
            }

            ServerHub serverHub = new ServerHub();
            string resMsg = result ? "SUCCESS" : "FAIL";
            logSinalR = $"[MEADIA RECORD INSERT][{resMsg}][DeviceId={deviceId}][{dtStarTtime:yyyy-MM-dd HH:mm:ss fff}]-[{dtEndTime:yyyy-MM-dd HH:mm:ss fff}]";

            try
            {
                //Task.Factory.StartNew(() =>
                //{
                //   serverHub.SendMdeiaMessage("admin", logSinalR).GetAwaiter();
                //}, TaskCreationOptions.LongRunning);

                Task.Run(() =>
                {
                    serverHub.SendMdeiaMessage("admin", logSinalR).GetAwaiter();
                });
            }
            catch
            {
                Console.WriteLine("Task.Run Exception");
            }

            return Ok(responseModalX);
        }

        /// <summary>
        /// 刪除錄像文件的信息屬性記錄
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("[controller]/[action]")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult DeleteCameraMpegRecord([FromBody] DelMpegInfoInput input)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            using BusinessContext businessContext = new BusinessContext();
            bool chkSerialNo = DeviceBusiness.GetDeviceIdByDeviceSerialNo(input.DeviceSerialNo, out int deviceId);
            if (false == chkSerialNo)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST, Success = false, Message = Lang.CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST };
                return Ok(responseModalX);
            }

            if (input.RecordId == 0)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = false, Message = $"RecordId/camera_mpeg_id not allow null/zero!" };
                return Ok(responseModalX);
            }
            try
            {
                var recordePKey = businessContext.FtCameraMpeg.Select(s => new { s.DeviceId, s.Id }).Where(c => c.DeviceId == deviceId && c.Id == input.RecordId).FirstOrDefault();

                if (recordePKey != null)
                {
                    var cameraMpeg = businessContext.FtCameraMpeg.Where(c => c.DeviceId == deviceId && c.Id == input.RecordId).FirstOrDefault();
                    businessContext.FtCameraMpeg.Remove(cameraMpeg);
                    bool res = businessContext.SaveChanges() > 0;
                    if (res)
                    {
                        responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Success = res, Message = $" {input.RecordId} {Lang.GeneralUI_SUCC} {Lang.GeneralUI_Delete}" };
                        responseModalX.data = cameraMpeg;
                        return Ok(responseModalX);
                    }
                    else
                    {
                        responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.FAIL, Success = res, Message = $" {input.RecordId}/{deviceId} {Lang.GeneralUI_Fail} {Lang.GeneralUI_Delete}" };
                        return Ok(responseModalX);
                    }
                }
                else
                {
                    var device = businessContext.FtDevice.Where(c => c.DeviceSerialNo.Contains(input.DeviceSerialNo)).FirstOrDefault();
                    responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Success = false, Message = $"RecordId/DeviceId {input.RecordId}/{device.DeviceId} {Lang.GeneralUI_NoRecord}({Lang.CamMpeg_DelNotExistRecordTips})" };
                    return Ok(responseModalX);
                }
            }
            catch (Exception ex)
            {
                responseModalX.meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.EXCEPTION, Success = false, Message = $"{ex.Message}" };
                return Ok(responseModalX);
            }
        }

        [AllowAnonymous] //For MPEGREC 項目的
        [Route("[controller]/[action]")]
        [HttpPost]
        public IActionResult CameraStreamInfoList([FromBody] DeviceSerialNoInput deviceSerialNoInput)
        {
            ResponseModalX responseModalX = new ResponseModalX();

            bool verifiedToken = DeviceBusiness.VerifiedDeviceToken(deviceSerialNoInput.DeviceSerialNo, deviceSerialNoInput.DeviceToken);
            if (!verifiedToken)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceAuthorizationErrorCode.DEVICE_AUTHORIZATION_ERROR, Message = Lang.DEVICE_AUTHORIZATION_ERROR };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            var cameraList = businessContext.FtCamera.Where(c => c.DeviceSerialNo.Contains(deviceSerialNoInput.DeviceSerialNo) & c.Visible == (int)CameraErrorCode.CAM_IS_VISIBLE).ToList();
            List<CameraStreamInfo> cameraStreamInfos = new List<CameraStreamInfo>();
            foreach (var item in cameraList)
            {
                string nameOfCameraStreamInfo = string.Format("{0}{1}", DeviceBusiness.CameraStreamInfo_Prefix, item.Id);
                FtConfig config = businessContext.FtConfig.Where(c => c.Name.Contains(nameOfCameraStreamInfo) && c.Visible == 1).FirstOrDefault();
                if (config != null)
                {
                    CameraStreamInfo cameraStreamInfo = JsonConvert.DeserializeObject<CameraStreamInfo>(config.Config);
                    cameraStreamInfos.Add(cameraStreamInfo);
                }
            }
            responseModalX.data = cameraStreamInfos;
            return Ok(responseModalX);
        }

        //http://localhost:5002/CamMpeg/Files/3d6e82ab45b601eac677e9d9da3cf882d12da03b/2000000000

        //https://qa.1r1g.com/sf/ask/2430838021/  在C#WebApi中直播FLV
        /// <summary>
        /// z轉發FLV | MP4  https://www.jianshu.com/p/70b4ff4a9d5f
        /// </summary>
        /// <param name="hmac"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("[controller]/Files/{hamc}/{id}")] // format: http://localhost:5002/Files/3d6e82ab45b601eac677e9d9da3cf882d12da03b/2000000000  設備id作key加密的結果 = 3d6e82ab45b601eac677e9d9da3cf882d12da03b
        [HttpGet]
        public FileContentResult Get(string hmac, long id)
        {

            HttpContext context = this.HttpContext;
            string uploadFolder = _uploadSetting.Value.TargetFolder;
            Logger.LogInformation(uploadFolder);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(id);

            using BusinessContext businessContext = new BusinessContext();
            var cameraMpeg = businessContext.FtCameraMpeg.Find(id);
            Microsoft.Net.Http.Headers.MediaTypeHeaderValue mediaTypeHeaderValue = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            if (cameraMpeg.FileFomat.Contains("flv"))
            {
                mediaTypeHeaderValue = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("video/flv");
            }
            string hmac2 = Common.CommonBase.HMACSHA1Encode(id.ToString(), cameraMpeg.DeviceId.ToString());

            if (hmac != hmac2)
            {
                string dateFolder = string.Format("{0:yyyyMMdd}", dateTimeOffset.UtcDateTime);
                Logger.LogInformation(dateFolder);
            }
            var url = $"http://192.168.0.146:180/record/31/BC/5200818452.flv";

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.KeepAlive = true;

            var resp = (HttpWebResponse)req.GetResponse();

            var response = context.Response;
            response.StatusCode = (int)resp.StatusCode;
            response.ContentType = resp.ContentType;

            int total = 0;
            byte[] buffer = new byte[65536];
            Stream stream = resp.GetResponseStream();
            while (!context.RequestAborted.IsCancellationRequested)
            {
                try
                {
                    int len = stream.Read(buffer, 0, buffer.Length);
                    if (len <= 0 || context.RequestAborted.IsCancellationRequested)
                    {
                        break;
                    }
                    context.Response.Body.WriteAsync(buffer, 0, len);
                    context.Response.Body.Flush();
                    total += len;
                    continue;
                }
                catch (Exception) { if (total < 1024 * 1024) { throw; } }
                break;
            }
            try { resp.Dispose(); } catch (Exception) { }
            try { stream.Dispose(); } catch (Exception) { }

            return null;
        }

        [HttpHead, HttpGet, HttpPost, Route("[controller]/Mp4/{mp4Name}.mp4")]
        public object Mp4(string mp4)
        {
            var url = $"http://实际服务器:8888/{mp4}.mp4";
            RouteMp4(this.HttpContext, url);
            return null;
        }

        //http://localhost:5002/CamMpeg/Flv/576442186286.flv
        //參考 https://blog.csdn.net/admans/article/details/102728129
        [HttpHead, HttpGet, HttpPost, Route("[controller]/Flv/{FlvName}.flv")]
        public async Task<IActionResult> Flv(string FlvName)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                //Clients.Add(socket);
                //await WaitForClose(HttpContext, socket);

                var url = $"http://实际服务器:8888/{FlvName}.mp4";
                url = $"http://192.168.0.146:180/record/1A/AA/{FlvName}.flv?token=2ccd575f54be0d71277a82f8baf2e8ea";  //http://192.168.0.146:180/record/1A/AA/576442186286.flv?token=2ccd575f54be0d71277a82f8baf2e8ea
                RouteFlv(this.HttpContext, url);
            }
            return new EmptyResult();
            // return Ok();
        }
        /// <summary>
        /// 读取一个 mp4 的 URL, 并将接收到的 实时字节写入 HttpContext
        /// <para>本函数可以实现 MP4 的 URL转发</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="url"></param>
        private static void RouteMp4(HttpContext context, string url)
        {
            var request = context.Request;

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.CookieContainer = new CookieContainer();
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            req.KeepAlive = true;
            req.Referer = "";
            req.AllowAutoRedirect = true;
            if (request.Headers != null && request.Headers.Count >= 1)
            {
                var headers = request.Headers;
                foreach (var header in headers)
                {
                    try { req.Headers[header.Key] = header.Value; } catch { }
                }
            }


            var resp = (HttpWebResponse)req.GetResponse();
            var stateCode = (int)resp.StatusCode;
            var contentType = resp.ContentType;
            var contentLength = resp.ContentLength;

            //设置返回状态
            var response = context.Response;
            response.StatusCode = stateCode;
            response.ContentType = contentType;
            response.ContentLength = contentLength;

            resp.Cookies = ((req.CookieContainer == null) ? null : req.CookieContainer.GetCookies(req.RequestUri));
            if (resp.Headers != null)
            {
                string[] keys = resp.Headers.AllKeys;
                foreach (string key in keys)
                {
                    if (!(key.ToLower() == "Transfer-Encoding".ToLower()))
                    {
                        try { response.Headers[key] = resp.Headers[key]; } catch { }
                    }
                }
            }


            int total = 0;
            byte[] buffer = new byte[65536];
            Stream stream = resp.GetResponseStream();
            while (!context.RequestAborted.IsCancellationRequested)
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                if (len <= 0 || context.RequestAborted.IsCancellationRequested) break;

                context.Response.Body.Write(buffer, 0, len);
                context.Response.Body.Flush();
                total += len;
                continue;
            }

            try { resp.Dispose(); } catch (Exception) { }
            try { stream.Dispose(); } catch (Exception) { }
        }

        private static void RouteFlv(HttpContext context, string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            //req.CookieContainer = new CookieContainer();
            //req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            //req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            //req.KeepAlive = true;
            //req.Referer = "";
            //req.AllowAutoRedirect = true;

            var resp = (HttpWebResponse)req.GetResponse();
            //int stateCode = (int)resp.StatusCode;
            //string contentType = resp.ContentType;

            var response = context.Response;
            //response.StatusCode =  stateCode;  
            //Microsoft.Net.Http.Headers.MediaTypeHeaderValue mediaTypeHeaderValue = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("video/x-flv");
            response.ContentType = req.ContentType; //"video/x-flv";
            //resp.Cookies = ((req.CookieContainer == null) ? null : req.CookieContainer.GetCookies(req.RequestUri));
            //if (resp.Headers != null)
            //{
            //    string[] keys = resp.Headers.AllKeys;
            //    string[] array = keys;
            //    foreach (string key in array)
            //    {
            //        if (!(key.ToLower() == "Content-Length".ToLower()) && !(key.ToLower() == "Content-Disposition".ToLower()) && !(key.ToLower() == "Accept-Ranges".ToLower()) && !(key.ToLower() == "Transfer-Encoding".ToLower()))
            //        {
            //            try { response.Headers[key] = resp.Headers[key]; } catch { }
            //        }
            //    }
            //}

            int total = 0;
            byte[] buffer = new byte[65536];
            Stream stream = resp.GetResponseStream();
            while (!context.RequestAborted.IsCancellationRequested)
            {
                try
                {
                    int len = stream.Read(buffer, 0, buffer.Length);
                    if (len <= 0 || context.RequestAborted.IsCancellationRequested)
                    {
                        break;
                    }
                    context.Response.Body.WriteAsync(buffer, 0, len);
                    context.Response.Body.Flush();
                    total += len;
                    continue;
                }
                catch (Exception) { if (total < 1024 * 1024) { throw; } }
                break;
            }
            try { resp.Dispose(); } catch (Exception) { }
            try { stream.Dispose(); } catch (Exception) { }
        }
    }
    //public class FileDownloadStream
    //{
    //    private readonly string _filename;

    //    public FileDownloadStream(string filePath)
    //    {
    //        _filename = filePath;
    //    }

    //    public void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
    //    {
    //        try
    //        {
    //            var buffer = new byte[4096];

    //            using (var video = File.Open(_filename, FileMode.Open, FileAccess.Read))
    //            {
    //                var length = (int)video.Length;
    //                var bytesRead = 1;

    //                while (length > 0 && bytesRead > 0)
    //                {
    //                    bytesRead = video.Read(buffer, 0, Math.Min(length, buffer.Length));
    //                    outputStream.Write(buffer, 0, bytesRead);
    //                    length -= bytesRead;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return;
    //        }
    //        finally
    //        {
    //            outputStream.Close();
    //        }
    //    }
    //}

    //private HttpResponseMessage DownloadContentChunked()
    //{
    //    var filename = HttpContext.Current.Request["f"];
    //    var filePath = _storageRoot + filename;
    //    if (File.Exists(filePath))
    //    {
    //        var fileDownload = new FileDownloadStream(filePath);
    //        var response = Request.CreateResponse();
    //        response.Content = new PushStreamContent(fileDownload.WriteToStream, new MediaTypeHeaderValue("application/octet-stream"));
    //        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
    //        {
    //            FileName = filename
    //        };
    //        return response;
    //    }
    //    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "");
    //}


    //Blob 问题

    //参考 https://volosoft.com/blog/File-Upload-Download-with-BLOB-Storage-in-ASP.NET-Core-and-ABP
}
