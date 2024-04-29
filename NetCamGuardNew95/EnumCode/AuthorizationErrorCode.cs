using System;
using System.ComponentModel;
using System.Reflection;
namespace EnumCode
{ 
    public enum AuthorizationErrorCode
    {
        [EnumDisplayName("GeneralUI_AUTH_WRONG_USERNAME_OR_PASSWORD")]
        WRONG_USERNAME_OR_PASSWORD = 0,
        [EnumDisplayName("GeneralUI_LoginFail")]
        LONGIN_FAIL = 10001,
        [EnumDisplayName("GeneralUI_SUCC")]
        SUCCESS = -1
    }
    public enum DeviceAuthorizationErrorCode
    {
        [EnumDisplayName("DEVICE_AUTHORIZATION_ERROR")]
        DEVICE_AUTHORIZATION_ERROR = 0,
        SUCCESS = -1
    }
    public enum LoginErrorCode
    {
        [EnumDisplayName("GeneralUI_LoginSucc")]
        SUCCESS = -1,
        [EnumDisplayName("GeneralUI_OrginalPasswordNotMatch")]
        OrginalPasswordNotMatch = 10023
    }

    public enum RegisterErrorCode
    { 
        [EnumDisplayName("Register_Fail")]
        RREGISTER_FAIL = 10021,
        [EnumDisplayName("Register_ExistUserName")]
        Register_ExistUserName = 10022
    }
}
