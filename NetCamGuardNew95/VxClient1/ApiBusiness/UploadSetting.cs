using EnumCode;
using LanguageResource;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoGuard.ApiModels;

namespace VxGuardClient
{  
    public class UploadSetting
    {
        [JsonProperty("webContentRoot")]
        public string WebContentRoot { get; set; }

        [JsonProperty("uploadServerIp")]
        public string UploadServerIp { get; set; }

        [JsonProperty("uploadServerPort")]
        public string UploadServerPort { get; set; }

        [JsonProperty("targetFolder")]
        public string TargetFolder { get; set; }

        [JsonProperty("fileSizeLimit")]
        public long FileSizeLimit { get; set; } 
    }

    public class IUploadSetting
    {
        [JsonProperty("webContentRoot")]
        public string WebContentRoot { get; set; }

        [JsonProperty("uploadServerIp")]
        public string UploadServerIp { get; set; }

        [JsonProperty("uploadServerPort")]
        public string UploadServerPort { get; set; }

        [JsonProperty("targetFolder")]
        public string TargetFolder { get; set; }

        [JsonProperty("fileSizeLimit")]
        public long FileSizeLimit { get; set; }
    }
    /// <summary>
    /// 在上传表单中，我们定义了附件的名称为 file 对应绑定模型的公共属性 File
    /// In the upload form, we define the name of the attachment as file, which corresponds to the public property of the binding model, File
    /// </summary>
    public class UserFile
    {
        public UserFile(IFormFile ifile)
        {
            File = ifile;
        }
        public string FileName { get; set; }

        public long Length { get; set; }

        public string Extension { get; set; }

        public string FileType { get; set; }

        private readonly static string[] Filters = { ".jpg", ".png", ".bmp" };

        public bool IsValid => !string.IsNullOrEmpty(this.Extension) && Filters.Contains(this.Extension);

        private IFormFile file;

        public IFormFile File
        {

            get { 
                return file; 
            }

            set
            {
                if (value != null)
                {
                    this.file = value;
                    this.FileName = this.file.FileName;
                    this.FileType = this.file.ContentType;

                    this.Length = this.file.Length;

                    this.Extension = this.file.FileName.Substring(file.FileName.LastIndexOf('.'));

                    if (string.IsNullOrEmpty(this.FileName))

                        this.FileName = this.FileName;
                }
            }
        }
        public async Task<ResponseModalX> SaveAs(string targetPath)
        {
            ResponseModalX responseModalX = new ResponseModalX { 
                meta= new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_FAIL,Message = Lang.FILE_UPLOAD_SUCCESS }
            };

            if (this.file == null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_FAIL, Message = Lang.FILE_UPLOAD_FAIL }
                };
                return responseModalX;
            }
            string[] pathStructArr = targetPath.Split("\\");
            string subPath = pathStructArr[pathStructArr.Length - 2];
            if (!string.IsNullOrEmpty(targetPath) && !Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            var newName = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            var newfileName = $"{newName}{this.Extension}";
             
            var newFile = Path.Combine(targetPath, newfileName);
            
            using (FileStream fs = new FileStream(newFile, FileMode.CreateNew))
            { 
                await this.file.CopyToAsync(fs);
                fs.Flush();
                fs.Close();
                responseModalX.data = string.Format("/Files/download/{0}/{1}", subPath, newName);  //Format: /Files/download/20200712152220123
                return responseModalX;
            } 
        }
    }

}
