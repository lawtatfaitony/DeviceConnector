using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    /// <summary>
    /// PersonErrorCode rename as [PersonCode]. ONLY REPRESENT FOR A CASE OF CODE TO RETURN. 
    /// </summary>
    public enum PersonErrorCode
    {
        [EnumDisplayName("PERSON_NOT_VISIBLE")]
        PERSON_NOT_VISIBLE = 0,

        [EnumDisplayName("PERSON_IS_VISIBLE")]
        PERSON_IS_VISIBLE = 1,

        [EnumDisplayName("PERSON_INVLID_LIBRARY_ID")]
        PERSON_INVLID_LIBRARY_ID = 4999,

        [EnumDisplayName("PERSON_EXIST_THE_NAME")]
        PERSON_EXIST_THE_NAME = 5000,

        [EnumDisplayName("PERSON_INVALID_NAME")]
        PERSON_INVALID_NAME = 50001,

        [EnumDisplayName("PERSON_ADD_FAIL")]
        PERSON_ADD_FAIL = 5001,

        [EnumDisplayName("PERSON_LIST_FAIL")]
        PERSON_LIST_FAIL = 5002,

        [EnumDisplayName("PERSON_NOT_EXIST")]
        PERSON_NOT_EXIST = 5003,

        [EnumDisplayName("PERSON_DELTET_SUCCESS")]
        PERSON_DELTET_SUCCESS = 5004,

        [EnumDisplayName("PERSON_DETAILS_FAIL")]
        PERSON_DETAILS_FAIL = 5005,

        [EnumDisplayName("PERSON_UPDATE_FAIL")]
        PERSON_UPDATE_FAIL = 5006,

        [EnumDisplayName("PERSON_DELETE_FAIL")]
        PERSON_DELETE_FAIL = 5007,

        [EnumDisplayName("PERSON_EXIST_PERSON")]
        PERSON_EXIST_PERSON = 5008,

        [EnumDisplayName("PERSON_EXIST_OUTTER_ID")]
        PERSON_EXIST_OUTTER_ID = 5009,

        [EnumDisplayName("PERSON_UNFORMAT_OUTTER_ID")]
        PERSON_UNFORMAT_OUTTER_ID = 50099,

        [EnumDisplayName("PERSON_CARD_NUMBER_OCCUPIED")]
        PERSON_CARD_NUMBER_OCCUPIED = 5010,

        [EnumDisplayName("PERSON_CARD_NUMBER_NOT_OCCUPIED")]
        PERSON_CARD_NUMBER_NOT_OCCUPIED = 5011,

        [EnumDisplayName("PERSON_CARD_NUMBER_UNFORMAT")]
        PERSON_CARD_NUMBER_UNFORMAT = 5012,

        [EnumDisplayName("PERSON_INVALID_PASSKEY")]
        PERSON_INVALID_PASSKEY = 5013,

    }
}
