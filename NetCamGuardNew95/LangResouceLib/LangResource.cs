using Caching;
using LanguageResource.Modal;
using LanguageResource.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.RegularExpressions;
namespace LanguageResource
{
    public enum LangParamsType { Route = 1, CultureUI = 2 }

    

    public partial class LangUtilities
    {
        private static LangResourceContext db = new LangResourceContext();

        private static string _LanguageCode;
        public static readonly FIFOCache<string, byte[]> cache = RunTimeCache.FIFOCache();

        public static string LanguageCode
        {
            get
            {
                try
                { 
                    if(!string.IsNullOrEmpty(_LanguageCode))
                    {
                        return _LanguageCode;
                    }else
                    {
                        _LanguageCode = StandardLanguageCode(Thread.CurrentThread.CurrentCulture.Name); // Templary to use this method.
                        return _LanguageCode;
                    } 
                }
                catch
                { 
                    _LanguageCode = Thread.CurrentThread.CurrentCulture.Name;
                    return _LanguageCode;
                }
            }
            set
            {
                _LanguageCode = StandardLanguageCode(value);

                try
                {
                    //Localize
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_LanguageCode);
                    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_LanguageCode);
                }
                catch
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// 通过KeyName 获取对应的语言域
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetString(string keyName)
        {
            Language LanguageDetails = new Language();
            var getLanguages = db.Language; //.Database.SqlQuery<Language>("select * from Language");
            LanguageDetails = getLanguages.Where(c => c.KeyName.Contains(keyName)).FirstOrDefault<Language>();
            string result;
            if (LanguageDetails == null)
            {
                result = keyName;
            }
            else
            {
                result = GetLangFieldValue(LanguageCode, LanguageDetails);
            }

            return result;
        }

        /// <summary>
        /// 仅获取多语言数据资源
        /// </summary>
        /// <param name="zhName">简体</param>
        /// <param name="keyName">键名</param>
        /// <param name="keyType">键名类型</param>
        /// <returns></returns>
        public static string GetKeyName(string zh_CN, string zh_HK, string en_US, string keyName, string keyType)
        {
            if (string.IsNullOrEmpty(keyName) || string.IsNullOrWhiteSpace(keyName))
            {
                return "keyName no exists";
            }
            Language LanguageDetails = new Language();
            // var getLanguages = db.Database.SqlQuery<Language>("select * from Language");
            var getLanguages = db.Language;
            LanguageDetails = getLanguages.Where(c => c.KeyName.Contains(keyName)).FirstOrDefault<Language>();
            string result;
            if (LanguageDetails == null)
            {
                //db.Database.ExecuteSqlCommand(string.Format("INSERT INTO dbo.Language(KeyName ,KeyType , zh_CN ,zh_HK,en_US)VALUES('{0}','{1}' ,'{2}','{3}','{4}')", keyName, keyType, zh_CN, zh_HK, en_US));
                Language lang1 = new Language
                {
                    KeyName = keyName,
                    KeyType = keyType,
                    Zh_CN= zh_CN,
                    Zh_HK = zh_HK,
                    En_US=en_US,
                    Remark=string.Empty,
                    IndustryIdArr=string.Empty,
                    MainComIdArr = string.Empty
                };

                db.Language.Add(lang1);
                bool res = db.SaveChanges() > 0;
                result = string.Format("{0}-(1) Result={2}", keyName, keyType, res);
            }
            else
            {
                result = GetLangFieldValue(LanguageCode, LanguageDetails);
            }

            return result;
        }
        /// <summary>
        /// 获取多语言数据资源
        /// </summary>
        /// <param name="zhName">简体</param>
        /// <param name="keyName">键名</param>
        /// <param name="keyType">键名类型</param>
        /// <returns></returns>
        public static string GetKeyName(string keyName)
        {
            Language LanguageDetails = new Language();
            // var getLanguages = db.Database.SqlQuery<Language>("select * from Language");
            var getLanguages = db.Language;
            LanguageDetails = getLanguages.Where(c => c.KeyName == keyName).FirstOrDefault<Language>();
            string result;
            if (LanguageDetails == null)
            {
                result = string.Format("{0}=(1)", keyName);
            }
            else
            {
                result = GetLangFieldValue(LanguageCode, LanguageDetails);
            }

            return result;
        }

        public static string GetStringReflectKeyName(string KeyName)
        {
            try
            {
                Lang tmp_Lang = new Lang();
                Type type = tmp_Lang.GetType();
                if (KeyName.Contains(" ") || Regex.IsMatch(KeyName, @"[\u4e00-\u9fa5]")) //包含汉字空格的情況下沒可能是屬性 2022-6-27
                    return KeyName;

                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(KeyName);
                if (propertyInfo == null)
                    return KeyName;

                string rtn_str = (string)propertyInfo.GetValue(tmp_Lang);
                if (string.IsNullOrEmpty(rtn_str))
                    return KeyName;

                return rtn_str;
            }
            catch
            {
                return KeyName;
            }
        }
        public static string GetLangFieldValue(string LanguageField, Language LangDetails)
        {
            LanguageField = GetLanguageAbbr(LanguageField);
            switch (LanguageField)
            {
                case "zh-CN":
                    return LangDetails.Zh_CN;
                case "zh-SG":
                    return LangDetails.Zh_CN;
                case "zh-HK":
                    return LangDetails.Zh_HK;
                case "zh-hant-HK":
                    return LangDetails.Zh_HK;
                case "cn":
                    return LangDetails.Zh_CN;
                case "zh":
                    return LangDetails.Zh_HK;
                case "en":
                    return LangDetails.En_US;
                default:
                    return LangDetails.En_US;
            }
        }
         
        public static string GetLanguageAbbr(string LanguageCode)
        {
            if (LanguageCode == "zh-CN" || LanguageCode == "zh-HK" || LanguageCode == "en-US")
            {
                return LanguageCode;
            }
            else
            {
                //etc : zh:华语, en:泛英 fr:法语区
                LanguageCode = LanguageCode.Substring(0, 2).ToLower();
                return LanguageCode;
            }
        }
        public static string StandardLanguageCode(string Language)
        {
            Language = Language.ToLower();
            string LanguageCode;

            
            switch (Language)
            {
                case "zh-hk":
                    LanguageCode = "zh-HK";
                    break;
                case "zh-cn":
                    LanguageCode = "zh-CN";
                    break;
                case "zh-hans-hk":
                    LanguageCode = "zh-HK";
                    break;
                case "zh-hants-hk":
                    LanguageCode = "zh-HK";
                    break;
                case "zh-tw":
                    LanguageCode = "zh-HK";
                    break;
                case "en-us":
                    LanguageCode = "en-US";
                    break;
                case "en-uk":
                    LanguageCode = "en-US";
                    break;
                case "zh-hant-ao":
                    LanguageCode = "zh-HK";
                    break;
                case "zh":
                    LanguageCode = "zh-CN";
                    break;
                case "cn":
                    LanguageCode = "zh-CN";
                    break;
                case "en":
                    LanguageCode = "en-US";
                    break;
                case "hk":
                    LanguageCode = "zh-HK";
                    break;
                default:
                    LanguageCode = "zh-CN";
                    break;
            }

            return LanguageCode;
        }

    }
}