using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Common
{

    public partial class CommonBase
    {
        public static string BasePath
        {
            get
            {
                string basePath;

                basePath = AppDomain.CurrentDomain.BaseDirectory;
                if (OSHelper.IsLowVsersion())
                {
                    basePath = Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    basePath = Path.GetDirectoryName(basePath);
                    return basePath;
                }
                return basePath;
            }
        }

        public static void ConsoleWriteline(string Loggerline)  //
        {
            string consoleLog = string.Format("[{0:yyyy-MM-dd HH:mm:ss fff}] [{1}]", DateTime.Now, Loggerline);
            Console.WriteLine("");
            Console.WriteLine(consoleLog);
            CommonBase.OperateDateLoger(Loggerline, CommonBase.LoggerMode.INFO);
            //LogHelper.Info(Loggerline);
            return;
        }

        public static string PathRemoveBin(string pathApp)
        {
            int pathIndex = pathApp.LastIndexOf("\\");
            if (pathIndex != -1)
            {
                string existBinPath = pathApp.Remove(0, pathIndex).ToLower();
                existBinPath = existBinPath.TrimStart('\\');

                if (existBinPath.ToLower() == "bin")
                {
                    pathApp = pathApp.Substring(0, pathIndex);
                    return pathApp;
                }
                else
                {
                    return pathApp.Substring(0, pathIndex);
                }
            }
            else
            {
                return pathApp;
            }
        }
        /// <summary>
        /// MillionSecond
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ConvertLongToDateTime(long timeStamp)
        {
            DateTime dtStart = new System.DateTime(1970, 1, 1).ToLocalTime();
            long lTime = timeStamp * 10000;  
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public static string SHA1encode(string txt)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(txt);
            HashAlgorithm hash = HashAlgorithm.Create();
            MemoryStream stream = new MemoryStream(bytes);
            return hash.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }
        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string md5Encode(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Default.GetBytes(str);
            byte[] md5data = md5.ComputeHash(data);
            md5.Clear();
            string re_str = "";
            for (int i = 0; i < md5data.Length; i++)
            {
                re_str += md5data[i].ToString("x").PadLeft(2, '0');
            }
            return re_str;
        }
         
        /// <summary>
        /// MD5 32位小写 可指定16位
        /// </summary>
        /// <param name="ConvertString"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string ConvertString, int code = 32)
        {
            string strEncrypt = string.Empty;

            if (code == 16)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                string t2 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(ConvertString)), 4, 8);
                t2 = t2.Replace("-", "");
                strEncrypt = t2.ToLower();   //所有字符转为小写
            }

            if (code == 32)
            {
                string pwd = "";
                string temp = "";
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
                byte[] bs = md5.ComputeHash(Encoding.UTF8.GetBytes(ConvertString));
                // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                    sb.Append(b.ToString("X2"));
                }
                strEncrypt = sb.ToString().ToLower();  //所有字符转为小写 
            }

            return strEncrypt;
        }

        /// <summary>
        /// 验证指定长度的MD5 小寫
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="length">MD5长度（默认32）</param>
        /// <returns></returns>
        public static bool IsMd5LowerCase(string plateText,int length = 32)
        {
            if (plateText.Length < length || plateText.Length > length)
                return false;

            int count = 0;
            var charArray = "0123456789abcdefabcdef".ToCharArray();

            foreach (var c in plateText.ToCharArray())
            {
                if (charArray.Any(x => x == c))
                    ++count;
            }
            return count == length;
        }
        public static bool IsCurrentPage(int i, int PageIndex)
        {
            return i == PageIndex;
        }
        public static string ReturnCurrentActive(bool IsCurrentPage)
        {
            if (IsCurrentPage)
            {
                return "active";
            }
            else
            {
                return "";
            }
        }

        public static string HMACSHA1Encode(string input, string strkey)
        {
            byte[] keyX = Encoding.ASCII.GetBytes(strkey);
            HMACSHA1 myhmacsha1 = new HMACSHA1(keyX);
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }
        /// <summary>
        /// 以當年為key 進行非對稱加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HMACSHA1Encode(string input)
        {
            string strkey = DateTime.Now.Year.ToString();
            byte[] keyX = Encoding.ASCII.GetBytes(strkey);
            HMACSHA1 myhmacsha1 = new HMACSHA1(keyX);
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }
        public static string DoPost(string url, string data)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            byte[] postData = Encoding.UTF8.GetBytes(data);
            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length); reqStream.Close();
            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
            return GetResponseAsString(rsp, encoding);
        }
        public static HttpWebRequest GetWebRequest(string url, string method)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ServicePoint.Expect100Continue = false;
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            req.ContentType = "text/json";
            req.Method = method;
            req.KeepAlive = true;
            req.Timeout = 1000000;
            req.Proxy = null;
            return req;
        }
        public static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            StringBuilder result = new StringBuilder();
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                // 每次读取不大于256个字符，并写入字符串
                char[] buffer = new char[256];
                int readBytes = 0;
                while ((readBytes = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    result.Append(buffer, 0, readBytes);
                }
            }
            finally
            {
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
            return result.ToString();
        }
        public static DateTimeRangeObj DateTimeRangeParse(string strDateTimeRange)
        {
            DateTimeRangeObj dateTimeRangeObj = new DateTimeRangeObj();
            string[] arrDateTimeRange = strDateTimeRange.Split(new char[] { '-', '/', 'T', ' ' });
            List<string> list = new List<string>();
            foreach (string item in arrDateTimeRange.ToList())
            {
                if (item.Trim().Length > 0)
                    list.Add(item);
            }
            arrDateTimeRange = list.ToArray();

            string strStart, strEnd;
            if (arrDateTimeRange.Count() == 6)
            {
                strStart = string.Format("{0}-{1}-{2}", arrDateTimeRange[0], arrDateTimeRange[1], arrDateTimeRange[2]);
                if (!DateTime.TryParseExact(strStart, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTimeRangeObj.Start))
                {
                    dateTimeRangeObj.Start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                }
                strEnd = string.Format("{0}-{1}-{2}", arrDateTimeRange[3], arrDateTimeRange[4], arrDateTimeRange[5]);
                if (!DateTime.TryParseExact(strEnd, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTimeRangeObj.End))
                {
                    dateTimeRangeObj.End = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                }
            }
            else
            {
                strStart = string.Format("{0}-{1}-{2} {3}", arrDateTimeRange[0], arrDateTimeRange[1], arrDateTimeRange[2], arrDateTimeRange[3]);
                if (!DateTime.TryParseExact(strStart, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTimeRangeObj.Start))
                {
                    dateTimeRangeObj.Start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                }
                strEnd = string.Format("{0}-{1}-{2} {3}", arrDateTimeRange[4], arrDateTimeRange[5], arrDateTimeRange[6], arrDateTimeRange[7]);
                if (!DateTime.TryParseExact(strEnd, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTimeRangeObj.End))
                {
                    dateTimeRangeObj.End = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                }
            }
            return dateTimeRangeObj;
        }
        #region GetPicThumbnail  
        public static bool GetPicThumbnail(string sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;
            Bitmap ob;
            //按比例缩放 
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Height > dWidth) //将**改成c#中的或者操作符号
            {
                if ((tem_size.Width * dHeight) > (tem_size.Height * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                    ob = new Bitmap(sW, sH);
                }
                else
                {
                    sH = dHeight;
                    sW = (dHeight * tem_size.Width) / tem_size.Height;
                    ob = new Bitmap(sW, sH);
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
                ob = new Bitmap(sW, sH);
            }

            Graphics g = Graphics.FromImage(ob);
            g.Clear(Color.White); //g.Clear(Color.Transparent); 透明
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(iSource, new Rectangle(0, 0, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            //以下代码为保存图片时，设置压缩质量 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100 
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径 
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();

            }
        }
        #endregion
        public static string GetProperties<T>(T t)
        {
            string tStr = string.Empty;
            if (t == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    tStr += string.Format("{0}:{1},", name, value);
                }
                else
                {
                    GetProperties(value);
                }
            }
            return tStr;
        }

        /// <summary>
        /// Judge Has Property 
        /// </summary>
        /// <param name="PropertyName">PropertyName</param>
        /// <param name="o">Object</param>
        /// <returns></returns>
        public static bool JudgeHasProperty(string PropertyName, Object o)
        {
            if (o == null)
            {
                o = new { };
            }
            PropertyInfo[] p1 = o.GetType().GetProperties();
            bool b = false;
            foreach (PropertyInfo pi in p1)
            {
                if (pi.Name.ToLower() == PropertyName.ToLower())
                {
                    b = true;
                }
            }
            return b;
        }
    }
    public class DateTimeRangeObj
    {
        public DateTime Start;
        public DateTime End;
    }
    public enum PictureSize
    {
        IsNotPict = 0, s48X48 = 1, s60X60 = 2, s100X100 = 3, s160X160 = 4, s230X230 = 5, s250X250 = 6, s310X310 = 7, s350X350 = 8, s600X600 = 9
    }
    /// <summary>
    /// Thumbnail file name suffix
    /// </summary>
    public class PictureSuffix
    {
        public static string ReturnSizePicUrl(string PicUrl, PictureSize pictureSize)
        {
            if (PicUrl.ToLower().IndexOf("gif") != -1)
            {
                return PicUrl + pictureSize + ".gif";
            }
            if (PicUrl.ToLower().IndexOf("png") != -1)
            {
                return PicUrl + pictureSize + ".png";
            }
            if (PicUrl.ToLower().IndexOf("jpeg") != -1)
            {
                return PicUrl + pictureSize + ".jpeg";
            }
            return PicUrl + pictureSize + ".jpg";

        }
    }
}
