using System;
using System.ComponentModel;
using System.Reflection;
namespace EnumCode
{
    public enum GeneralLanguageCode
    {
        /// <summary>
        /// 简体
        /// </summary>
        [EnumDisplayName("GeneralUI_zhCN")]
        ZHCN = -1,

        /// <summary>
        /// 繁体
        /// </summary>
        [EnumDisplayName("GeneralUI_zhHK")]
        ZHHK = 1,

        /// <summary>
        /// 英文
        /// </summary>
        [EnumDisplayName("GeneralUI_enUS")]
        ENUS = 2 
    }
    /// <summary>
    /// For api 等等ResponseX 對象的 ErrorCode 的一部分常規代碼,
    /// 其他的在對應的業務常量對照表
    /// </summary>
    public enum GeneralReturnCode
    {
        /// <summary>
        /// GeneralUI_SUCC
        /// </summary>
        [EnumDisplayName("GeneralUI_SUCC")]
        SUCCESS = -1,

        /// <summary>
        /// GeneralUI_Fail
        /// </summary>
        [EnumDisplayName("GeneralUI_Fail")]
        FAIL = 1,

        /// <summary>
        /// GeneralUI_EXCEPTION
        /// </summary>
        [EnumDisplayName("GeneralUI_EXCEPTION")]
        EXCEPTION = 2,

        /// <summary>
        /// GeneralUI_UNAUTHORIED
        /// </summary>
        [EnumDisplayName("GeneralUI_UNAUTHORIED")]
        UNAUTHORIED = 3,

        /// <summary>
        /// GeneralUI_PAGE_NO_ERR
        /// </summary>
        [EnumDisplayName("GeneralUI_PAGE_NO_ERR")]
        GENERALUI_PAGE_NO_ERR = 4,

        /// <summary>
        /// GeneralUI_NoRecord
        /// </summary>
        [EnumDisplayName("GeneralUI_NoRecord")]
        GENERALUI_NO_RECORD = 5,

        /// <summary>
        /// GeneralUI_ExistRecord
        /// </summary>
        [EnumDisplayName("GeneralUI_ExistRecord")]
        GENERALUI_EXIST_RECORD =13,  //和EVENT DEPLOY/DGX 要一致
        /// <summary>
        /// FILE_UPLOAD_SUCCESS
        /// </summary>
        [EnumDisplayName("FILE_UPLOAD_SUCCESS")]
        FILE_UPLOAD_SUCCESS = 6,

        /// <summary>
        /// 上存的文件已经存在，不重新保存（不考虑文件冲突问题）
        /// </summary>
        [EnumDisplayName("FILE_UPLOAD_EXISTS")]
        FILE_UPLOAD_EXISTS = 66,
        /// <summary>
        /// FILE_UPLOAD_FAIL
        /// </summary>
        [EnumDisplayName("FILE_UPLOAD_FAIL")]
        FILE_UPLOAD_FAIL = 7,

        /// <summary>
        /// FILESIZE_IS_LIMITED
        /// </summary>
        [EnumDisplayName("FILESIZE_IS_LIMITED")]
        FILESIZE_IS_LIMITED = 8,

        /// <summary>
        /// LIST_NO_RECORD
        /// </summary>
        [EnumDisplayName("LIST_NO_RECORD")]
        LIST_NO_RECORD = 9,

        /// <summary>
        /// GeneralUI_NoMatchMainComId
        /// </summary>
        [EnumDisplayName("GeneralUI_NoMatchMainComId")]
        NO_MATCH_MAINCOMID = 91,

        /// <summary>
        /// Config_FAIL 配置失败
        /// </summary>
        [EnumDisplayName("Config_FAIL")]
        CONFIG_FAIL = 10,

        [EnumDisplayName("GeneralUI_MainComIdRequired")]
        REQUIRED_CORRECT_PARMS_MAINCOM_ID = 11,

        [EnumDisplayName("GENERALUI_NO_DEV_POST_RECORD")]
        GENERALUI_NO_DEV_POST_RECORD = 12,

        [EnumDisplayName("DEVICE_SERIALNO_NOT_EXIST")]
        DEVICE_SERIALNO_NOT_EXIST = 16  //数值和HIK DEPLOY EVENT 对应一致

    }
     
    /// <summary>
    /// 通用狀態
    /// </summary>
    public enum GeneralStatus
    {
        [EnumDisplayName("GeneralUI_Undefined")]
        UNDEFINED = -1,

        [EnumDisplayName("GeneralUI_ACTIVE")]
        ACTIVE = 1,

        [EnumDisplayName("GeneralUI_DEACTIVE")]
        DEACTIVE = 0 
    }

    public enum GeneralVisible
    {
        [EnumDisplayName("GeneralUI_VISIBLE")]
        VISIBLE = 1,

        [EnumDisplayName("GeneralUI_INVISIBLE")]
        INVISIBLE = 0
    }

    /// <summary>
    /// 人 , 陌生人 , 汽車(停車場用),危險品(刀具),狗,
    /// </summary>
    public enum CLASSIFY
    {  
        [EnumDisplayName("GeneralUI_STRANGER")]
        STRANGER = 0, 
        [EnumDisplayName("GeneralUI_IS_PERSON")]
        IS_PERSON = 1,
        [EnumDisplayName("GeneralUI_IS_CAR")]
        IS_CAR = 2,
        [EnumDisplayName("GeneralUI_IS_DOG")]
        IS_DOG = 3,
        [EnumDisplayName("GeneralUI_IS_CAT")]
        IS_CAT = 4
    }
    public class EnumItem
    {
        public string Text;
        public string Value;
        public bool Selected = false;
    }
}
