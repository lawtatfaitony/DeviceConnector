using System.Threading.Tasks;
using Common;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using X.PagedList;
using VxGuardClient.Context; 
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using VideoGuard.ApiModels.ApiModels;
using VxGuardClient.ModelView;
using System.Drawing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using LogUtility;
using System.Collections;

namespace VxGuardClient.Controllers
{
    public class UtilityController : BaseController
    {
        private static readonly string[]  subFolderLimited= PersonBusiness.SubFolderLimited;  //{ "Person", "CropPerson", "Mpeg" };
        private IOptions<UploadSetting> _uploadSetting;
        private string HttpHost;
        private string wwwRootPath;
        public UtilityController(IAuthenticateService service, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IOptions<UploadSetting> uploadSetting)
              : base(webHostEnvironment, httpContextAccessor)
        {
            wwwRootPath = webHostEnvironment.WebRootPath;
            _uploadSetting = uploadSetting;
            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
        }

        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("{Language}/[controller]/[action]")]
        [HttpPost]
        public IActionResult UploadProcess(string subpath,IFormFile file)
        {
            ResponseModalX responseModalX = new ResponseModalX();
            if(string.IsNullOrEmpty(subpath))
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_FAIL, Message = "No Explicit SubPath" }
                };
                return Ok(responseModalX);
            }
            else
            {
                if (((IList)subFolderLimited).Contains(subpath)==false)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_FAIL, Message = "No Explicit SubPath" }
                    };
                    return Ok(responseModalX);
                }
            }
           

            PictureUploadRet pictureUploadRet = new PictureUploadRet { PicUrl = "", PicClientUrl = "" };
            UserFile userFile = new UserFile(file);

            
            string uploadFolder = _uploadSetting.Value.TargetFolder;
            long fileSizeLimit = _uploadSetting.Value.FileSizeLimit;
            if (fileSizeLimit < userFile.Length)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILESIZE_IS_LIMITED, Message = Lang.FILESIZE_IS_LIMITED }
                };
                return Ok(responseModalX);
            }
            string monthFolder = string.Format("{0:yyyyMM}", DateTime.Now);

            string targetPath = Path.Combine(wwwRootPath, uploadFolder, subpath, monthFolder);
            string tagetUrlPath = $"/{uploadFolder}/{subpath}/{monthFolder}/";
            responseModalX = userFile.SaveAs(targetPath).Result;
            string picClientUrl = responseModalX.data.ToString().Trim();

            pictureUploadRet.PicClientUrl = picClientUrl;

            string[] pathStructArr = picClientUrl.Split("/");
            string retFileName = string.Format("{0}.jpg", pathStructArr[pathStructArr.Length - 1]);
            string sourcePathOfPicClientUrl = Path.Combine(targetPath, retFileName);
             
            string picUrl = picClientUrl;
            pictureUploadRet.PicUrl = picUrl;  //picUrl 和 picClientUrl 一样 2022年6月18日

            string picClientUrlAbsolutePath = $"{tagetUrlPath}{retFileName}";  
            string picUrlAbsolutePath = picClientUrlAbsolutePath;

            return Ok(new
            {
                picClientUrl,
                picClientUrlAbsolutePath,
                picUrl,
                picUrlAbsolutePath
            }); 
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
    }
}
