using System;
using System.Runtime.InteropServices;

namespace LanguageResource
{
    public static class ChineseStringUtility
    {
        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        public static string ToSimplified(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        public static string ToTraditional(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
        /// <summary>
        ///  如果是华语地区访问则自动转换为繁体(非中国大陆访问):
        /// </summary>
        /// <param name="SourceSimplified">识别主体内容为简体</param>
        /// <returns></returns>
        public static string ToAutoTraditional(string SourceSimplified)
        {
            if (string.IsNullOrEmpty(SourceSimplified))
            {
                return SourceSimplified;
            }
            LanguageDetector LanguageDetector1 = new LanguageDetector();
            string IdentifyLang = LanguageDetector1.Detect(SplitHtml(SourceSimplified));
            //如果识别主体是 IdentifyLang =="zh"(简体) 并且 系统语言是香港或者华语地区则 转换成繁体 并响应浏览器语言是当前 LangUtilities.LanguageCode
            //if ((LangUtilities.LanguageCode.ToLower() == "zh-tw" || LangUtilities.LanguageCode.ToLower() == "zh-hk") && IdentifyLang != "zh")
            //{ 
            //    SourceSimplified  = ChineseStringUtility.ToTraditional(SourceSimplified); 
            //}
            if (LangUtilities.LanguageCode.ToLower() == "zh-tw" || LangUtilities.LanguageCode.ToLower() == "zh-hk")
            {
                SourceSimplified = ChineseStringUtility.ToTraditional(SourceSimplified);
            }
            return SourceSimplified;
        }
        public static string SplitHtml(string Htmlstring)
        {

            Htmlstring = System.Text.RegularExpressions.Regex.Replace(Htmlstring, @"<[^>]*>", "");
            return Htmlstring;
        }
    }

}
