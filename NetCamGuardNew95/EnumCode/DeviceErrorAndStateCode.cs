using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    /// <summary>
    /// 設備操作返回錯誤碼 對照
    /// </summary>
    public enum DeviceErrorCode
    {
        
        [EnumDisplayName("SUCC_UPD_DEVICE_CAMERA_STATUS_LIST")]
        SUCC_UPD_DEVICE_CAMERA_STATUS_LIST = GeneralReturnCode.SUCCESS,

        [EnumDisplayName("DEVICE_ADDNEW_FAIL")]
        ADDNEW_FAIL = 60000,
        [EnumDisplayName("DEVICE_ADDNEW_SUCCESS")]
        ADDNEW_SUCCESS = 60001,
        [EnumDisplayName("DEVICE_EXIST_THE_SAME_NAME")]
        EXIST_THE_SAME_NAME = 60002,

        [EnumDisplayName("DEVICE_SITE_REQUIED")]
        SITE_REQUIED = 60003,
        [EnumDisplayName("DEVICE_NOT_EXIST")]
        DEVICE_NOT_EXIST = 60003,
        [EnumDisplayName("DEVICE_SERIAL_NO_OCCUPIED")]
        DEVICE_SERIAL_NO_OCCUPIED = 60004,
        [EnumDisplayName("Device_ExistDeviceSerialNoTips")]
        EXIST_THE_SAME_DEVCIE_SERIALNO=60005,
        [EnumDisplayName("Device_IlleggleDeviceId")]
        ILLEGAL_DEVICE_ID = 60006,
        [EnumDisplayName("DEVICE_DELETE_SUCCSS")]
        DEVICE_DELETE_SUCCSS = 60007,
        [EnumDisplayName("DEVICE_DELETE_FAIL")]
        DEVICE_DELETE_FAIL = 60008,
        [EnumDisplayName("DEVICE_HAS_STILL_INUSE")]
        DEVICE_HAS_STILL_INUSE = 60009,
        [EnumDisplayName("ILLEGAL_DEVICE_SERIAL_NUMBER")]
        ILLEGAL_DEVICE_SERIAL_NUMBER = 60007,

        [EnumDisplayName("GENERALUI_DEVICE_NOT_SYNC_UNIXTIME")]
        GENERALUI_DEVICE_NOT_SYNC_UNIXTIME = 60008,

        [EnumDisplayName("GENERALUI_DEVICE_TOKEN_ILLEGGLE")]
        GENERALUI_DEVICE_TOKEN_ILLEGGLE = 60009

    }


    /// <summary>
    /// value 的int值要和 DevicePersonDownInsertStatus/DevicePersonDownUpdateStatus/DevicePersonDownDelStatus 三者一致,否則無法解析為對應的enum
    /// </summary>
    public enum DevicePersonDownStatus
    {
        DEVICE_PERSON_DOWN_NO_OPERATE = -1,
        DEVICE_PERSON_DOWN_WAIT = 0,  
        DEVICE_PERSON_DOWN_COMPLETE = 1,
        DEVICE_PERSON_DOWN_FAIL = 2
    }
    /// <summary>
    /// 下行用戶到設備的增加狀態
    /// </summary>
    public enum DevicePersonDownInsertStatus
    {
        [EnumDisplayName("DEVICE_PERSON_DOWN_INSERT_NO_OPERATE")]  //無操作
        DEVICE_PERSON_DOWN_INSERT_NO_OPERATE = -1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_INSERT_WAIT")] //等待
        DEVICE_PERSON_DOWN_INSERT_WAIT = 0,

        [EnumDisplayName("DEVICE_PERSON_DOWN_INSERT_COMPLETE")] //完成
        DEVICE_PERSON_DOWN_INSERT_COMPLETE = 1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_INSERT_FAIL")] //完成
        DEVICE_PERSON_DOWN_INSERT_FAIL = 2
    }
    /// <summary>
    /// 下行用戶到設備的更新狀態
    /// </summary>
    public enum DevicePersonDownUpdateStatus
    {
        [EnumDisplayName("DEVICE_PERSON_DOWN_UPDATE_NO_OPERATE")]  //無操作
        DEVICE_PERSON_DOWN_UPDATE_NO_OPERATE = -1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_UPDATE_WAIT")] //等待
        DEVICE_PERSON_DOWN_UPDATE_WAIT = 0,

        [EnumDisplayName("DEVICE_PERSON_DOWN_UPDATE_COMPLETE")] //完成
        DEVICE_PERSON_DOWN_UPDATE_COMPLETE = 1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_UPDATE_FAIL")] //完成
        DEVICE_PERSON_DOWN_UPDATE_FAIL = 2
    }
    /// <summary>
    /// 下行用戶到設備的刪除狀態
    /// </summary>
    public enum DevicePersonDownDelStatus
    {
        [EnumDisplayName("DEVICE_PERSON_DOWN_DEL_NO_OPERATE")]  //無操作
        DEVICE_PERSON_DOWN_DEL_NO_OPERATE = -1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_DEL_WAIT")] //等待
        DEVICE_PERSON_DOWN_DEL_WAIT = 0,

        [EnumDisplayName("DEVICE_PERSON_DOWN_DEL_COMPLETE")] //完成
        DEVICE_PERSON_DOWN_DEL_COMPLETE = 1,

        [EnumDisplayName("DEVICE_PERSON_DOWN_DEL_FAIL")] //完成
        DEVICE_PERSON_DOWN_DEL_FAIL = 2
    }
    /// <summary>
    /// 驗證介質模式
    /// 請求介質類型 例如 指紋 的人員 則 FINGERPRINT ( ACCESS_CARD | FINGERPRINT | FACE | PASSKEY )
    /// </summary>
    public enum DeviceVerifyMode
    {
        [EnumDisplayName("DEVICE_VERIFY_MODE_ACCESS_CARD")]  //ACCESS_CARD
        ACCESS_CARD = -1,

        [EnumDisplayName("DEVICE_VERIFY_MODE_FINGERPRINT")] //FINGERPRINT
        FINGERPRINT = 0,

        [EnumDisplayName("DEVICE_VERIFY_MODE_FACE")] //FACE
        FACE = 1,

        [EnumDisplayName("DEVICE_VERIFY_MODE_PASSKEY")] //PASSKEY
        PASSKEY = 2,

        [EnumDisplayName("DEVICE_VERIFY_MODE_ALL_REQUEST_MODE")] //ALL_REQUEST_MODE 默認是所有用戶
        ALL_REQUEST_MODE = 3
    }
    /// <summary>
    /// 同步模式 分为三种 INSERT UPDATE DELETE
    /// </summary>
    public enum DeviceOperateMode
    {
        [EnumDisplayName("DEVICE_OPERATE_MODE_ALL")] //所有操作
        DEVICE_OPERATE_MODE_ALL = -1,

        [EnumDisplayName("DEVICE_OPERATE_MODE_INSERT")]  //同步增加
        DEVICE_OPERATE_MODE_INSERT = 1,

        [EnumDisplayName("DEVICE_OPERATE_MODE_UPDATE")] //同步更新
        DEVICE_OPERATE_MODE_UPDATE = 2,

        [EnumDisplayName("DEVICE_OPERATE_MODE_DELETE")] //同步删除
        DEVICE_OPERATE_MODE_DELETE = 3
    }
}
