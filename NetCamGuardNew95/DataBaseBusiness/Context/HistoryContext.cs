using System;
using System.Configuration;
using DataBaseBusiness.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistoryContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string CONNECTION_STRING = AppSetting.GetConfig("ConnectionStrings:mySqlConn_History");
                optionsBuilder.UseMySql(CONNECTION_STRING);
                //optionsBuilder.UseMySql("Data Source=81.71.74.135;Database=camera_guard_history;User ID=root;Password=Admin@62595738;pooling=true;port=3306;sslmode=none;CharSet=utf8;");
            }
        }
    }

    /// <summary>
    /// 為了兩個系統兼容,返回的對象一樣(來自DATAGUARD XCORE)
    /// </summary>
    public partial class AttendanceLogReturn
    {
        public long AttendanceLogId { get; set; }
        public int Mode { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceEntryMode { get; set; }
        public string EmployeeId { get; set; }
        public string AccesscardId { get; set; }
        public string CnName { get; set; }
        public long OccurDateTime { get; set; }
        public string CatchPictureFileName { get; set; }
        public string MainComId { get; set; }
        public string CompanyName { get; set; }
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string PositionId { get; set; }
        public string PositionTitle { get; set; }
        public DateTime CratedDateTime { get; set; }
        public string CatchPicture { get; set; }
        public string FacialArea { get; set; }
        public string FacialAvatar { get; set; }
        public string LatitudeAndLongitude { get; set; }
    }
}
