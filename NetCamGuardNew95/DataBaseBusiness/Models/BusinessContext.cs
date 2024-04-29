using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace DataBaseBusiness.Models
{
    public partial class BusinessContext : DbContext
    {
        public BusinessContext()
        {
        }

        public BusinessContext(DbContextOptions<BusinessContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Efmigrationshistory> Efmigrationshistory { get; set; }
        public virtual DbSet<FtCamera> FtCamera { get; set; }
        public virtual DbSet<FtCameraMpeg> FtCameraMpeg { get; set; }
        public virtual DbSet<FtConfig> FtConfig { get; set; }
        public virtual DbSet<FtDevice> FtDevice { get; set; }
        public virtual DbSet<FtDevicePerson> FtDevicePerson { get; set; }
        public virtual DbSet<FtDoor> FtDoor { get; set; }
        public virtual DbSet<FtLibrary> FtLibrary { get; set; }
        public virtual DbSet<FtPerson> FtPerson { get; set; }
        public virtual DbSet<FtPicture> FtPicture { get; set; }
        public virtual DbSet<FtSite> FtSite { get; set; }
        public virtual DbSet<FtTask> FtTask { get; set; }
        public virtual DbSet<FtUser> FtUser { get; set; }
        public virtual DbSet<Language> Language { get; set; }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                optionsBuilder.UseMySql("data source=localhost;database=camera_guard_business;user id=root;password=admin123;pooling=true;port=3306;sslmode=none;charset=utf8");
        //            }
        //        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Efmigrationshistory>(entity =>
            {
                entity.HasKey(e => e.MigrationId)
                    .HasName("PRIMARY");

                entity.ToTable("__efmigrationshistory");

                entity.Property(e => e.MigrationId)
                    .HasColumnType("varchar(95)")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasColumnType("varchar(32)")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<FtCamera>(entity =>
            {
                entity.ToTable("ft_camera");

                entity.HasIndex(e => e.Name)
                    .HasName("index_name");

                entity.HasIndex(e => e.Type)
                    .HasName("index_type");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("device_name")
                    .HasColumnType("varchar(128)")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.DeviceSerialNo)
                    .HasColumnName("device_serial_no")
                    .HasColumnType("varchar(128)")
                    .HasComment("[device_serial_no]  a label of device Identity ")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasColumnName("ip")
                    .HasColumnType("varchar(32)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("name of camera")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.OnLive)
                    .HasColumnName("on_live")
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'0'");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.RecordStatus)
                    .HasColumnName("record_status")
                    .HasDefaultValueSql("'0'")
                    .HasComment("录像状态 录像中 录像停止 录像站厅中");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Rtsp)
                    .IsRequired()
                    .HasColumnName("rtsp")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.SiteId)
                    .HasColumnName("site_id")
                    .HasComment("所在位置");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("'0'")
                    .HasComment("鏡頭類型: 海康 KW-1111 ,大華AXIS 等等的用途");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of camera, 0: valid, 1: invalid");
            });

            modelBuilder.Entity<FtCameraMpeg>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.DeviceId })
                    .HasName("PRIMARY");

                entity.ToTable("ft_camera_mpeg");

                entity.HasComment(@"Record the generated MPEG files Merge MP4 Files by day / hours for replay or other function
設備ID(NVR ID) + record id (NVR 本地數據庫的主鍵id) 混合為此表的主鍵索引");

                entity.HasIndex(e => e.MpegFilename)
                    .HasName("index_mpeg_filename");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.CameraId).HasColumnName("camera_Id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.EndTimestamp)
                    .HasColumnName("end_timestamp")
                    .HasComment("e.g. , end_timestamp=1601019000 this value fron streamHandle::do_decode while write mp4 trailer to create new value");

                entity.Property(e => e.FileFomat)
                    .IsRequired()
                    .HasColumnName("file_fomat")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("文件格式")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.FileSize)
                    .HasColumnName("file_size")
                    .HasComment("文件大小");

                entity.Property(e => e.GroupTimestamp)
                    .HasColumnName("group_timestamp")
                    .HasComment("default value = 0 ; for merge by group，fill in the id(Merge GroupId),like batch product GroupId=1601018246");

                entity.Property(e => e.IsFormatVirified)
                    .HasColumnName("is_format_virified")
                    .HasComment("signed that Verification of the mpeg format ");

                entity.Property(e => e.IsGroup)
                    .HasColumnName("is_group")
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("是否由多个媒体合并而来");

                entity.Property(e => e.IsUpload)
                    .HasColumnName("is_upload")
                    .HasComment("is_upload of ft_camera_mpeg,Uoload to cloud from device,isUploaded=1");

                entity.Property(e => e.MpegFilename)
                    .IsRequired()
                    .HasColumnName("mpeg_filename")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("除了mp4扩展名的mp4文件，还可能是其他类型，例如*.flv")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.StartTimestamp)
                    .HasColumnName("start_timestamp")
                    .HasComment("e.g. , start_timestamp=1601018000 this value should come from generate_filename when begin to write");

                entity.Property(e => e.Visible).HasColumnName("visible");
            });

            modelBuilder.Entity<FtConfig>(entity =>
            {
                entity.ToTable("ft_config");

                entity.HasIndex(e => e.Name)
                    .HasName("index_name");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Config)
                    .IsRequired()
                    .HasColumnName("config")
                    .HasColumnType("varchar(1024)")
                    .HasDefaultValueSql("''")
                    .HasComment("the detail of config")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of config")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasComment("受控于EnumBusiness.cs的ConfigType 1=CAMERA;2=DEVICE;可扩展其它的表");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of task, 0: valid, 1: invalid (暂时没什么用)");
            });

            modelBuilder.Entity<FtDevice>(entity =>
            {
                entity.HasKey(e => e.DeviceId)
                    .HasName("PRIMARY");

                entity.ToTable("ft_device");

                entity.HasComment("record the infomation of the  edge computing device about serial no, backend running handle,etc... 字段 device_type :EnumBusiness.DeviceEntryMode控制int的值");

                entity.HasIndex(e => e.DeviceId)
                    .HasName("device_id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DeviceConfig)
                    .HasColumnName("device_config")
                    .HasColumnType("varchar(4000)")
                    .HasComment("max=4000 因应 EnumCode.SysModuleType的模块类型 面向的对象解析为JSON不同, 如果是DVR设备,则对应的JSON对象是DVR.如果是HIK面型机则对应是HIK FACE的JSON解析对象")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.DeviceEntryMode)
                    .HasColumnName("device_entry_mode")
                    .HasComment("in/out/undefined");

                entity.Property(e => e.DeviceName)
                    .IsRequired()
                    .HasColumnName("device_name")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.DeviceSerialNo)
                    .IsRequired()
                    .HasColumnName("device_serial_no")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("'0'")
                    .HasComment("设备序列号: 电脑以CPU为默认的序列号,手机app,默认的设备序列号是手机IMEI,用于登记为合法的访问设备.")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.DeviceType)
                    .HasColumnName("device_type")
                    .HasComment("設備類型 兼容部分AttendanceMod常量,定義設備的類型. EnumBusiness.DeviceEntryMode控制int的值");

                entity.Property(e => e.IsReverseHex)
                    .HasColumnName("is_reverse_hex")
                    .HasColumnType("bit(1)")
                    .HasComment("from card hex decimal 拍卡的16进制码是否交叉反向解析为10进制");

                entity.Property(e => e.LibId)
                    .HasColumnName("lib_id")
                    .HasDefaultValueSql("'0'")
                    .HasComment("定義設備控制的人群");

                entity.Property(e => e.LibName)
                    .HasColumnName("lib_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("設備指派的群組的名稱");


                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .HasComment("公司ID, 如果是云部储的情况下,则很有用,否则使用MainCom对象NEW一个默认值.")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.SiteId)
                    .HasColumnName("site_id")
                    .HasComment("设备所在位置(分布式结构) 未指定=0");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("use Enum type to dinfine the status type");

                entity.Property(e => e.TaskId)
                    .HasColumnName("task_id")
                    .HasComment("用於警報模塊的任務,任務指定需要警報的鏡頭 0表示沒有任務,默認值0");

                entity.Property(e => e.TaskName)
                    .HasColumnName("task_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("任務那裡 有鏡頭組1 和鏡頭組2 對於鏡頭警報來說很適合.暫時改造成 設備下掛一個任務for 警報任務的. ")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<FtDevicePerson>(entity =>
            {
                entity.HasKey(e => new { e.DeviceId, e.PersonId })
                    .HasName("PRIMARY");

                entity.ToTable("ft_device_person");

                entity.HasComment("設備用戶狀態表,主要收集設備的用戶並且記錄中");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id")
                    .HasComment("設備ID");

                entity.Property(e => e.PersonId)
                    .HasColumnName("person_id")
                    .HasComment("人員ID");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("device_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("設備名稱")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.DownDelStatus)
                    .HasColumnName("down_del_status")
                    .HasComment("從設備刪除 無操作=-1  狀態: 等待=0  完成=1 失敗=2  無操作情況下顯示 刪除按鈕");

                entity.Property(e => e.DownDelStatusDt)
                    .HasColumnName("down_del_status_dt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasComment("相關刪除操作 記錄時間");

                entity.Property(e => e.DownInsertStatus)
                    .HasColumnName("down_insert_status")
                    .HasComment("下行到設備 狀態: 無操作=-1 等待=0  完成=1 失敗=2 無操作情況下顯示Insert按鈕");

                entity.Property(e => e.DownInsertStatusDt)
                    .HasColumnName("down_insert_status_dt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasComment("相關下行增加操作 記錄時間");

                entity.Property(e => e.DownUpdateStatus)
                    .HasColumnName("down_update_status")
                    .HasComment("下行更新到設備 無操作=-1  狀態: 等待=0  完成=1 失敗=2  無操作情況下顯示 update按鈕");

                entity.Property(e => e.DownUpdateStatusDt)
                    .HasColumnName("down_update_status_dt")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasComment("相關更新操作 記錄時間");

                entity.Property(e => e.LibId)
                    .HasColumnName("lib_id")
                    .HasComment("群組庫ID");

                entity.Property(e => e.LibName)
                    .IsRequired()
                    .HasColumnName("lib_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("設備人員是來自哪個群組庫")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasComment("公司ID")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.PersonName)
                    .HasColumnName("person_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("人員名稱")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.SynchronizedStatusRemark)
                    .HasColumnName("synchronized_status_remark")
                    .HasColumnType("varchar(500)")
                    .HasComment("記錄同步下行 指紋 卡號 密碼鍵 的三個狀態標識,是對象SynchronizedStatus的JSON形式存儲")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");
            });

            modelBuilder.Entity<FtDoor>(entity =>
            {
                entity.HasKey(e => e.DoorId)
                    .HasName("PRIMARY");

                entity.ToTable("ft_door");

                entity.HasComment(@"門禁系統單元
主要控制門,關聯到具體的控制設備(ft_device)");

                entity.Property(e => e.DoorId)
                    .HasColumnName("door_id")
                    .HasComment("門ID");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.DeviceId)
                    .HasColumnName("device_id")
                    .HasComment("受控的設備ID");

                entity.Property(e => e.DeviceName)
                    .HasColumnName("device_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("受控的設備名稱")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.DoorName)
                    .IsRequired()
                    .HasColumnName("door_name")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("門的名稱")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("主公司ID")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.SiteId)
                    .HasColumnName("site_id")
                    .HasComment("位置ID");

                entity.Property(e => e.SiteName)
                    .HasColumnName("site_name")
                    .HasColumnType("varchar(128)")
                    .HasComment("位置")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Status).HasComment("開關狀態");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<FtLibrary>(entity =>
            {
                entity.ToTable("ft_library");

                entity.HasIndex(e => e.LibId)
                    .HasName("index_lib_id");

                entity.HasIndex(e => e.Name)
                    .HasName("index_name");

                entity.HasIndex(e => e.Type)
                    .HasName("index_type");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LibId)
                    .HasColumnName("lib_id")
                    .HasComment("lib-id of library");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of library")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.PersonCount)
                    .HasColumnName("person_count")
                    .HasDefaultValueSql("'0'")
                    .HasComment("lib-id of person");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0:common, 1:VIP, ...");

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

            modelBuilder.Entity<FtPerson>(entity =>
            {
                entity.ToTable("ft_person");

                entity.HasIndex(e => e.Category)
                    .HasName("index_category");

                entity.HasIndex(e => e.LibId)
                    .HasName("index_lib_id");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CardNo)
                    .HasColumnName("card_no")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("''")
                    .HasComment("identity of person")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.PassKey)
                   .HasColumnName("pass_key")
                   .HasColumnType("varchar(200)")
                   .HasDefaultValueSql("''")
                   .HasComment("密碼 或JSON形式保存多個密碼")
                   .ForMySQLHasCharset("utf8mb3")
                   .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasDefaultValueSql("'1'")
                    .HasComment("category of person, 1: 非锁定/白名单, 0: 锁定/黑名单 范围是-128到127 默认值=1 非锁定");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LibId)
                    .HasColumnName("lib_id")
                    .HasComment("lib-id of person");

                entity.Property(e => e.LibIdGroups)
                    .HasColumnName("lib_id_groups")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .HasComment("所屬的多個人員群組")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of person 規則 中文_FIRST_LAST 空格隔開, split[0]=中文名,split[1]英文全名")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.OuterId)
                    .HasColumnName("outer_id")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("'0'")
                    .HasComment("outer id is from external system etc.: EmployeeId")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Phone)
                    .HasColumnName("phone")
                    .HasColumnType("varchar(15)")
                    .HasDefaultValueSql("''")
                    .HasComment("phone number")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Sex)
                    .HasColumnName("sex")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0: unknow, 1: male, 2: femal");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of person, 0: valid, 1: invalid");
            });

            modelBuilder.Entity<FtPicture>(entity =>
            {
                entity.ToTable("ft_picture");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.PersonId)
                    .HasColumnName("person_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.PicClientUrl)
                    .HasColumnName("pic_client_url")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.PicClientUrlBase64)
                    .HasColumnName("pic_client_url_base64")
                    .HasColumnType("longtext")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.PicId)
                    .HasColumnName("pic_id")
                    .HasDefaultValueSql("'0'")
                    .HasComment("id of picture");

                entity.Property(e => e.PicUrl)
                    .IsRequired()
                    .HasColumnName("pic_url")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .HasComment("url of picture")
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
                    .HasComment("state of person, 0: valid, 1: invalid");
            });

            modelBuilder.Entity<FtSite>(entity =>
            {
                entity.HasKey(e => e.SiteId)
                    .HasName("PRIMARY");

                entity.ToTable("ft_site");

                entity.HasComment("位置/大厦分布式结构 Sites/Buildings");

                entity.Property(e => e.SiteId).HasColumnName("site_id");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasColumnType("varchar(512)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.CameraCount).HasColumnName("camera_count");

                entity.Property(e => e.CreateDate)
                    .HasColumnName("create_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'6000014'")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.ParentsId)
                    .HasColumnName("parents_id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.PersonCount).HasColumnName("person_count");

                entity.Property(e => e.SiteName)
                    .IsRequired()
                    .HasColumnName("site_name")
                    .HasColumnType("varchar(256)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.UpdateDate)
                    .HasColumnName("update_date")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<FtTask>(entity =>
            {
                entity.ToTable("ft_task");

                entity.HasIndex(e => e.Name)
                    .HasName("index_name");

                entity.HasIndex(e => e.State)
                    .HasName("index_state");

                entity.HasIndex(e => e.Type)
                    .HasName("index_type");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CameraList1)
                    .IsRequired()
                    .HasColumnName("camera_list1")
                    .HasColumnType("varchar(1024)")
                    .HasDefaultValueSql("''")
                    .HasComment("camera list of entrance")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.CameraList2)
                    .HasColumnName("camera_list2")
                    .HasColumnType("varchar(1024)")
                    .HasDefaultValueSql("''")
                    .HasComment("camera list of exit")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Interval)
                    .HasColumnName("interval")
                    .HasDefaultValueSql("'3000'")
                    .HasComment("timer to capture a frame");

                entity.Property(e => e.LibList)
                    .IsRequired()
                    .HasColumnName("lib_list")
                    .HasColumnType("varchar(1024)")
                    .HasDefaultValueSql("''")
                    .HasComment("library list")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of task")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Plan)
                    .HasColumnName("plan")
                    .HasColumnType("varchar(256)")
                    .HasDefaultValueSql("''")
                    .HasComment("the plan of task, timer to start/stop the task")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.Remark)
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb3")
                    .ForMySQLHasCollation("utf8_general_ci");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasDefaultValueSql("'0'")
                    .HasComment("state of task, 0: off, 1: on");

                entity.Property(e => e.Threshold)
                    .HasColumnName("threshold")
                    .HasDefaultValueSql("'0.8'")
                    .HasComment("the line to judge a person is stranger");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0: common,...");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of task, 0: valid, 1: invalid");
            });

            modelBuilder.Entity<FtUser>(entity =>
            {
                entity.ToTable("ft_user");

                entity.HasIndex(e => e.Name)
                    .HasName("index_name");

                entity.HasIndex(e => e.Visible)
                    .HasName("index_visible");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("create_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.MaincomId)
                    .IsRequired()
                    .HasColumnName("maincom_id")
                    .HasColumnType("varchar(128)")
                    .HasDefaultValueSql("'0'")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(128)")
                    .HasComment("name of user")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("varchar(128)")
                    .HasComment("password of user, md5 encode")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasColumnName("remark")
                    .HasColumnType("varchar(512)")
                    .HasDefaultValueSql("''")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.UpdateTime)
                    .HasColumnName("update_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Visible)
                    .HasColumnName("visible")
                    .HasDefaultValueSql("'1'")
                    .HasComment("state of user, 0: valid, 1: invalid");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasKey(e => e.KeyName)
                    .HasName("PRIMARY");

                entity.ToTable("language");

                entity.HasIndex(e => e.KeyName)
                    .HasName("key_name_Index");

                entity.Property(e => e.KeyName)
                    .HasColumnName("key_name")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.EnUs)
                    .HasColumnName("en_us")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.IndustryIdArr)
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.KeyType)
                    .HasColumnName("key_type")
                    .HasColumnType("varchar(50)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.MainComIdArr)
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.Remarks)
                    .HasColumnName("remarks")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.ZhCn)
                    .HasColumnName("zh_cn")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");

                entity.Property(e => e.ZhHk)
                    .HasColumnName("zh_hk")
                    .HasColumnType("varchar(500)")
                    .ForMySQLHasCharset("utf8mb4")
                    .ForMySQLHasCollation("utf8mb4_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
