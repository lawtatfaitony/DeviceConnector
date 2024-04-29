using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnumCode; 
using Common;
using DataBaseBusiness.ModelHistory;
using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoGuard.ApiModels;
using VxGuardClient;
using VxGuardClient.Controllers;
using VideoGuard.Business;
using VideoGuard.ApiModels.ApiModels;
using DataBaseBusiness.Models;
using VxClient.Models;
namespace DataGuardXcore.Controllers
{
    /// <summary>
    /// 来自 考勤系统移植过来的 手机hik人脸同步app和桌面hik同步机软件
    /// </summary>
    public class UpFileController : BaseController
    { 
        private string HttpHost { get; set; }
        private UploadSetting _UploadSetting { get; set; }
        private string _SubFolderEntries { get; set; } = "EntriesLogImages";
        private string _VideoFolderEntries { get; set; } = "Video";
        private IWebHostEnvironment WebHostEnvironment { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        private static ILogger<BaseController> _Logger { get; set; }
        public UpFileController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, ILogger<BaseController> logger, IOptions<UploadSetting> uploadSetting)
            : base(webHostEnvironment, httpContextAccessor)
        {
            WebHostEnvironment = webHostEnvironment;
            HttpContextAccessor = httpContextAccessor;

            HttpRequest httpRequest = httpContextAccessor.HttpContext.Request;
            HttpHost = $"{httpRequest.Scheme}://{httpRequest.Host.Host}:{httpRequest.Host.Port}";
            _UploadSetting = uploadSetting.Value;
            string targetPath = Path.Combine(WebHostEnvironment.WebRootPath, _UploadSetting.TargetFolder, _SubFolderEntries);
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string targetVideoPath = Path.Combine(WebHostEnvironment.WebRootPath, _UploadSetting.TargetFolder, _VideoFolderEntries);
            if (!Directory.Exists(targetVideoPath))
            {
                Directory.CreateDirectory(targetVideoPath);
            }

            _Logger = logger;
            Logger = logger;
        }

        /// <summary>
        /// 识别上存数据包括图片 从DataGuardXcore转换过来的
        /// ONLY ACCEPT :'video/mp4' UPLOAD 
        /// 路徑和格式說明
        /// 默認統一的媒體路徑位置: Upload/Video/{DeviceId}/{CameraId}/{2023-12-23}/{1657263394000}.mp4 
        /// 如果沒有CameraId(設備本身兼有的功能,如識別機),則去掉此文件夾,直接保存在設備下:Upload/Video/{DeviceId}/{2023-12-23}/{1657263394000}.mp4 
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <param name="attendanceLogId">對應本系統的表[hist_recognize_record].[Id]</param>
        /// <param name="upFileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult UploadEntries(string mainComId,string deviceId,string deviceSerialNo, string attendanceLogId, string upFileName, IFormFile file)
        { 
            UserFile userFile = new UserFile(file);

            string logline = $"[FUNC::UpFile/UploadEntries][AttendanceLogId][{attendanceLogId}][FILE LENGHT][{userFile.Length}][Extension = {userFile.Extension}][{userFile.FileName}]";

#if DEBUG
            CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
#endif            
            Logger.LogInformation(logline);

            //-----------------------------------------------------------------------------------------
            ResponseModalX responseModalX = new ResponseModalX();
            //Check MainComId
            if (string.IsNullOrEmpty(deviceId))
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][NO DEVICE ID][{Lang.GeneralUI_NoParms}] [{mainComId}]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.GeneralUI_NoParms} [MainComId]" };
                return Ok(responseModalX);
            }
             
            using BusinessContext businessContext = new BusinessContext();
            using HistoryContext historyContext = new HistoryContext();

            if(int.TryParse(deviceId,out int intDeviceId)==false)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][{Lang.Device_IlleggleDeviceId}] [deviceId={deviceId}]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.ILLEGAL_DEVICE_ID, Message = $"[UploadEntries]{Lang.Device_IlleggleDeviceId}[deviceId={deviceId}]" };
                return Ok(responseModalX);
            }

            FtDevice device = businessContext.FtDevice.Find(intDeviceId);
            if(device==null)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][NO DEVICE][NULL][{deviceId}] [{deviceSerialNo}]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.GeneralUI_Device}{Lang.GeneralUI_NoRecord} [{deviceId}]" };
                return Ok(responseModalX);
            }
            else if(device.DeviceSerialNo!= deviceSerialNo)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][DEVICE SERIAL NO NOT EQUAL][NULL][DeviceSerialNo From DB][{device.DeviceSerialNo}] [{deviceSerialNo}][FROM POST]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.ILLEGAL_DEVICE_SERIAL_NUMBER} [{deviceSerialNo}]" };
                return Ok(responseModalX);
            }

            if(string.IsNullOrEmpty(mainComId))
            {
                CommonBase.OperateDateLoger($"[REQUIRED_CORRECT_PARMS_MAINCOM_ID][MAINCOM ID NULL][{GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetDescriptionX()}] ", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = $"{Lang.GeneralUI_MainComIdRequired}[MainComId={mainComId}]" };
                return Ok(responseModalX);
            }
            if (device.MaincomId != mainComId)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][REQUIRED_CORRECT_PARMS_MAINCOM_ID][MAINCOM ID NULL][{device.MaincomId}!={mainComId}][{Lang.GeneralUI_NoMatchMainComId}] ", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.NO_MATCH_MAINCOMID, Message = Lang.GeneralUI_NoMatchMainComId };
                return Ok(responseModalX);
            }
            string fileName = string.Format("{0}.{1}", attendanceLogId, "jpg");
            bool parseAttId = long.TryParse(attendanceLogId, out long lAttendanceLogId);
            var attExist = historyContext.HistRecognizeRecord.Find(lAttendanceLogId);
            if (parseAttId && attExist != null)
                HistRecBusiness.UpdCatchPictureFileName(lAttendanceLogId, fileName); //update to the  field ::CatchPictureFileName
            else
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile.UploadEntries][{attendanceLogId}][{Lang.GENERALUI_NO_HISTORY_RECORD}]");
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_RECORD, Message = Lang.GENERALUI_NO_HISTORY_RECORD };
                return Ok(responseModalX);
            }

            //Check FileSize 
            long fileSizeLimit = _UploadSetting.FileSizeLimit;
            if (fileSizeLimit < userFile.Length)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILESIZE_IS_LIMITED, Message = Lang.FILESIZE_IS_LIMITED }
                };
                return Ok(responseModalX);
            }
             
            string uploadFolder = _UploadSetting.TargetFolder;
            string monthFolder = string.Format("{0:yyyyMM}", DateTime.Now);
            string targetPath = Path.Combine(WebHostEnvironment.WebRootPath, uploadFolder, _SubFolderEntries, monthFolder); 
            if(!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
             
            string pathFileName = Path.Combine(targetPath, fileName); 

            if(System.IO.File.Exists(pathFileName))
            {
                //已经存在文件，返回成功，不处理保存
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_EXISTS, Message = $"{Lang.FILE_UPLOAD_EXISTS}_{fileName}" };
                return Ok(responseModalX);
            }

            string fileType =file.ContentType;
            
            if (fileType== "image/bmp")
            {
                Bitmap bt = new Bitmap(file.OpenReadStream());
                bt.Save(pathFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                bt.Dispose();
            }
            else  //bmp or jpg , other not allow
            {
                if (System.IO.File.Exists(pathFileName))
                {
                    string existsFileLog = $"][FUNC::UpFileController.UploadEntries][{Lang.FILE_UPLOAD_EXISTS}_{pathFileName}]";
                    Logger.LogInformation(existsFileLog);
                    //已经存在文件，返回成功，不处理保存
                    responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_EXISTS, Message = $"{Lang.FILE_UPLOAD_EXISTS}_{pathFileName}" };
                    return Ok(responseModalX);
                }
                using FileStream fs = new FileStream(pathFileName, FileMode.OpenOrCreate);
                file.CopyTo(fs);
                fs.Flush();
                fs.Close();
            } 
            string PicClientUrl = string.Format("{0}/{1}/{2}/{3}/{4}", HttpHost, uploadFolder, _SubFolderEntries, monthFolder, fileName);
           
            string dThumbnailFile = pathFileName + Common.PictureSize.s60X60.ToString() +  ".jpg";
            bool thumbnail_ok = GetPicThumbnail(pathFileName, dThumbnailFile, 60, 60, 100);

            string loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][SUCCESS][FUNC::UpFileController.UploadEntries][PicClientUrl={PicClientUrl}]";
            Logger.LogInformation(loggerline);

            responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_SUCC} [ThumbnailFile = {thumbnail_ok}]" };
            responseModalX.data = PicClientUrl;
            return Ok(responseModalX); 
        }

        /// <summary>
        /// 仅For 桌面海康设备同步用 EVENT DEPLOY 和手機版的HIK SYNC 都是用這個API
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <param name="attendanceLogId">來自DGX系統中的表AttendanceLog的主鍵ID對應就是表hist_recognize_record的主鍵ID </param>
        /// <param name="upFileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{Language}/[controller]/[action]/{mainComId}/{deviceId}/{deviceSerialNo}/{attendanceLogId}/{upFileName}")] 
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult UploadEntriesTemp(string mainComId, string deviceId, string deviceSerialNo, string attendanceLogId, string upFileName, IFormFile file)
        {
            string loggerline;
            try
            { 
                loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::UpFileController.UploadEntriesTemp][MainComId={mainComId}][DeviceId={deviceId}][DeviceSerialNo={deviceSerialNo}][AttendanceLogId={attendanceLogId}][FileName={upFileName}]";
                Logger.LogWarning(loggerline);
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::DeviceController.UploadEntriesTemp][MainComId={mainComId}][DeviceId={deviceId}][DeviceSerialNo={deviceSerialNo}][AttendanceLogId={attendanceLogId}][FileName={upFileName}][EXCEPTION]\n{ex.Message}";
                Logger.LogError(loggerline);
                CommonBase.OperateDateLoger($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][{loggerline}][{ex.Message}]");
            }

            UserFile userFile = new UserFile(file);

            CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntries][UploadEntriesTemp][{attendanceLogId}][FILE LENGHT][{userFile.Length}][Extension = {userFile.Extension}][{userFile.FileName}]", CommonBase.LoggerMode.INFO);

            ResponseModalX responseModalX = new ResponseModalX();
            //Check MainComId
            if (string.IsNullOrEmpty(deviceId)||int.TryParse(deviceId,out int intDeviceId) == false)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntriesTemp][NO DEVICE ID]/[{Lang.DEVICE_DEVICE_ID_UNFORMAT}] [{deviceId}]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.GeneralUI_NoParms} [deviceId]" };
                return Ok(responseModalX);
            }

            using BusinessContext businessContext = new BusinessContext();
             
            FtDevice device = businessContext.FtDevice.Find(intDeviceId);

            if (device == null)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntriesTemp][NO DEVICE][NULL][{deviceId}] [{deviceSerialNo}]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.GeneralUI_Device}{Lang.GeneralUI_NoRecord} [{deviceId}]" };
                return Ok(responseModalX);
            }
            else if (device.DeviceSerialNo != deviceSerialNo)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntriesTemp][DEVICE SERIAL NO NOT EQUAL][NULL][DeviceSerialNo From DB][{device.DeviceSerialNo}] [{deviceSerialNo}][FROM POST]", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"{Lang.Device_DeviceSerialNo} [{deviceSerialNo}]" };
                return Ok(responseModalX);
            }

            if (string.IsNullOrEmpty(mainComId))
            {
                CommonBase.OperateDateLoger($"[REQUIRED_CORRECT_PARMS_MAINCOM_ID][MAINCOM ID NULL][{Lang.GeneralUI_MainComIdRequired }] ", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = $"{Lang.GeneralUI_MainComIdRequired}[MainComId={mainComId}]" };
                return Ok(responseModalX);
            }
            if (device.MaincomId != mainComId)
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile/UploadEntriesTemp][REQUIRED_CORRECT_PARMS_MAINCOM_ID][MAINCOM ID NULL][{device.MaincomId}!={mainComId}][{GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetDescriptionX()}] ", CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID, Message = GeneralReturnCode.REQUIRED_CORRECT_PARMS_MAINCOM_ID.GetDescriptionX() };
                return Ok(responseModalX);
            }
            string fileName = string.Format("{0}.{1}", attendanceLogId, "jpg");
            bool parseAttId = long.TryParse(attendanceLogId, out long lAttendanceLogId);

            using HistoryContext historyContext = new HistoryContext();

            var attExist = historyContext.HistRecognizeRecord.Find(lAttendanceLogId);
            if (parseAttId && attExist != null)
                HistRecBusiness.UpdCatchPictureFileName(lAttendanceLogId, fileName); //update to the  field ::CatchPictureFileName
            else
            {
                CommonBase.OperateDateLoger($"[FUNC::UpFile.UploadEntriesTemp][{attendanceLogId}][{Lang.GENERALUI_NO_DEV_POST_RECORD}]");
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.GENERALUI_NO_DEV_POST_RECORD, Message = $"{GeneralReturnCode.GENERALUI_NO_DEV_POST_RECORD.GetDescriptionX()} [Table::hist_recognize_record:{Lang.GeneralUI_NoRecord}]" };
                return Ok(responseModalX);
            }

            //Check FileSize 
            long fileSizeLimit = _UploadSetting.FileSizeLimit;
            if (fileSizeLimit < userFile.Length)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILESIZE_IS_LIMITED, Message = Lang.FILESIZE_IS_LIMITED }
                };
                return Ok(responseModalX);
            }

            string uploadFolder = _UploadSetting.TargetFolder;
            string monthFolder = string.Format("{0:yyyyMM}", DateTime.Now);
            string targetPath = Path.Combine(WebHostEnvironment.WebRootPath, uploadFolder, _SubFolderEntries, monthFolder );
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            string fileNameExcludeExtension = Path.GetFileNameWithoutExtension(upFileName);

            string pathFileName = Path.Combine(targetPath, fileName);

            if (System.IO.File.Exists(pathFileName))
            {
                //已经存在文件，返回成功，不处理保存
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_EXISTS, Message = $"{Lang.FILE_UPLOAD_EXISTS}_{fileName}" };
                return Ok(responseModalX);
            }

            string fileType = file.ContentType;

            if (fileType == "image/bmp")
            {
                Bitmap bt = new Bitmap(file.OpenReadStream());
                bt.Save(pathFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                bt.Dispose();
            }
            else  //bmp or jpg , other not allow
            {
                using (FileStream fs = new FileStream(pathFileName, FileMode.OpenOrCreate))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
            }
             
            string PicClientUrl = $"{HttpHost}/{uploadFolder}/{_SubFolderEntries}/{monthFolder}/{fileName}";

            string dThumbnailFile = pathFileName + Common.PictureSize.s60X60.ToString() + ".jpg";
            bool thumbnail_ok = GetPicThumbnail(pathFileName, dThumbnailFile, 60, 60, 100);

            loggerline = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][SUCCESS][FUNC::UpFileController.UploadEntriesTemp][PicClientUrl={PicClientUrl}]";
            Logger.LogInformation(loggerline);

            responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.FILE_UPLOAD_SUCCESS} [ThumbnailFile = {thumbnail_ok}]" };
            responseModalX.data = PicClientUrl;
            return Ok(responseModalX);
        }

        /// <summary>
        /// 压缩 Thumbnail
        /// </summary>
        /// <param name="sFile"></param>
        /// <param name="dFile"></param>
        /// <param name="dHeight"></param>
        /// <param name="dWidth"></param>
        /// <param name="flag">flag;//设置压缩的比例1-100 </param>
        /// <returns></returns>
        public static bool GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            _Logger.LogInformation($"[FUNC::UpFile.GetPicThumbnail][sFile][{sFile}] [Destination dFile][{dFile}]");

            using FileStream fileStream1 = new System.IO.FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            try
            {
                using var iSource = Image.FromStream(fileStream1, true);
                _Logger.LogInformation($"[FUNC::UpFile.GetPicThumbnail.iSource][iSource.Size]=[{iSource.Size}Bytes]");

                ImageFormat tFormat = iSource.RawFormat;
                int sW = 0, sH = 0;
                Bitmap ob;
                //按比例缩放
                Size tem_size = new Size(iSource.Width, iSource.Height);
                if (tem_size.Width > dHeight || tem_size.Height > dWidth) //将**改成c#中的或者操作符号
                {
                    if ((tem_size.Width * dHeight) > (tem_size.Height * dWidth))
                    {
                        sW = dWidth;
                        sH = (dWidth * tem_size.Height) / tem_size.Width;
                        ob = new Bitmap(sW, sH);
                    }
                    else
                    {
                        sH = dHeight;
                        sW = (dHeight * tem_size.Width) / tem_size.Height;
                        ob = new Bitmap(sW, sH);
                    }
                }
                else
                {
                    sW = tem_size.Width;
                    sH = tem_size.Height;
                    ob = new Bitmap(sW, sH);
                }

                Graphics g = Graphics.FromImage(ob);
                g.Clear(Color.White); //g.Clear(Color.Transparent); 透明
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(iSource, new Rectangle(0, 0, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
                g.Dispose();
                //以下代码为保存图片时，设置压缩质量 
                EncoderParameters ep = new EncoderParameters();
                long[] qy = new long[1];
                qy[0] = flag;//设置压缩的比例1-100 
                EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                ep.Param[0] = eParam;
                try
                {
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICIinfo = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICIinfo = arrayICI[x];
                            break;
                        }
                    }
                    if (jpegICIinfo != null)
                    {
                        ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径 
                    }
                    else
                    {
                        ob.Save(dFile, tFormat);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    CommonBase.OperateDateLoger($"[FUNC::UpFile.GetPicThumbnail][{dFile}][ThumbNail][EXCEPTION][{e.Message}][SOURCE][{sFile}]");
                    return false;
                }
                finally
                {
                    iSource.Dispose();
                    ob.Dispose();
                }
            }
            catch (Exception ex)
            {
                _Logger.LogInformation($"[FUNC::UpFile.GetPicThumbnail][sFile][{sFile}] [Destination dFile][{dFile}]");
                CommonBase.OperateDateLoger($"[FUNC::UpFile.GetPicThumbnail] [EXCEPTION] [{ex.Message}][MAYBE:File format is corrupted][{sFile}]");
                return false;
            }
            finally
            {
                fileStream1.Dispose();
            }
        }
        public static ImageRetangleSize GetImageRetangleSize(string sFile)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            Size tem_size = new Size(iSource.Width, iSource.Height);
            ImageRetangleSize imageRetangleSize = new ImageRetangleSize { Width = tem_size.Width, Height = tem_size.Height };
            iSource.Dispose();
            return imageRetangleSize;
        }
        public class ImageRetangleSize
        {
            public int Width;
            public int Height;
        }

        /// <summary>
        /// ONLY ACCEPT :'video/mp4' UPLOAD ,非鏡頭為設備的情況下上存到這裡
        /// 而DVR中定義的鏡頭為設備的概念不在這裡上存,需要按照DVR特定的路徑格式上存如:D:\media\record\0A\17\6204837569767.mp4 => DvrId(DeviceId)\media\record\0A\17\6204837569767.mp4
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceSerialNo"></param>
        /// <param name="startTimeStamp"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [HttpPost]
        [Route("{Language}/[controller]/[action]")]
        [RequestSizeLimit(1_200_000_000)]  //大於1Gb=1_074_790_400,但說明最大限制 1,200,000,000.0 Bytes (B)=1.11759 Gigabytes (GB)
        public IActionResult UploadVideo(string deviceId,string deviceSerialNo, string startTimeStamp,  IFormFile file)
        {
            UserFile userFile = new UserFile(file);
            string logline = $"[MP4]-[FUNC::UpFile/UploadVideo][deviceId][{deviceId}][FILE LENGHT][{userFile.Length}][Extension = {userFile.Extension}][{userFile.FileName}]";
            Logger.LogInformation(logline, CommonBase.LoggerMode.INFO);

            ResponseModalX responseModalX = new ResponseModalX();
             
            //CHECK DEVICE ID
            if (string.IsNullOrEmpty(deviceId))
            {
                logline = $"[FUNC::UpFile/UploadVideo][REQUIRED DEVICE ID IS INT: {deviceId}]";
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = logline };
                return Ok(responseModalX);
            }
            if (int.TryParse(deviceId, out int intDeviceId))
            {
                logline = $"[FUNC::UpFile/UploadVideo][{Lang.Device_IlleggleDeviceId}][REQUIRED DEVICE ID IS INT: {deviceId}]";
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = logline };
                return Ok(responseModalX);
            }
            using BusinessContext businessContext = new BusinessContext();
            
            FtDevice device = businessContext.FtDevice.Find(intDeviceId);
            if (device == null)
            {
                logline = $"[FUNC::UpFile/UploadVideo][NO DEVICE][NULL][{deviceId}] [{deviceSerialNo}]";
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)DeviceErrorCode.DEVICE_NOT_EXIST, Message = $"{Lang.GeneralUI_Device}{Lang.GeneralUI_NoRecord}" };
                return Ok(responseModalX);
            }
            else if (device.DeviceSerialNo != deviceSerialNo)
            {
                logline = $"[FUNC::UpFile/UploadVideo][DEVICE SERIAL NO NOT EQUAL][NULL][DeviceSerialNo From DB][{device.DeviceSerialNo}] [{deviceSerialNo}][FROM POST]";
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FAIL, Message = $"[NOT VALID DEVICE SERIAL NO] [{deviceSerialNo}]" };
                return Ok(responseModalX);
            }

            string deviceVideoPath = Path.Combine(WebHostEnvironment.WebRootPath, _UploadSetting.TargetFolder, device.MaincomId, _VideoFolderEntries, device.DeviceId.ToString());
            if (!Directory.Exists(deviceVideoPath))
            {
                Directory.CreateDirectory(deviceVideoPath);
            }

            string fileName = string.Format("{0}{1}", startTimeStamp, userFile.Extension);

            string deviceVideoPathFileName = Path.Combine(deviceVideoPath, fileName);
            
            if (System.IO.File.Exists(deviceVideoPathFileName))
            {
                logline = $"[FUNC::UpFile/UploadVideo][FILE EXIST][{device.DeviceId}] [{fileName}]";
                Logger.LogInformation(logline);
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.INFO); 
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = logline };
                return Ok(responseModalX);
            }

            string fileType = file.ContentType;

            if (fileType != "video/mp4")
            {
                logline = $"[FUNC::UpFile/UploadVideo][FILE TYPE NOT ACCEPT][{device.DeviceId}] [{file.FileName}][{fileType}]";
                Logger.LogInformation(logline);
                CommonBase.OperateDateLoger(logline, CommonBase.LoggerMode.WARNNING);
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILE_UPLOAD_FAIL, Message = logline };
                return Ok(responseModalX);
            }

            //Check FileSize 
            long fileSizeLimit = _UploadSetting.FileSizeLimit;
            if (fileSizeLimit < userFile.Length)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.FILESIZE_IS_LIMITED, Message = Lang.FILESIZE_IS_LIMITED }
                };
                return Ok(responseModalX);
            }

            using (FileStream fs = new FileStream(deviceVideoPathFileName, FileMode.OpenOrCreate))
            {
                file.CopyTo(fs);
                fs.Flush();
                fs.Close();
            }

            DateTime start = DateTime.Now;

            if(long.TryParse(startTimeStamp,out long lStart))
            {
                start = DateTimeHelp.ConvertToDateTime(lStart);
            }

            string videoUrl = $"{HttpHost}/Files/Video/{deviceId}/{userFile.Extension.Remove(0, 1)}/{startTimeStamp}";

            //数据记录 存入 表 [ft_camera_mpeg]
            DateTime dt = DateTime.Now;
            FtCameraMpeg ftCameraMpeg = new FtCameraMpeg
            {
                Id = 0,
                DeviceId = device.DeviceId,
                CameraId = 0, //设备本身就是camera的情况下 CameraId=0
                MpegFilename = string.Format("{0}.mp4", startTimeStamp),
                IsGroup =Convert.ToByte(false),
                GroupTimestamp = 0,
                StartTimestamp = lStart,
                EndTimestamp = lStart + 10*60*1000, //由于缺乏传入的结束时长,只能是默认十分钟时长.
                Visible = (int)CamMpegErrorCode.CamMpeg_VISIBLE,
                IsFormatVirified = 0,
                IsUpload = 1, //已经上存
                CreateTime = dt
            };
            bool CameraMpegTableRecord = CamMpegBusiness.AddCameraMpeg(ftCameraMpeg, ref responseModalX);
             
            responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"{Lang.GeneralUI_SUCC} [TABLE RECORD={CameraMpegTableRecord}] " };
            responseModalX.data = new { videoUrl, startTimeStamp, CameraMpegTableRecord };
            return Ok(responseModalX);
        } 
    }
}

//安卓java的API URL
//public static String getAttendancePostUrl()
//{
//    return String.format("/%s/Admin/DeviceManage/AttendancePost", LanguageUtil.getDefaultLanguageCode(HikApplication.getInstance()));
//}

//public static String getUploadFileUrl()
//{
//    return String.format("/%s/UpFile/UploadEntries", LanguageUtil.getDefaultLanguageCode(HikApplication.getInstance()));
//}

//public static String getLoginUrl()
//{
//    return "/Authentication/RequestToken";
//}

//public static String getRefreshTokenUrl()
//{
//    return "/Authentication/RefreshToken";
//}

//public static String getUploadVideoUrl()
//{
//    return String.format("/%s/UpFile/UploadVideo", LanguageUtil.getDefaultLanguageCode(HikApplication.getInstance()));
//}