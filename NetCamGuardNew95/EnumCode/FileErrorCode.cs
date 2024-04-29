using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    /// <summary>
    /// PictureErrorCode rename as [PersonCode]. ONLY REPRESENT FOR A CASE OF CODE TO RETURN. 
    /// </summary>
    public enum PictureErrorCode
    { 
        [EnumDisplayName("PICTURE_NOT_VISIBLE")]
        PICTURE_NOT_VISIBLE = 0,

        [EnumDisplayName("PICTURE_IS_VISIBLE")]
        PICTURE_IS_VISIBLE = 1,

        [EnumDisplayName("PICTURE_EXIST")]
        PICTURE_EXIST = 6000,

        [EnumDisplayName("PERSON_EXIST_THE_NAME")]
        PICTURE_NOT_EXIST = 60001,

        [EnumDisplayName("PICTURE_VALID")]
        PICTURE_VALID = 6002,

        [EnumDisplayName("PICTURE_INVALID")]
        PICTURE_INVALID = 60003,

        [EnumDisplayName("PICTURE_UPDATE_FAIL")]
        PICTURE_UPDATE_FAIL = 60004,
    }
}