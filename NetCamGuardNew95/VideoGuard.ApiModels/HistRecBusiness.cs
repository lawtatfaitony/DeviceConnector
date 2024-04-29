using Common;
using DataBaseBusiness.ModelHistory;
using DataBaseBusiness.Models;
using EnumCode;
using LanguageResource;
using System;
using System.Linq;
using VideoGuard.ApiModels;
using HistRecognizeRecord = DataBaseBusiness.ModelHistory.HistRecognizeRecord;

namespace VideoGuard.Business
{
    public class HistRecBusiness
    {
        public static bool UpdCatchPictureFileName(long attendanceLogId, string fileName)
        {
            long id = attendanceLogId; //由DataGuard X core 系統轉換過來的函數,所以相應改動對應 表[HistRecognizeRecord]
            using (HistoryContext historyContext = new HistoryContext())
            {
                var histRec = historyContext.HistRecognizeRecord.Find(id);

                histRec.CapturePath = fileName;

                historyContext.HistRecognizeRecord.Update(histRec);
                bool result = historyContext.SaveChanges() > 0;
                return result;
            }
        }

        /// <summary>
        /// For DataGuardXcore 系統移植(FUNC:AttendancePost)過來的 對應返回的對象 
        /// </summary>
        /// <param name="histReco"></param>
        /// <param name="attPostReturn"></param>
        /// <param name="responseModalX"></param>
        /// <returns></returns>
        public static bool AddNewHistRecordForAttendancePost(HistoryRecordsDefault histReco, FtDevice device, ref ResponseModalX responseModalX)
        {
            using HistoryContext historyContext = new HistoryContext();

            HistRecognizeRecord histRec1 = new HistRecognizeRecord
            {
                Id = histReco.OccurDatetime,
                Mode = histReco.Mode,
                MaincomId = device.MaincomId,
                OccurDatetime = histReco.OccurDatetime,
                DeviceId = histReco.DeviceId,
                DeviceName = histReco.DeviceName,
                TaskId = histReco.TaskId,
                TaskName = histReco.TaskName,
                CameraId = histReco.CameraId,
                CameraName = histReco.CameraName,
                PersonId = histReco.PersonId,
                PersonName = histReco.PersonName,
                Sex = histReco.Sex,
                CardNo = histReco.CardNo,
                Category = histReco.Category,
                LibId = histReco.LibId,
                LibName = histReco.LibName,
                Classify = histReco.Classify,
                PicPath = histReco.PicPath,
                CapturePath = string.Empty,
                Similarity = histReco.Similarity,
                Remark = histReco.Remark,
                Visible = histReco.Visible,
                CaptureTime = histReco.CaptureTime,
                CreateTime = histReco.CreateTime,
                UpdateTime = histReco.UpdateTime
            };

            //位置查詢 
            string siteName = DeviceBusiness.GetDeviceSiteName(device);

            ////如果存在记录,返回return数据对象以作前端跟进补漏
            var histRecExist = historyContext.HistRecognizeRecord.Find(histRec1.Id);
            if (histRecExist != null)
            {

                AttendanceLogReturn attendanceLogReturn = new AttendanceLogReturn
                {
                    AttendanceLogId = histRecExist.Id,
                    Mode = histRecExist.Mode,
                    DeviceId = histRecExist.DeviceId.ToString(),
                    DeviceName = histRecExist.DeviceName ?? string.Empty,
                    DeviceEntryMode = (int)device.DeviceEntryMode,
                    EmployeeId = histRecExist.PersonId.ToString(),
                    AccesscardId = histRecExist.CardNo,
                    CnName = histRecExist.PersonName ?? string.Empty,
                    OccurDateTime = histRecExist.OccurDatetime,
                    CatchPictureFileName = histRecExist.CapturePath??string.Empty,
                    MainComId = device.MaincomId,
                    CompanyName = string.Empty,
                    ContractorId = string.Empty,
                    ContractorName = string.Empty,
                    SiteId = device.SiteId.ToString(),
                    SiteName = siteName,
                    DepartmentId = string.Empty,
                    DepartmentName = string.Empty,
                    JobId = string.Empty,
                    JobName = string.Empty,
                    PositionId = string.Empty,
                    PositionTitle = string.Empty,
                    CratedDateTime = histRecExist.CreateTime,
                    CatchPicture = histRecExist.CapturePath ?? string.Empty,
                    FacialArea = string.Empty,
                    FacialAvatar = string.Empty,
                    LatitudeAndLongitude = string.Empty
                };
                //存在记录 ： 可能由于图片没有成功保存导致保存记录，这里返回true，让前端标记成功后上存图片等一系列动作
                //即：存在图片或记录都返回成功
                responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.GENERALUI_EXIST_RECORD, Message = $"{Lang.GeneralUI_ExistRecord} ID={histReco.Id}]" };
                responseModalX.data = attendanceLogReturn;
                return false;
            }

            try
            {
                historyContext.HistRecognizeRecord.Add(histRec1);
                bool result = historyContext.SaveChanges() > 0;

                DateTime occurDateTime = DateTimeHelp.ConvertToDateTime(histReco.OccurDatetime);
                if (result)
                    Console.WriteLine($"[SUCCESS][ATTENDANCE LOG INS][OCCUR:{occurDateTime:yyyy-MM-dd HH:mm:ss fff}][DATE TIME NOW:{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceManageController.AttendancePost][ID={histReco.Id}]");
                else
                    Console.WriteLine($"[FAILURE][ATTENDANCE LOG INS][OCCUR:{occurDateTime:yyyy-MM-dd HH:mm:ss fff}][DATE TIME NOW:{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC::DeviceManageController.AttendancePost][FAIL][entry.Occur={histReco.OccurDatetime}]");

                if (result)
                {
                    AttendanceLogReturn attendanceLogReturn = new AttendanceLogReturn
                    {
                        AttendanceLogId = histRec1.Id,
                        Mode = histRec1.Mode,
                        DeviceId = histRec1.DeviceId.ToString(),
                        DeviceName = histRec1.DeviceName ?? string.Empty,
                        DeviceEntryMode = (int)device.DeviceEntryMode,
                        EmployeeId = histRec1.PersonId.ToString(),
                        AccesscardId = histRec1.CardNo,
                        CnName = histRec1.PersonName ?? string.Empty,
                        OccurDateTime = histRec1.OccurDatetime,
                        CatchPictureFileName = string.Empty,//HttpUtility.UploadToBackend (EventDeploy设备同步用到的字段，空白提示上存图片) histRec1.CapturePath, //第一次新增的时候，肯定没有图片，返回string.empty 让前端识别用于上存图片
                        MainComId = device.MaincomId,
                        CompanyName = string.Empty,
                        ContractorId = string.Empty,
                        ContractorName = string.Empty,
                        SiteId = device.SiteId == 0 ? string.Empty : device.SiteId.ToString(),
                        SiteName = siteName,
                        DepartmentId = string.Empty,
                        DepartmentName = string.Empty,
                        JobId = string.Empty,
                        JobName = string.Empty,
                        PositionId = string.Empty,
                        PositionTitle = string.Empty,
                        CratedDateTime = histRec1.CreateTime,
                        CatchPicture = string.Empty,// histRec1.CapturePath, //第一次新增的时候，肯定没有图片，返回string.empty 让前端识别用于上存图片
                        FacialArea = string.Empty,
                        FacialAvatar = string.Empty,
                        LatitudeAndLongitude = string.Empty
                    }; 
                   

                    responseModalX.meta = new MetaModalX { Success = true, ErrorCode = (int)GeneralReturnCode.SUCCESS, Message = $"SUCCESSFUL TO ADD NEW  saveDatabaseResult={result}" };
                    responseModalX.data = attendanceLogReturn;

                    return true;
                }
                else
                {
                    responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = $"INSERT HistRecognizeRecord DATA BASE FAIL saveDatabaseResult={result}" };
                    responseModalX.data = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                responseModalX.meta = new MetaModalX { Success = false, ErrorCode = (int)GeneralReturnCode.EXCEPTION, Message = $"{ex.Message}" };
                responseModalX.data = null;
                return false;
            }
        }
        /// <summary>
        /// Hist 記錄入口
        /// </summary>
        public class AttendancePostEntry
        {
            public string MainComId { get; set; }
            public string DeviceId { get; set; }
            public string DeviceSerialNo { get; set; }
            public string EmployeeNo { get; set; }
            /// <summary>
            /// 記錄模式 例如 HIK人臉識別/GPS定位等等  參考 EnumBusiness.cs :: AttendanceMode
            /// </summary> 
            public int AttendanceMode { get; set; }
            public long Occur { get; set; }
            public string PhysicalId { get; set; }
            public string Face { get; set; }
            public string FingerPrint { get; set; }
        }
    }
}
