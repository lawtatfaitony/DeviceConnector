using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataBaseBusiness.Migrations
{
    public partial class InitializeDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__efmigrationshistory",
                columns: table => new
                {
                    MigrationId = table.Column<string>(type: "varchar(95)", nullable: false),
                    ProductVersion = table.Column<string>(type: "varchar(32)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MigrationId);
                });

            migrationBuilder.CreateTable(
                name: "ft_camera",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    site_id = table.Column<int>(nullable: false, comment: "所在位置"),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "''", comment: "name of camera"),
                    rtsp = table.Column<string>(type: "varchar(256)", nullable: false, defaultValueSql: "''"),
                    ip = table.Column<string>(type: "varchar(32)", nullable: false, defaultValueSql: "''"),
                    username = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "''"),
                    password = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "''"),
                    type = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "鏡頭類型: 海康 KW-1111 ,大華AXIS 等等的用途"),
                    remark = table.Column<string>(type: "varchar(512)", nullable: true, defaultValueSql: "''"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of camera, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    device_serial_no = table.Column<string>(type: "varchar(128)", nullable: true, comment: "[device_serial_no]  a label of device Identity "),
                    device_id = table.Column<int>(nullable: false),
                    device_name = table.Column<string>(type: "varchar(128)", nullable: true),
                    record_status = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "录像状态 录像中 录像停止 录像站厅中"),
                    on_live = table.Column<ulong>(type: "bit(1)", nullable: true, defaultValueSql: "b'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_camera", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_camera_mpeg",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false),
                    device_id = table.Column<int>(nullable: false),
                    camera_Id = table.Column<int>(nullable: false),
                    mpeg_filename = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "''", comment: "除了mp4扩展名的mp4文件，还可能是其他类型，例如*.flv"),
                    group_timestamp = table.Column<long>(nullable: false, comment: "default value = 0 ; for merge by group，fill in the id(Merge GroupId),like batch product GroupId=1601018246"),
                    file_size = table.Column<long>(nullable: false, comment: "文件大小"),
                    file_fomat = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "''", comment: "文件格式"),
                    is_group = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'0'", comment: "是否由多个媒体合并而来"),
                    start_timestamp = table.Column<long>(nullable: false, comment: "e.g. , start_timestamp=1601018000 this value should come from generate_filename when begin to write"),
                    end_timestamp = table.Column<long>(nullable: false, comment: "e.g. , end_timestamp=1601019000 this value fron streamHandle::do_decode while write mp4 trailer to create new value"),
                    visible = table.Column<sbyte>(nullable: false),
                    is_format_virified = table.Column<sbyte>(nullable: false, comment: "signed that Verification of the mpeg format "),
                    is_upload = table.Column<sbyte>(nullable: false, comment: "is_upload of ft_camera_mpeg,Uoload to cloud from device,isUploaded=1"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.device_id });
                },
                comment: "Record the generated MPEG files Merge MP4 Files by day / hours for replay or other function 設備ID(NVR ID) + record id (NVR 本地數據庫的主鍵id) 混合為此表的主鍵索引");

            migrationBuilder.CreateTable(
                name: "ft_config",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, comment: "name of config"),
                    type = table.Column<int>(nullable: false, comment: "受控于EnumBusiness.cs的ConfigType 1=CAMERA;2=DEVICE;可扩展其它的表"),
                    config = table.Column<string>(type: "varchar(1024)", nullable: false, defaultValueSql: "''", comment: "the detail of config"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of task, 0: valid, 1: invalid (暂时没什么用)"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_device",
                columns: table => new
                {
                    device_id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    device_name = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    device_type = table.Column<int>(nullable: true, comment: "設備類型 兼容部分AttendanceMod常量,定義設備的類型. EnumBusiness.DeviceEntryMode控制int的值"),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'", comment: "公司ID, 如果是云部储的情况下,则很有用,否则使用MainCom对象NEW一个默认值."),
                    site_id = table.Column<int>(nullable: false, comment: "设备所在位置(分布式结构) 未指定=0"),
                    lib_id = table.Column<int>(nullable: true, defaultValueSql: "'0'", comment: "定義設備控制的人群"),
                    lib_name = table.Column<string>(type: "varchar(126)", nullable: false, defaultValueSql: "'0'", comment: "設備指派的群組的名稱"),
                    device_serial_no = table.Column<string>(type: "varchar(256)", nullable: false, defaultValueSql: "'0'", comment: "设备序列号: 电脑以CPU为默认的序列号,手机app,默认的设备序列号是手机IMEI,用于登记为合法的访问设备."),
                    device_config = table.Column<string>(type: "varchar(4000)", nullable: true, comment: "max=4000 因应 EnumCode.SysModuleType的模块类型 面向的对象解析为JSON不同, 如果是DVR设备,则对应的JSON对象是DVR.如果是HIK面型机则对应是HIK FACE的JSON解析对象"),
                    is_reverse_hex = table.Column<ulong>(type: "bit(1)", nullable: false, comment: "from card hex decimal 拍卡的16进制码是否交叉反向解析为10进制"),
                    device_entry_mode = table.Column<int>(nullable: false, comment: "in/out/undefined"),
                    status = table.Column<int>(nullable: false, defaultValueSql: "'1'", comment: "use Enum type to dinfine the status type"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.device_id);
                },
                comment: "record the infomation of the  edge computing device about serial no, backend running handle,etc... 字段 device_type :EnumBusiness.DeviceEntryMode控制int的值");

            migrationBuilder.CreateTable(
                name: "ft_library",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    lib_id = table.Column<int>(nullable: false, comment: "lib-id of library"),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, comment: "name of library"),
                    person_count = table.Column<int>(nullable: true, defaultValueSql: "'0'", comment: "lib-id of person"),
                    type = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "0:common, 1:VIP, ..."),
                    remark = table.Column<string>(type: "varchar(512)", nullable: false, defaultValueSql: "''"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of library, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_library", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_person",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    outer_id = table.Column<string>(type: "varchar(50)", nullable: true, defaultValueSql: "'0'", comment: "outer id is from external system etc.: EmployeeId"),
                    lib_id = table.Column<int>(nullable: false, comment: "lib-id of person"),
                    lib_id_groups = table.Column<string>(type: "varchar(256)", nullable: true, defaultValueSql: "''", comment: "所屬的多個人員群組"),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, comment: "name of person"),
                    sex = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "0: unknow, 1: male, 2: femal"),
                    card_no = table.Column<string>(type: "varchar(128)", nullable: true, defaultValueSql: "''", comment: "identity of person"),
                    pass_key = table.Column<string>(type: "varchar(200)", nullable: true, defaultValueSql: "''", comment: "密碼 或JSON形式保存多個密碼"),
                    phone = table.Column<string>(type: "varchar(15)", nullable: true, defaultValueSql: "''", comment: "phone number"),
                    category = table.Column<sbyte>(nullable: false, defaultValueSql: "'1'", comment: "category of person, 1: 非锁定/白名单, 0: 锁定/黑名单 范围是-128到127 默认值=1 非锁定"),
                    remark = table.Column<string>(type: "varchar(512)", nullable: true, defaultValueSql: "''"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of person, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_person", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_picture",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    pic_id = table.Column<long>(nullable: true, defaultValueSql: "'0'", comment: "id of picture"),
                    person_id = table.Column<long>(nullable: true, defaultValueSql: "'0'"),
                    pic_url = table.Column<string>(type: "varchar(256)", nullable: false, defaultValueSql: "''", comment: "url of picture"),
                    pic_client_url = table.Column<string>(type: "varchar(256)", nullable: true, defaultValueSql: "''"),
                    pic_client_url_base64 = table.Column<string>(type: "longtext", nullable: true),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of person, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_picture", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_site",
                columns: table => new
                {
                    site_id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'6000014'"),
                    parents_id = table.Column<int>(nullable: true, defaultValueSql: "'0'"),
                    site_name = table.Column<string>(type: "varchar(256)", nullable: false),
                    camera_count = table.Column<int>(nullable: false),
                    person_count = table.Column<int>(nullable: false),
                    address = table.Column<string>(type: "varchar(512)", nullable: true),
                    create_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.site_id);
                },
                comment: "位置/大厦分布式结构 Sites/Buildings");

            migrationBuilder.CreateTable(
                name: "ft_task",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, comment: "name of task"),
                    type = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "0: common,..."),
                    camera_list1 = table.Column<string>(type: "varchar(1024)", nullable: false, defaultValueSql: "''", comment: "camera list of entrance"),
                    camera_list2 = table.Column<string>(type: "varchar(1024)", nullable: true, defaultValueSql: "''", comment: "camera list of exit"),
                    lib_list = table.Column<string>(type: "varchar(1024)", nullable: false, defaultValueSql: "''", comment: "library list"),
                    interval = table.Column<int>(nullable: true, defaultValueSql: "'3000'", comment: "timer to capture a frame"),
                    threshold = table.Column<float>(nullable: true, defaultValueSql: "'0.8'", comment: "the line to judge a person is stranger"),
                    state = table.Column<sbyte>(nullable: true, defaultValueSql: "'0'", comment: "state of task, 0: off, 1: on"),
                    plan = table.Column<string>(type: "varchar(256)", nullable: true, defaultValueSql: "''", comment: "the plan of task, timer to start/stop the task"),
                    remark = table.Column<string>(type: "varchar(512)", nullable: true, defaultValueSql: "''"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of task, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_task", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ft_user",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.AutoIncrement),
                    maincom_id = table.Column<string>(type: "varchar(128)", nullable: false, defaultValueSql: "'0'"),
                    name = table.Column<string>(type: "varchar(128)", nullable: false, comment: "name of user"),
                    password = table.Column<string>(type: "varchar(128)", nullable: false, comment: "password of user, md5 encode"),
                    remark = table.Column<string>(type: "varchar(512)", nullable: false, defaultValueSql: "''"),
                    visible = table.Column<sbyte>(nullable: true, defaultValueSql: "'1'", comment: "state of user, 0: valid, 1: invalid"),
                    create_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    update_time = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ft_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    key_name = table.Column<string>(type: "varchar(50)", nullable: false),
                    key_type = table.Column<string>(type: "varchar(50)", nullable: true),
                    zh_cn = table.Column<string>(type: "varchar(500)", nullable: true),
                    zh_hk = table.Column<string>(type: "varchar(500)", nullable: true),
                    en_us = table.Column<string>(type: "varchar(500)", nullable: true),
                    remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IndustryIdArr = table.Column<string>(type: "varchar(500)", nullable: true),
                    MainComIdArr = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.key_name);
                });

            migrationBuilder.CreateIndex(
                name: "index_name",
                table: "ft_camera",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "index_type",
                table: "ft_camera",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_camera",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "index_mpeg_filename",
                table: "ft_camera_mpeg",
                column: "mpeg_filename");

            migrationBuilder.CreateIndex(
                name: "index_name",
                table: "ft_config",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_config",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "device_id",
                table: "ft_device",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "index_lib_id",
                table: "ft_library",
                column: "lib_id");

            migrationBuilder.CreateIndex(
                name: "index_name",
                table: "ft_library",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "index_type",
                table: "ft_library",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_library",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "index_category",
                table: "ft_person",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "index_lib_id",
                table: "ft_person",
                column: "lib_id");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_person",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_picture",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "index_name",
                table: "ft_task",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "index_state",
                table: "ft_task",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "index_type",
                table: "ft_task",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_task",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "index_name",
                table: "ft_user",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "index_visible",
                table: "ft_user",
                column: "visible");

            migrationBuilder.CreateIndex(
                name: "key_name_Index",
                table: "language",
                column: "key_name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__efmigrationshistory");

            migrationBuilder.DropTable(
                name: "ft_camera");

            migrationBuilder.DropTable(
                name: "ft_camera_mpeg");

            migrationBuilder.DropTable(
                name: "ft_config");

            migrationBuilder.DropTable(
                name: "ft_device");

            migrationBuilder.DropTable(
                name: "ft_library");

            migrationBuilder.DropTable(
                name: "ft_person");

            migrationBuilder.DropTable(
                name: "ft_picture");

            migrationBuilder.DropTable(
                name: "ft_site");

            migrationBuilder.DropTable(
                name: "ft_task");

            migrationBuilder.DropTable(
                name: "ft_user");

            migrationBuilder.DropTable(
                name: "language");
        }
    }
}
