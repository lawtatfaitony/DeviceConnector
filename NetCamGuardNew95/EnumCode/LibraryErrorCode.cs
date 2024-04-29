using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    public enum LibraryErrorCode
    {
        [EnumDisplayName("LIB_DEL_STATUS_NOT_VISIBLE")]
        LIB_DELETED = 0,

        [EnumDisplayName("LIB_DEL_STATUS_IS_VISIBLE")]
        LIB_IS_VISIBLE = 1,

        [EnumDisplayName("LIB_EXIST_THE_NAME")]
        LIB_EXIST_THE_NAME = 3000, 

        [EnumDisplayName("LIB_ADD_FAIL")]
        LIB_ADD_FAIL = 3001,

        [EnumDisplayName("LIB_LIST_FAIL")]
        LIB_LIST_FAIL = 3002, 

        [EnumDisplayName("LIB_NOT_EXIST")]
        LIB_NOT_EXIST = 3003,
         
        [EnumDisplayName("LIB_DELTET_SUCCESS")]
        LIB_DELTET_SUCCESS = 3004,

        [EnumDisplayName("LIB_DETAILS_FAIL")]
        LIB_DETAILS_FAIL = 3005,

        [EnumDisplayName("LIB_UPDATE_SUCCESS")]
        LIB_UPDATE_SUCCESS = 3006,

        [EnumDisplayName("LIB_UPDATE_FAIL")]
        LIB_UPDATE_FAIL = 30060,

        [EnumDisplayName("LIB_DELETE_SUCCESS")]
        LIB_DELETE_SUCCESS = 3007,

        [EnumDisplayName("LIB_DELETE_FAIL")]
        LIB_DELETE_FAIL = 3008,
        [EnumDisplayName("LIB_GET_DETAILS_FAIL")]
        LIB_GET_DETAILS_FAIL = 3009,

        [EnumDisplayName("LIB_EXIST_THE_SAME_NAME")]
        LIB_EXIST_THE_SAME_NAME = 30010,

        [EnumDisplayName("LIB_EXIST_PERSON_DEL_NOT_ALLOW")]
        LIB_EXIST_PERSON_DEL_NOT_ALLOW = 30011
    }
}
