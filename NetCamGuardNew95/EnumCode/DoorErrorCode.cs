namespace EnumCode
{
    public enum DoorErrorCode
    { 
        [EnumDisplayName("DOOR_EXIST_THE_SAME_NAME")]
        DOOR_EXIST_THE_SAME_NAME = 6020,
        [EnumDisplayName("DOOR_RUQIRED_SITE")]
        DOOR_RUQIRED_SITE = 6021,
        [EnumDisplayName("DOOR_RUQIRED_DEVICE")]
        DOOR_RUQIRED_DEVICE = 6022
    }

    /// <summary>
    /// 門禁狀態
    /// </summary>
    public enum DoorStatus
    {
        [EnumDisplayName("DOOR_UNKOWN")] //門在未知狀態
        DOOR_UNKOWN = -1,
        [EnumDisplayName("DOOR_OPEN")] //門開啟
        DOOR_OPEN = 0,
        [EnumDisplayName("DOOR_CLOSED")] //門關閉
        DOOR_CLOSED = 1,
        [EnumDisplayName("DOOR_STAY_OPEN")] //門持續開啟
        DOOR_STAY_OPEN = 2,
        [EnumDisplayName("DOOR_STAY_CLOSED")] //門持續關閉
        DOOR_STAY_CLOSED = 3
    }
}
