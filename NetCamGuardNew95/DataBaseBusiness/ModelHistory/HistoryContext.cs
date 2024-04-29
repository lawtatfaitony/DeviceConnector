using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace DataBaseBusiness.ModelHistory
{
    public partial class HistoryContext : DbContext
    {
        public HistoryContext()
        {
        }

        public HistoryContext(DbContextOptions<HistoryContext> options)
            : base(options)
        {
        }

        public virtual DbSet<HistAlarm> HistAlarm { get; set; }
        public virtual DbSet<HistGpsRecord> HistGpsRecord { get; set; }
        public virtual DbSet<HistRecognizeRecord> HistRecognizeRecord { get; set; }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                optionsBuilder.UseMySql("data source=81.71.74.135;database=camera_guard_history;user id=root;password=Admin@62595738;pooling=true;port=3306;sslmode=none;charset=utf8", x => x.ServerVersion("8.0.29-mysql"));
        //            }
        //        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistAlarm>(entity =>
            {
                entity.ToTable("hist_alarm");

                entity.HasComment("For 人工智能的識別數據警報。例如設置人臉識別的警報 車牌識別的警報 工衣警報等等");

                entity.HasIndex(e => e.HistAlarmId)
                    .HasName("hist_alarm_id");

                entity.Property(e => e.HistAlarmId).HasColumnName("hist_alarm_id");

                entity.Property(e => e.AlarmLevel)
                    .HasColumnName("alarm_level")
                    .HasDefaultValueSql("'0'")
                    .HasComment("警報的等級。如果全局設置等級3，則大於等於3才會警報或其他形式的警報");

                entity.Property(e => e.CameraId)
                    .HasColumnName("camera_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CameraName)
                    .HasColumnName("camera_name")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.CapturePath)
                    .HasColumnName("capture_path")
                    .HasColumnType("varchar(300)")
                    .HasComment("相對路徑 例如：/Upload/pitcure/aa.jpg")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.CaptureTime)
                    .HasColumnName("capture_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasComment("以日期時間格式表示，occur以timestamp bigInt表示");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.MaincomId)
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.ObjJsonData)
                    .HasColumnName("obj_json_data")
                    .HasColumnType("varchar(2000)")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_unicode_ci");

                entity.Property(e => e.ObjName)
                    .HasColumnName("obj_name")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.ObjShortDesc)
                    .HasColumnName("obj_short_desc")
                    .HasColumnType("varchar(500)")
                    .HasComment("由邏輯生成的簡短描述 例如：張學友沒有穿工作服")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.OccurDatetime)
                    .HasColumnName("occur_datetime")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_unicode_ci");

                entity.Property(e => e.TaskId).HasColumnName("task_id");

                entity.Property(e => e.TaskName)
                    .HasColumnName("task_name")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.TaskType)
                    .HasColumnName("task_type")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TaskTypeDesc)
                    .HasColumnName("task_type_desc")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Threshold)
                    .HasColumnName("threshold")
                    .HasColumnType("double")
                    .HasComment("0.99標識精度99%")
                    .HasDefaultValueSql("'0.99'");
            });

            modelBuilder.Entity<HistGpsRecord>(entity =>
            {
                entity.ToTable("hist_gps_record");

                entity.HasComment("hist_recognize_record的擴展表,擴展GPS定位功能");

                entity.Property(e => e.HistGpsRecordId).HasColumnName("hist_gps_record_id");

                entity.Property(e => e.HistRecognizeRecordId).HasColumnName("hist_recognize_record_id");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("float(10,6)");

                entity.Property(e => e.LatitudeAndLongitudeJson)
                    .HasColumnName("latitude_and_longitude_json")
                    .HasColumnType("varchar(4000)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.LongAddress)
                    .HasColumnName("long_address")
                    .HasColumnType("varchar(256)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("float(10,6)");

                entity.Property(e => e.ShortAddress)
                    .HasColumnName("short_address")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");
            });

            modelBuilder.Entity<HistRecognizeRecord>(entity =>
            {
                entity.ToTable("hist_recognize_record");

                entity.HasComment("主要是人臉識別機 指紋機 拍卡機等等的識別數據LOG記錄往這裡POST. 可以說是一個LOG性質的記錄表。");

                entity.HasIndex(e => e.CameraId)
                    .HasName("index_camera_id");

                entity.HasIndex(e => e.CardNo)
                    .HasName("index_card_no");

                entity.HasIndex(e => e.Classify)
                    .HasName("index_classify");

                entity.HasIndex(e => e.LibId)
                    .HasName("index_lib_id");

                entity.HasIndex(e => e.TaskId)
                    .HasName("index_task_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CameraId)
                    .HasColumnName("camera_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CameraName)
                    .IsRequired()
                    .HasColumnName("camera_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of camera")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.CapturePath)
                    .IsRequired()
                    .HasColumnName("capture_path")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.CaptureTime)
                    .HasColumnName("capture_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.CardNo)
                    .IsRequired()
                    .HasColumnName("card_no")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("identity of person/NFC CARD NUMBER")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasDefaultValueSql("'0'")
                    .HasComment("category of person, 0: 白名单, 1: 黑名单");

                entity.Property(e => e.Classify)
                    .HasColumnName("classify")
                    .HasDefaultValueSql("'0'")
                    .HasComment("STRANGER = 0; IS_PERSON=1");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("device_name")
                    .HasColumnType("varchar(256)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.LibId)
                    .HasColumnName("lib_id")
                    .HasDefaultValueSql("'0'")
                    .HasComment("lib-id of person");

                entity.Property(e => e.LibName)
                    .IsRequired()
                    .HasColumnName("lib_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of library")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.MaincomId)
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Mode)
                    .HasColumnName("mode")
                    .HasComment("Attendance Mode");

                entity.Property(e => e.OccurDatetime)
                    .HasColumnName("occur_datetime")
                    .HasComment("unix timestamp format");

                entity.Property(e => e.PersonId)
                    .HasColumnName("person_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.PersonName)
                    .IsRequired()
                    .HasColumnName("person_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of person")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.PicPath)
                    .IsRequired()
                    .HasColumnName("pic_path")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Sex)
                    .HasColumnName("sex")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0: unknow, 1: male, 2: femal");

                entity.Property(e => e.Similarity)
                    .HasColumnName("similarity")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TaskId)
                    .HasColumnName("task_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasColumnName("task_name")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("name of task")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of library, 0: valid, 1: invalid");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
