using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    public enum TaskErrorCode
    {
        [EnumDisplayName("TASK_STOP_STATUS_DESC")]
        TASK_STOP_IN_VALUE = 0,
        [EnumDisplayName("TASK_STOP")]
        TASK_STOP = 3998,

        [EnumDisplayName("TASK_START_STATUS_DESC")]
        TASK_START_IN_VALUE = 1,
        [EnumDisplayName("TASK_START")]
        TASK_START = 3999,

        [EnumDisplayName("TASK_DEL_STATUS_NOT_VISIBLE")]
        TASK_DELETED = 0,

        [EnumDisplayName("TASK_DEL_STATUS_IS_VISIBLE")]
        TASK_IS_VISIBLE = 1,

        [EnumDisplayName("TASK_EXIST_THE_NAME")]
        TASK_EXIST_THE_NAME = 4000,

        [EnumDisplayName("TASK_ADD_SUCCESS")]
        TASK_ADD_SUCCESS = 40011,

        [EnumDisplayName("TASK_ADD_FAIL")]
        TASK_ADD_FAIL = 4001,

        [EnumDisplayName("TASK_LIST_FAIL")]
        TASK_LIST_FAIL = 4002,

        [EnumDisplayName("TASK_NOT_EXIST")]
        TASK_NOT_EXIST = 4003,

        [EnumDisplayName("TASK_DELTET_SUCCESS")]
        TASK_DELTET_SUCCESS = 4004,

        [EnumDisplayName("TASK_DETAILS_FAIL")]
        TASK_DETAILS_FAIL = 4005,

        [EnumDisplayName("TASK_UPDATE_SUCCESS")]
        TASK_UPDATE_SUCCESS = 4006,

        [EnumDisplayName("TASK_UPDATE_FAIL")]
        TASK_UPDATE_FAIL = 40066,

        [EnumDisplayName("TASK_DELETE_SUCCESS")]
        TASK_DELETE_SUCCESS = 4007,

        [EnumDisplayName("TASK_DELETE_FAIL")]
        TASK_DELETE_FAIL = 4008,

        [EnumDisplayName("TASK_START_SUCCESS")]
        TASK_START_SUCCESS = 4009,

        [EnumDisplayName("TASK_START_FAIL")]
        TASK_START_FAIL = 4010,

        [EnumDisplayName("TASK_STOP_SUCCESS")]
        TASK_STOP_SUCCESS = 4011,

        [EnumDisplayName("TASK_STOP_FAIL")]
        TASK_STOP_FAIL = 4012,

        [EnumDisplayName("TASK_ILLEGAL_NAME")]
        TASK_ILLEGAL_NAME = 4013
    }
    public enum TaskType
    {
        /// <summary>
        /// 任務類型: UNDEFINED 未定義的任務 (不適用於任務場景的任務，如考勤的警報數據等等)
        /// </summary>
        [EnumDisplayName("TASK_TYPE_UNDEFINED")]
        UNDEFINED = 0,  //NOT APPLICABLE (不適用)

        ///// <summary>
        ///// 任務類型:CAMERA_GUARD 桌面人臉識別系統
        ///// </summary>
        //[EnumDisplayName("TASK_TYPE_CAMERA_GUARD")]
        //CAMERA_GUARD = 1,

        ///// <summary>
        ///// 任務類型: CAMERA_DVR 桌面錄像系統
        ///// </summary>
        //[EnumDisplayName("TASK_TYPE_CAMERA_DVR")]
        //CAMERA_DVR = 2,

        ///// <summary>
        ///// 任務類型: HIK_DATA_RETRIVE 桌面海康設備數據獲取系統
        ///// </summary>
        //[EnumDisplayName("TASK_TYPE_DESKTOP_HIK_DATA_RETRIVE")]
        //DESKTOP_HIK_DATA_RETRIVE = 3,

        ///// <summary>
        ///// 任務類型: HIK_DATA_ANDROID_RETRIVE  手機版的海康設備數據獲取系統
        ///// </summary>
        //[EnumDisplayName("TASK_TYPE_ANDROID_HIK_DATA_RETRIVE")]
        //ANDROID_HIK_DATA_RETRIVE = 4,

        ///// <summary>
        ///// 任務類型: ANDROID_CIC_DATA_RETRIVE  手機版的 CIC的NFC拍卡數據獲取系統
        ///// </summary>
        //[EnumDisplayName("TASK_TYPE_ANDROID_CIC_DATA_RETRIVE")]
        //ANDROID_CIC_DATA_RETRIVE = 5,

        /// <summary>
        /// 任務類型: 车牌识别 POST到Python处理后返回
        /// </summary>
        [EnumDisplayName("TASK_TYPE_CAR_PLATE_RECOGNITION")]
        CAR_PLATE_RECOGNITION = 6,

        /// <summary>
        /// 任務類型: 人脸识别
        /// </summary>
        [EnumDisplayName("TASK_TYPE_FACE_RECOGNITION")]
        FACE_RECOGNITION = 7,

        /// <summary>
        /// 任務類型: 工程着装识别
        /// </summary>
        [EnumDisplayName("TASK_TYPE_WORK_CLOTHES_RECOGNITION")]
        WORK_CLOTHES_RECOGNITION = 8,

        /// <summary>
        /// 任務類型: 佩戴头盔识别
        /// </summary>
        [EnumDisplayName("TASK_TYPE_WEARING_HELMET_RECOGNITION")]
        WEARING_HELMET_RECOGNITION = 9,

        /// <summary>
        /// 任務類型: 有人闖入
        /// </summary> 
        [EnumDisplayName("TASK_TYPE_SOMEONE_BROKE_IN")]
        SOMEONE_BROKE_IN = 10,

        /// <summary>
        /// 任務類型: 火警
        /// </summary> 
        [EnumDisplayName("TASK_TYPE_FIRE_ALARM")]
        FIRE_ALARM = 11,

        /// <summary>
        /// 任務類型: 人脸檢測
        /// </summary>
        [EnumDisplayName("TASK_TYPE_FACE_DETECTION")]
        FACE_DETECTION = 12


    }
    /// <summary>
    /// 警報級別
    /// </summary>
    public enum AlarmLevel
    {
        /// <summary>
        /// 警報級別 包括 0=INFO ALARM 信息 1=危機警報 CRISIS ALARM 2=.緊急警報 EMERGENCY ALARM -1=不適用 NOT_APPLICABLE
        /// </summary>
        [EnumDisplayName("GeneralUI_NOT_APPLICABLE")]
        NOT_APPLICABLE = -1,  //NOT APPLICABLE (不適用)
        [EnumDisplayName("AlarmLevel_INFO_ALARM")]
        INFO_ALARM = 0,  //信息警報
        [EnumDisplayName("AlarmLevel_CRISIS_ALARM")]
        CRISIS_ALARM = 1, //危機警報
        [EnumDisplayName("AlarmLevel_EMERGENCY_ALARM")]
        EMERGENCY_ALARM = 2  //緊急警報
    }
}
