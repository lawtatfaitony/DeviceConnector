using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumCode
{
    /// <summary>
    /// Type 人員底庫類型 分為: 拍卡\人臉\指紋\GPS\二維碼\ 拍卡、人臉、指紋、GPS或二維碼
    /// Type Personnel database type is divided into: tap card\face\fingerprint\GPS\QR code\ tap card, face, fingerprint, GPS or QR code
    /// </summary>
    public enum LibraryTypeCode
    {
        /// <summary>
        /// 固定群組庫
        /// </summary>
        [EnumDisplayName("Library_Name")]
        LIB_FIX_GROUP = -1,

        /// <summary>
        /// 拍卡 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_TAP_CARD")]
        LIB_TAP_CARD = 0,

        /// <summary>
        /// 人臉 類型人員底庫
        /// </summary>
       [EnumDisplayName("LIB_FACE")]
        LIB_FACE = 1,

        /// <summary>
        /// 指紋 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_FINGERPRINT")]
        LIB_FINGERPRINT = 2,

        /// <summary>
        /// GPS 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_GPS")]
        LIB_GPS = 3,

        /// <summary>
        /// 二維碼 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_QR_CODE")]
        LIB_QR_CODE = 4,

        /// <summary>
        /// 拍卡、人臉、指紋、GPS或二維碼 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_CARD_FACE_FINGERPRINT_GPS_QRCODE")]
        LIB_CARD_FACE_FINGERPRINT_GPS_QRCODE = 5, 
        /// <summary>
        /// 電子圍欄 類型人員底庫
        /// </summary>
        [EnumDisplayName("LIB_ELECTRIC_FENCE")]
        LIB_ELECTRIC_FENCE = 6
 
    }
}
