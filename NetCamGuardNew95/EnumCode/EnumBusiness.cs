using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EnumCode
{
    /// <summary>
    /// 設備狀態 | 功能模組 | 設備類型 | 配置類型
    /// </summary>
    public partial class EnumBusiness
    {
        /// <summary>
        /// 控制表 [ft_config][type]
        /// </summary>
        public enum ConfigType
        {
            /// <summary>
            /// 配置來源於鏡頭 FOR TABLE [ft_camera]
            /// </summary>
            [EnumDisplayName("ConfigType_CAMERA")]
            CAMERA = 1,
            /// <summary>
            /// 配置來源於設備 FOR TABLE [ft_device] | 另外设备表有自己独立的字段 DeviceConfig可以用
            /// </summary>
            [EnumDisplayName("ConfigType_DEVICE")]
            DEVICE = 2,
        }

        /// <summary>
        /// NVR_Type 錄像機類型 c++開發了兩種 2種：MediaGuard.exe 和 Media.exe
        /// MediaGuard.exe : D:\OpensslRSA_NEW_MPEG_REC\new-ffmpeg-helper2023\MediaGuard
        /// </summary>
        public enum NVR_TYPE
        {
            /// <summary>
            /// 配置來源  NVR_TYPE_MEDIA_GUARD_EXE
            /// </summary>
            [EnumDisplayName("NVR_TYPE_MEDIA_GUARD_EXE")]
            MEDIA_GUARD = 0,
            /// <summary>
            /// 配置來源 NVR_TYPE_MEDIA_EXE
            /// </summary>
            [EnumDisplayName("NVR_TYPE_MEDIA_EXE")]
            MEDIA = 1
        }
        /// <summary>
        /// 初始默認是創建MainCom公司的時候就按如下的屬性批量插入對應的表 SysModule
        /// </summary>
        public enum SysModuleType
        {
            /// <summary>
            /// 功能
            /// </summary>
            FUNCTIONS = -3,

            /// <summary>
            /// 模塊
            /// </summary>
            MODULE = -2,

            /// <summary>
            /// 未分類的設備
            /// </summary>
            UNCATEGORIZED = -1,

            /// <summary>
            /// 設備(泛指)
            /// </summary>
            DEVICE = 0
        }

        /// <summary>
        /// 原來的值: UNDEFINED = 0, IN = 1,OUT = -1, INOUT = 2 改為與CIC一致 即 IN = 0,OUT = 1,TAPPED=2 (2022-4-19)
        /// </summary>
        public enum DeviceEntryMode
        {
            [EnumDisplayName("DeviceEntryMode_UNDEFINED")]
            UNDEFINED = -1,
            [EnumDisplayName("DeviceEntryMode_IN")]
            IN = 0,
            [EnumDisplayName("DeviceEntryMode_OUT")]
            OUT = 1,
            [EnumDisplayName("DeviceEntryMode_INOUT")]
            INOUT = 2
        }

        public enum Genders
        {
            [EnumDisplayName("Genders_MALE")]
            Male = 1,
            [EnumDisplayName("Genders_FEMALE")]
            Female = 2,
            [EnumDisplayName("Genders_UNKOWN")] //GeneralUI_Undefined
            Unkown = 3  // M;F;UNKOWN
        }
        /// <summary>
        /// 複製自DGX 參考用途 切勿引用
        /// </summary>
        public enum AttendanceModeDGX
        {
            [EnumDisplayName("AttendanceMode_FACE_RECOGNITION")]
            FACE = -1,
            [EnumDisplayName("AttendanceMode_CAM_GUARD")]
            CAM_GUARD = 0,
            [EnumDisplayName("AttendanceMode_HIK_CARD")]
            HIK_CARD = 1,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD")]
            STANDARD_CARD = 2,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD_AND_FINGERPRINT")]
            STANDARD_CARD_AND_FINGERPRINT = 3,
            [EnumDisplayName("AttendanceMode_CIC_CARD")]
            CIC_CARD = 4,
            [EnumDisplayName("AttendanceMode_BAIDU_MAP")]
            BAIDU = 5,
            [EnumDisplayName("AttendanceMode_GOOGLE_MAP")]
            GOOGLE = 6,
            [EnumDisplayName("AttendanceMode_QQ_MAP")]
            QQ = 7,
            [EnumDisplayName("AttendanceMode_GPS_MAP")]
            GPS = 8,
            [EnumDisplayName("AttendanceMode_FINGERPRINT")]
            FINGERPRINT = 9,
            [EnumDisplayName("AttendanceMode_PASSWORD")]
            PASSWORD = 10,
            [EnumDisplayName("AttendanceMode_COMBINE_VERIFY")]
            COMBINE_VERIFY = 11,
            [EnumDisplayName("AttendanceMode_CARD_FINGERPRINT_PASSWD")]
            CARD_FINGERPRINT_PASSWD = 12,
            [EnumDisplayName("AttendanceMode_FACE_CARD_VERIFY")]
            FACE_CARD_VERIFY = 13,
            //手动调整 MANUAL ADJUSTMENT  FROM [AttRegularMode]
            [EnumDisplayName("AttRegularMode_WORK_ON")]
            WORK_ON = 994,
            [EnumDisplayName("AttRegularMode_WORK_OFF")]
            WORK_OFF = 995,
            [EnumDisplayName("AttRegularMode_LUNCH_START")]
            LUNCH_START = 996,
            [EnumDisplayName("AttRegularMode_LUNCH_END")]
            LUNCH_END = 997,
            [EnumDisplayName("AttRegularMode_OVERTIME_START")]
            OVERTIME_START = 998,
            [EnumDisplayName("AttRegularMode_OVERTIME_END")]
            OVERTIME_END = 999
        }

        /// <summary>
        /// 設備類型 主要是指設備的 類型 其他實體設備直接以品牌大寫+設備型號大寫為設備名 例如 HIK_DS_KIT341BMW
        /// 部分設備的類型引用AttendanceMode
        /// 部分是虛擬設備 例如QQ MAP GOOGLE MAP 屬於GPS的虛擬設備
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// 無定義的 (設備) DEVICE_UNDEFINED_DEVICE
            /// </summary>
            [EnumDisplayName("DEVICE_UNDEFINED_DEVICE")]
            UNDEFINED_DEVICE = 1000,
            /// <summary>
            /// Android 拍卡 (設備)
            /// </summary>
            [EnumDisplayName("DEVICE_ANDROID_NFC")]
            ANDROID_NFC = 1001,

            [EnumDisplayName("AttendanceMode_DAHUA_CARD_FACE")] //20001為大華設備類型 如何拍卡人臉 DAHUA_CARD_FACE = 1001
            DAHUA_CARD_FACE = 20001,
            /// <summary>
            /// 海康 HIK_DS_KIT341BMW人臉/拍卡 設備類型
            /// </summary>
            [EnumDisplayName("DEVICE_HIK_DS_DEVICE")]
            HIK_DEVICE = 10022,

            /// <summary>
            /// 海康 HIK_DS_KIT341BMW人臉/拍卡 設備類型
            /// </summary>
            [EnumDisplayName("DEVICE_HIK_DS_KIT341BMW")]
            HIK_DS_KIT341BMW = 1002,
            /// <summary>
            /// 海康 HIK_DS_KIT804MF 設備類型
            /// </summary>
            [EnumDisplayName("DEVICE_HIK_DS_KIT804MF")]
            HIK_DS_KIT804MF = 1003,

            /// <summary>
            /// NFC CIC協會的CHECK SITE模式
            /// </summary>
            [EnumDisplayName("DEVICE_TYPE_NFCCIC_CHK")]
            NFCCIC_CHECK = 1004,

            /// <summary>
            /// NFC CIC協會的 拍卡模式 mode=9
            /// </summary>
            [EnumDisplayName("DEVICE_TYPE_NFCCIC_TAP")]
            NFCCIC_TAP = 1005,

            /// <summary>
            /// DESTOP DVR 數字錄像設備/数字录像设备 基於電腦的
            /// </summary>
            [EnumDisplayName("DEVICE_TYPE_DESTOP_DVR")]
            DESTOP_DVR = 1006,

            [EnumDisplayName("AttendanceMode_FACE_RECOGNITION")]
            FACE = -1,
            [EnumDisplayName("AttendanceMode_CAM_GUARD")]
            CAM_GUARD = 0,
            [EnumDisplayName("AttendanceMode_HIK_CARD")]
            HIK_CARD = 1,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD")]
            STANDARD_CARD = 2,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD_AND_FINGERPRINT")]
            STANDARD_CARD_AND_FINGERPRINT = 3,
            [EnumDisplayName("AttendanceMode_CIC_CARD")]
            CIC_CARD = 4,
            [EnumDisplayName("AttendanceMode_CIC_CARD_CHECK")]
            CIC_CARD_CHECK = 44,
            [EnumDisplayName("AttendanceMode_BAIDU_MAP")]
            BAIDU = 5,
            [EnumDisplayName("AttendanceMode_GOOGLE_MAP")]
            GOOGLE = 6,
            [EnumDisplayName("AttendanceMode_QQ_MAP")]
            QQ = 7,
            [EnumDisplayName("AttendanceMode_APPLE_MAP")]
            APPLE_MAP = 77,
            [EnumDisplayName("AttendanceMode_GPS_MAP")]
            GPS = 8,
            [EnumDisplayName("AttendanceMode_FINGERPRINT")]
            FINGERPRINT = 9,
            [EnumDisplayName("AttendanceMode_PASSWORD")]
            PASSWORD = 10,
            [EnumDisplayName("AttendanceMode_COMBINE_VERIFY")]
            COMBINE_VERIFY = 11,
            [EnumDisplayName("AttendanceMode_CARD_FINGERPRINT_PASSWD")]
            CARD_FINGERPRINT_PASSWD = 12,
            [EnumDisplayName("AttendanceMode_FACE_CARD_VERIFY")]
            FACE_CARD_VERIFY = 13
        }

        /// <summary>
        /// 和DataGuardXcore系統一樣 主導表[HistRecognizeRecord].[Mode]的模式的值
        ///  ATTENDANCE MODE FOR ATTENDANCE_LOG
        /// </summary>
        public enum AttendanceMode
        {
            [EnumDisplayName("AttendanceMode_FACE_RECOGNITION")]
            FACE = -1,
            [EnumDisplayName("AttendanceMode_CAM_GUARD")]
            CAM_GUARD = 0,
            [EnumDisplayName("AttendanceMode_DAHUA_CARD_FACE")] //20001為大華設備類型 如何拍卡人臉 DAHUA_CARD_FACE = 20001
            DAHUA_CARD_FACE = 20001,
            [EnumDisplayName("AttendanceMode_HIK_CARD")]
            HIK_CARD = 1,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD")]
            STANDARD_CARD = 2,
            [EnumDisplayName("AttendanceMode_STANDARD_CARD_AND_FINGERPRINT")]
            STANDARD_CARD_AND_FINGERPRINT = 3,
            [EnumDisplayName("AttendanceMode_CIC_CARD")]
            CIC_CARD = 4,
            [EnumDisplayName("AttendanceMode_BAIDU_MAP")]
            BAIDU = 5,
            [EnumDisplayName("AttendanceMode_GOOGLE_MAP")]
            GOOGLE = 6,
            [EnumDisplayName("AttendanceMode_QQ_MAP")]
            QQ = 7,
            [EnumDisplayName("AttendanceMode_GPS_MAP")]
            GPS = 8,
            [EnumDisplayName("AttendanceMode_FINGERPRINT")]
            FINGERPRINT = 9,
            [EnumDisplayName("AttendanceMode_PASSWORD")]
            PASSWORD = 10,
            [EnumDisplayName("AttendanceMode_COMBINE_VERIFY")]
            COMBINE_VERIFY = 11,
            [EnumDisplayName("AttendanceMode_CARD_FINGERPRINT_PASSWD")]
            CARD_FINGERPRINT_PASSWD = 12,
            [EnumDisplayName("AttendanceMode_FACE_CARD_VERIFY")]
            FACE_CARD_VERIFY = 13,
            //手动调整 MANUAL ADJUSTMENT  FROM [AttRegularMode]
            [EnumDisplayName("AttRegularMode_WORK_ON")]
            WORK_ON = 994,
            [EnumDisplayName("AttRegularMode_WORK_OFF")]
            WORK_OFF = 995,
            [EnumDisplayName("AttRegularMode_LUNCH_START")]
            LUNCH_START = 996,
            [EnumDisplayName("AttRegularMode_LUNCH_END")]
            LUNCH_END = 997,
            [EnumDisplayName("AttRegularMode_OVERTIME_START")]
            OVERTIME_START = 998,
            [EnumDisplayName("AttRegularMode_OVERTIME_END")]
            OVERTIME_END = 999
        }

        /// <summary>
        /// 行業代碼
        /// </summary>
        public enum IndustryCode
        {
            IN00001 = 1, //Commercial service industry 0
            IN00002 = 2, // Catering 0
            IN00003 = 3, // Communication 0
            IN00005 = 4, // Household Services 0
            IN00006 = 5, // Education Services 0
            IN00007 = 6, // Finance 0
            IN00008 = 7, // government sector 0
            IN00009 = 8, // hospital 0
            IN00010 = 9, // Hospitality 0
            IN00011 = 10, // import and export trade 0
            IN00012 = 11, // Insurance 0
            IN00013 = 12, // Electronics 0
            IN00014 = 13, // Metal Products 0
            IN00015 = 14, // Plastics 0
            IN00016 = 15, // Textile 0
            IN00017 = 16, // Apparel 0
            IN00018 = 17, // real estate 0
            IN00019 = 18, // Retail 0
            IN00020 = 19, // warehouse 0
            IN00021 = 20, // Transportation 0
            IN00022 = 21, // welfare agency 0
            IN00023 = 22, // Wholesale 0
            IN00024 = 23, // Other Community and Social Services 0
            IN00025 = 24, // other manufacturing 0
            IN00026 = 25, // other personal services 0
            IN60006 = 26, // Construction
            IN60066 = 27 // chain	 
        }
    }

}
