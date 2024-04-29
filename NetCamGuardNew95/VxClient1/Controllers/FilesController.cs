using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net.Appender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using VideoGuard.ApiModels;
using EnumCode;
using LanguageResource;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Headers;
using VideoGuard.ApiModels.ApiModels;
using LogUtility;
using Microsoft.Extensions.Logging;

namespace VxGuardClient.Controllers
{  
    public partial class FilesController : BaseController
    {
        private static object lockObj = new object();
        private string HttpHost;
        private IOptions<UploadSetting> _uploadSetting;
        private string wwwRootPath;
        public FilesController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<TokenManagement> tokenManagement, IOptions<UploadSetting> uploadSetting, ILogger<BaseController> logger)
             : base(webHostEnvironment, httpContextAccessor)
        {
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            wwwRootPath = webHostEnvironment.WebRootPath;
            Logger = logger;
        }

        [Authorize]
        [Route("[controller]/[action]")]
        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            PictureUploadRet pictureUploadRet = new PictureUploadRet { PicUrl = "", PicClientUrl = "" }; 
            UserFile userFile = new UserFile(file); 

            ResponseModalX responseModalX = new ResponseModalX(); 
            string uploadFolder = _uploadSetting.Value.TargetFolder;
            long fileSizeLimit = _uploadSetting.Value.FileSizeLimit;
            if(fileSizeLimit < userFile.Length)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILESIZE_IS_LIMITED, Message = Lang.FILESIZE_IS_LIMITED }
                }; 
                return Ok(responseModalX);
            }
            string monthFolder = string.Format("{0:yyyyMM}",DateTime.Now); 
            string targetPath = Path.Combine(webHostEnvironment.ContentRootPath, uploadFolder, monthFolder);
            
            responseModalX = userFile.SaveAs(targetPath).Result;

            if(responseModalX.meta.Success)
            {
                string picClientUrl = responseModalX.data.ToString().Trim(); 
                pictureUploadRet.PicClientUrl = $"{HttpHost}/{picClientUrl.TrimStart('/')}";
                string[] pathStructArr = picClientUrl.Split("/");
                string retFileName = string.Format("{0}.jpg", pathStructArr[pathStructArr.Length - 1]);
                string sourcePathOfPicClientUrl = Path.Combine(targetPath, retFileName);
                string picUrl = picClientUrl;// 已经没有CamGuard 完全取消：UploadToBackend(sourcePathOfPicClientUrl);
                
                pictureUploadRet.PicUrl = picUrl;
                responseModalX.data = pictureUploadRet;

                //Case: third part API SERVER STATUS is closed!!!
                if (string.IsNullOrEmpty(picUrl))
                {
                    responseModalX.meta.ErrorCode = (int)FileErrorCode.FILE_BACKEND_CLOSED;
                    pictureUploadRet.PicUrl = ""; 
                }
            }
            return Ok(responseModalX);
        }

        /// <summary>
        /// UploadToBackend
        /// </summary>
        /// <param name="PathFileName"></param>
        /// <returns></returns>
        private string UploadToBackend(string PathFileName)
        { 
            string UploadPersonPictureApi = string.Format("http://{0}:{1}/upload", _uploadSetting.Value.UploadServerIp, _uploadSetting.Value.UploadServerPort);  

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                client.DefaultRequestHeaders.Add("Connection", "Keep-Alive"); // client.DefaultRequestHeaders.Add("Connection", "close");
                client.DefaultRequestHeaders.Add("Keep-Alive", "timeout=6000"); //6s

                try
                { 
                    MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                    FileStream fs = System.IO.File.Open(PathFileName, FileMode.Open);
                    StreamContent streamContent = new StreamContent(fs);
                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");
                    streamContent.Headers.ContentDisposition.FileName = Path.GetFileName(PathFileName);
                    streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/form-data");
                    streamContent.Headers.ContentDisposition.Name = "file";
                    streamContent.Headers.ContentDisposition.Size = fs.Length;

                    streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/jpeg");
                    multipartFormDataContent.Add(streamContent); 
                    HttpResponseMessage response = client.PostAsync(UploadPersonPictureApi, multipartFormDataContent).Result;
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    response.Dispose();
                    client.Dispose();
                    fs.Flush();
                    fs.Close();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        responseBody = responseBody.TrimEnd('\n');
                        responseBody = responseBody.TrimEnd('\r');
                    } 
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    string ErrorStr = string.Format("{0}-{1}[BACKEND SERVER IS CLOSED] [{2}]", Lang.GeneralUI_Fail, e.Message, UploadPersonPictureApi);
                    LogHelper.Error(ErrorStr);
                    return string.Empty;
                }
            }
        } 

        [Route("Files/download/{subPath}/{id}")] // format: http://localhost:5002/Files/download/Person/202007/12152220123
        [HttpGet]   
        public FileContentResult Get(string subPath,long id)
        {
            string uploadFolder = _uploadSetting.Value.TargetFolder;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(id);

            string monthFolder = string.Format("{0:yyyyMM}", dateTimeOffset.UtcDateTime);
            string targetPath = Path.Combine(wwwRootPath, uploadFolder, monthFolder);
            if(!string.IsNullOrEmpty(targetPath))
            {
                targetPath = Path.Combine(wwwRootPath, uploadFolder, subPath, monthFolder);
            }
            string pahtFileName = string.Format("{0}\\{1}.jpg", targetPath, id);
              
            if (!System.IO.File.Exists(pahtFileName)) // case: NOT EXISTS ::  NoThisPicture.jpg
            {
                pahtFileName = Path.Combine(webHostEnvironment.ContentRootPath, "NoThisPicture.jpg");
            }
            lock (lockObj)
            {
                FileStream fs = new FileStream(pahtFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);

                Microsoft.Net.Http.Headers.MediaTypeHeaderValue mediaTypeHeaderValue = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                //关闭读写流和文件流
                sr.Close();
                fs.Close();
                return new FileContentResult(bytes, mediaTypeHeaderValue); 
            }
        }
    }
}
