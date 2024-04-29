using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public partial class CommonBase
    {
        public static string Substr(string str, int Lenght)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            str = str.Replace("\"", "");
            str = str.Replace("'", "");
            if (str.Length > Lenght)
            {
                return string.Format("{0}..", str.Substring(0, Lenght));
            }
            else
            {
                return str;
            }
        }
        public static string Substr2(string str, int StartIndex, int Lenght)
        {
            if (string.IsNullOrEmpty(str) || str.Length < Lenght)
            {
                return str;
            }
            str = str.Replace("\"", "");
            str = str.Replace("'", "");
            if (str.Length > Lenght)
            {
                return string.Format("{0}..", str.Substring(StartIndex, Lenght));
            }
            else
            {
                return str;
            }
        }
        private static readonly char[] constantNumber =
        {
               '0','1','2','3','4','5','6','7','8','9'
        };
        public static string GenerateNumberRandom(int Length)
        {

            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(10);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constantNumber[rd.Next(10)]);
            }
            return newRandom.ToString();
        }
        public static string HtmlEncode(string theString)
        {
            theString = theString.Replace(">", "&gt;");
            theString = theString.Replace("<", "&lt;");
            theString = theString.Replace(" ", "&nbsp;");
            theString = theString.Replace("\"", "&quot;");
            theString = theString.Replace("\'", "&#39;");
            theString = theString.Replace("\n", "<br/>");
            return theString;
        }
        public static string HtmlDecode(string theString)
        {
            theString = theString.Replace("&gt;", ">");
            theString = theString.Replace("&lt;", "<");
            theString = theString.Replace("&nbsp;", " ");
            theString = theString.Replace("&quot;", "\"");
            theString = theString.Replace("&#39;", "\'");
            theString = theString.Replace("<br/>", "\n");
            return theString;
        }
        public static string TransHtmlBr(string theString, string toStr)
        {
            theString = theString.Replace("<br>", toStr);
            return theString;
        }
        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToBase64(string str)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(str));
        }

        /// <summary>
        /// Base64 解密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64ToString(string str)
        {
            return System.Text.Encoding.Default.GetString(System.Convert.FromBase64String(str));
        }

        /// <summary>
        /// 去掉字符串中的数字
        /// </summary>
        public static string RemoveNumber(string key)
        {
            return Regex.Replace(key, @"\d", "");
        }

        /// <summary>
        /// 去掉字符串中的非数字
        /// </summary>
        public static string RemoveNotNumber(string key)
        {
            return Regex.Replace(key, @"[^\d]*", "");
        }
        public static bool IsIP(string ip)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// 判断中文字符(双字节字符
        /// </summary>
        /// <param name="CNstring"></param>
        /// <returns></returns>
        public static bool IsChineseString(string CNstring)
        {
            bool BoolValue = false;
            for (int i = 0; i < CNstring.Length; i++)
            {
                if (Convert.ToInt32(Convert.ToChar(CNstring.Substring(i, 1))) < Convert.ToInt32(Convert.ToChar(128)))
                {
                    BoolValue = false;
                }
                else
                {
                    return BoolValue = true;
                }
            }
            return BoolValue;
        }
        public static bool IsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            char[] chArray = new char[str.Length];
            chArray = str.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if ((chArray[i] < '0') || (chArray[i] > '9'))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsValidEmail(string strIn)
        {
            if (string.IsNullOrEmpty(strIn))
            {
                return false;
            }
            // Return true if strIn is in valid e-mail format. ref : https://msdn.microsoft.com/zh-tw/library/01escwtf(v=vs.100).aspx
            return Regex.IsMatch(strIn,
                   @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }
        /// <summary>
        /// 判断输入的字符串是否全是英文字母
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLetter(string input)
        {
            string pattern = @"^[A-Za-z]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
        public static bool IsDigitAndLetter(string strMessage, int iMinLong, int iMaxLong)
        {
            bool bResult = false;
            string pattern = @"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9a-zA-Z]+$";
            if (strMessage.Length >= iMinLong && strMessage.Length <= iMaxLong)//判断字符串长度
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(strMessage, pattern))
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                }
            }

            return bResult;
        }
        public static bool IsDigitOrLetter(string strMessage, int iMinLong, int iMaxLong)
        {
            bool bResult = false;

            //开头匹配一个字母或数字+匹配两个十进制数字+匹配一个字母或数字+匹配两个相同格式的的（-加数字）+已字母或数字结尾
            //如：1111-111-1119
            //string pattern = @"^[a-zA-Z0-9]\d{2}[a-zA-Z0-9](-\d{3}){2}[A-Za-z0-9]$";

            //string pattern = @"^[a-zA-Z0-9]\d{2}$"; //开头以字母或数字，然后后面加两个数字字符

            string pattern = @"^[a-zA-Z0-9]*$"; //匹配所有字符都在字母和数字之间

            //string pattern = @"^[a-z0-9]*$"; //匹配所有字符都在小写字母和数字之间

            //string pattern = @"^[A-Z][0-9]*$"; //以大写字母开头，后面的都是数字

            //string pattern = @"^\d{3}-\d{2}$";//匹配 333-22 格式,三个数字加-加两个数字

            if (strMessage.Length >= iMinLong && strMessage.Length <= iMaxLong)//判断字符串长度
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(strMessage, pattern))
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                }
            }

            return bResult;
        }
    }

}
