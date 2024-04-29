namespace EnumCode
{
    public enum CamMpegErrorCode
    {
        [EnumDisplayName("CamMpeg_EXIST_THE_NAME")]
        CamMpeg_VISIBLE = 1,  //也可以用GeneralVisibal 的通用常量
        CamMpeg_INVISIBLE = 0,

        [EnumDisplayName("CAMMPEG_THE_MPEG_FILENAME_IS_EMPTY")]
        CAMMPEG_THE_MPEG_FILENAME_IS_EMPTY = 8001,

        [EnumDisplayName("CamMpeg_EXIST_THE_NAME")]
        CamMpeg_EXIST_THE_NAME = 8002,

        [EnumDisplayName("CamMpeg_MPEG_WRONG_FORMAT")]
        CamMpeg_MPEG_WRONG_FORMAT = 8011,

        [EnumDisplayName("CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST")]
        CAMMPEG_DEVICE_SERIAL_NO_NOT_EXIST = 8012,

        [EnumDisplayName("CamMpeg_ADD_FAIL")]
        CamMpeg_ADD_FAIL = 8001,
        [EnumDisplayName("CAMMPEG_RECORE_ID_ERROR")]
        CAMMPEG_RECORE_ID_ERROR = 8001
    }
    /// <summary>
    /// 表 [ft_camera_mpeg].is_upload
    /// </summary>
    public enum CamMpegIsUpload
    {
        [EnumDisplayName("CamMpeg_DVR_RECORD_DEFAULT")]
        DVR_RECORD_DEFAULT = 0, //默認是DVR記錄  /初始增加一條錄像記錄下的默認值
        [EnumDisplayName("CamMpeg_UPLOAD_IN_PROGRESS")]
        UPLOAD_IN_PROGRESS = 1, //上存中
        [EnumDisplayName("CamMpeg_UPLOAD_COMPLETED")]
        UPLOAD_COMPLETED = 2, //上存完成
        [EnumDisplayName("CamMpeg_UPLOAD_DAMAGE")]
        UPLOAD_DAMAGE = 3, //上存文件損毀
        [EnumDisplayName("CamMpeg_CLOUD_DELETED")]  //不適用,現在只是DVR物理刪除
        CLOUD_DELETED = 4, //雲端刪除
        [EnumDisplayName("CamMpeg_DVR_DELETED")] //不適用,現在只是DVR物理刪除
        DVR_DELETED = 5 //DVR設備端刪除
    }

    /// <summary>
    /// 表 [ft_camera_mpeg].visible 是否可展示  //也可以用GeneralVisibal 的通用常量
    /// </summary>
    public enum CamMpegVisible
    {
        [EnumDisplayName("CamMpeg_VISIBLE")] //可展示
        VISIBLE = 1,  
        [EnumDisplayName("CamMpeg_INVISIBLE")] //不可展示
        INVISIBLE = 0
    } 
}
