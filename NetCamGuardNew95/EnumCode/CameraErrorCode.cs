namespace EnumCode
{
    public enum CameraErrorCode
    {
        [EnumDisplayName("CAM_NOT_VISIBLE")]
        CAM_NOT_VISIBLE = 0,

        [EnumDisplayName("CAM_IS_VISIBLE")]
        CAM_IS_VISIBLE = 1,

        [EnumDisplayName("CameraErrorCode_ExistTheName")]
        EXIST_THE_NAME = 2010,

        [EnumDisplayName("RTSP_WRONG_FORMAT")]
        RTSP_WRONG_FORMAT = 2011,

        [EnumDisplayName("CamAddNewResult_Fail")]
        CAMERA_ADD_FAIL = 2001,

        [EnumDisplayName("QUERY_CAMERA_LIST_FAIL")]
        QUERY_CAMERA_LIST_FAIL = 2002,

        [EnumDisplayName("GeneralUI_ILLEGAL_RTSP")]
        ILLEGAL_RTSP = 2003,

        [EnumDisplayName("CAM_CAMERA_NOT_EXIST")]
        CAMERA_NOT_EXIST = 2004,

        [EnumDisplayName("CAM_GET_CAMLIST_FAIL")]
        CAM_GET_CAMLIST_FAIL = 20041,

        [EnumDisplayName("CAM_EXIST_THE_SAME_NAME")]
        CAM_EXIST_THE_SAME_NAME = 2005,

        [EnumDisplayName("CAM_DELTET_SUCCESS")]
        CAM_DELTET_SUCCESS = 2006,

        [EnumDisplayName("CAM_GET_DETAILS_FAIL")]
        CAM_GET_DETAILS_FAIL = 2007,

        [EnumDisplayName("CAM_SITE_REQUIED")]
        CAM_SITE_REQUIED = 2008,

        [EnumDisplayName("CAM_TASK_EXISTS_REFERENCE")]
        CAM_TASK_EXISTS_REFERENCE = 2009,

        [EnumDisplayName("CAM_ONLIVE_OR_IN_RECORD_REJECT_DEL")]
        CAM_ONLIVE_OR_IN_RECORD_REJECT_DEL = 2010
    }

    /// <summary>
    /// 表 [ft_camera].visible 是否停用/使用中  //也可以用GeneralVisibal 的通用常量
    /// </summary>
    public enum CameraVisible
    {
        [EnumDisplayName("CAMERA_VISIBLE")] //使用中
        VISIBLE = 1,
        [EnumDisplayName("CAMERA_INVISIBLE")] //停用
        INVISIBLE = 0
    }

    /// <summary>
    /// Table : [ft_camera].[record_status] 鏡頭錄像狀態
    /// </summary>
    public enum CameraRecordStatus
    {
        [EnumDisplayName("CAMERA_IN_STOP")] //錄像停止状态
        IN_STOP = 0,
        [EnumDisplayName("CAMERA_IN_RECORD")] //錄像進行中
        IN_RECORD = 1,
        [EnumDisplayName("CAMERA_SUSPEND_RECORD")] //錄像暫停中
        SUSPEND_RECORD = 2
    }
}
