using Common;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using System;
using System.Linq;
using VideoGuard.ApiModels;

namespace VideoGuard.Business
{
    public class CamMpegBusiness
    {
        /// <summary>
        /// 判断是否相同的文件名称存在
        /// </summary>
        /// <param name="MpegFilename"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool IsTheSameOfMpegFilename(int deviceId, string MpegFilename, ref ResponseModalX responseModalX)
        {
            if (string.IsNullOrEmpty(MpegFilename))
            {
                string loggerline = $"[FUNC::CamMpegBusiness.IsTheSameOfMpegFilename][DeviceId = {deviceId}][MpegFilename={MpegFilename} {Lang.CAMMPEG_THE_MPEG_FILENAME_IS_EMPTY}]";
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);

                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CAMMPEG_THE_MPEG_FILENAME_IS_EMPTY, Success = false, Message = Lang.CAMMPEG_THE_MPEG_FILENAME_IS_EMPTY },
                    data = null
                };
                return true;
            }

            using BusinessContext businessContext = new BusinessContext();

            FtCameraMpeg ftCameraMpeg = businessContext.FtCameraMpeg.Where(c => c.DeviceId == deviceId && c.MpegFilename.Contains(MpegFilename)).FirstOrDefault();

            if (ftCameraMpeg != null)
            {
                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CamMpeg_EXIST_THE_NAME, Success = false, Message = $"{ Lang.CamMpeg_EXIST_THE_NAME}:({deviceId}{MpegFilename})" },
                    data = null
                };
                return true;
            }
            else
            {
                responseModalX = new ResponseModalX();
                return false;
            }
        }

        /// <summary>
        /// 增加上存mpeg文件信息记录
        /// </summary>
        /// <param name="MpegFilename"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool AddCameraMpeg(FtCameraMpeg cameraMpeg, ref ResponseModalX responseModalX)
        {
            using BusinessContext businessContext = new BusinessContext();
            string loggerline;
            var recordePKey = businessContext.FtCameraMpeg.Select(s => new { s.DeviceId, s.Id }).Where(c => c.DeviceId == cameraMpeg.DeviceId && c.Id == cameraMpeg.Id).FirstOrDefault();
            if (recordePKey != null) //使用传入的ID 和DeviceId 作主键 如果主鍵(DvrId) + RecordId  已經存在,則返回成功標記.
            {
                loggerline = $"[FUNC::CamMpegBusiness.AddCameraMpeg][DeviceId = {cameraMpeg.DeviceId}][Id = {cameraMpeg.Id}][{Lang.GeneralUI_ExistRecord}]";
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.GENERALUI_EXIST_RECORD, Message = Lang.GeneralUI_ExistRecord };
                responseModalX.data = recordePKey;
                return true;
                //以上對於存在記錄的,也返回success = true / return true : 以應對由於之前超時應答導致,循環post記錄上雲端
            }

            try
            {
                businessContext.FtCameraMpeg.Add(cameraMpeg);
                bool result = businessContext.SaveChanges() > 0;

                if (result)
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = Lang.GeneralUI_SUCC, Success = true },
                        data = cameraMpeg
                    };
                    return true;
                }
                else
                {
                    responseModalX = new ResponseModalX
                    {
                        meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CamMpeg_ADD_FAIL, Success = false, Message = Lang.CamMpeg_ADD_FAIL },
                        data = null
                    };
                    return false;
                }
            }
            catch (Exception ex)
            {
                loggerline = $"[FUNC::CamMpegBusiness.AddCameraMpeg][DATABASE INERT][PLEASE CHECK PRIMARY DUPLICATE [Id(RecordId)={cameraMpeg.Id}+DeviceId(DvrId) {cameraMpeg.DeviceId}][EXCEPTION::{ex.Message}]";
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]{loggerline}");
                CommonBase.OperateDateLoger(loggerline, CommonBase.LoggerMode.FATAL);

                responseModalX = new ResponseModalX
                {
                    meta = new MetaModalX { ErrorCode = (int)CamMpegErrorCode.CamMpeg_ADD_FAIL, Success = false, Message = $"[{Lang.CamMpeg_ADD_FAIL}][PLEASE CHECK PRIMARY DUPLICATE [Id(RecordId)={cameraMpeg.Id}+DeviceId(DvrId) {cameraMpeg.DeviceId}][EXCEPTION::{ex.Message}]" },
                    data = null
                };
                return false;
            }
        }
    }

    /// <summary>
    ///  c.Id, c.DeviceId, c.CameraId, c.MpegFilename, c.FileFomat, c.IsGroup, c.StartTimestamp, c.EndTimestamp, c.Visible 
    /// </summary>
    public partial class CameraMpeg
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public int CameraId { get; set; }
        public string MpegFilename { get; set; }
        public string FileFomat { get; set; }
        public ulong IsGroup { get; set; }
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public sbyte Visible { get; set; }
        public sbyte IsUpload { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
