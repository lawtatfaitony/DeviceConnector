
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{ 
    public class DateTimeHelp
    { 
        public static long ToUnixTimeMilliseconds(System.DateTime datetime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(datetime); 
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }
        public static long ToUnixTimeSeconds(System.DateTime datetime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(datetime);
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 測試過ok .必須是毫秒單位的unix time stamp format E.g.:1572321217157 ( 13bit )
        /// 測試過ok 2022年7月11日
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(long timeStamp) 
        {
            if (timeStamp.ToString().Length == 10)
                timeStamp = ConvertToMillSecond(timeStamp);

            DateTime dtStart = new System.DateTime(1970, 1, 1).ToLocalTime(); 
            long lTime = timeStamp * 10000; 
            TimeSpan toNow = new TimeSpan(lTime);  
            return dtStart.Add(toNow);
        }
        /// <summary>
        /// 把10位的(秒)轉為13位的毫秒
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static long ConvertToMillSecond(long timeStamp)
        {
            if (timeStamp.ToString().Length == 10)
                timeStamp = timeStamp * 1000;

            return timeStamp; 
        }

        /// <summary>
        /// 取得某月的第一天
        /// </summary>
        /// <param name="datetime">要取得月份第一天的时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }
        /**//// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(DateTime datetime)
        {
            DateTime dt = datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
            TimeSpan ts = new TimeSpan(23, 59, 59);
            DateTime endMonthDateTime = new DateTime(dt.Year, dt.Month, dt.Day, ts.Hours, ts.Minutes, ts.Seconds, 999);
            return endMonthDateTime;
        }
    }
    
}