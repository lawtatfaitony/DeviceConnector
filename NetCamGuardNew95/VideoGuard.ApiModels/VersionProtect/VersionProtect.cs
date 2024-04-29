using Common;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoGuard.Business.V
{
    /// <summary>
    /// AppAuthKey.Key 
    /// </summary>
    public class AppAuth
    {
        [JsonProperty("appAuthKey")]
        public string AppAuthKey { get; set; }
        [JsonProperty("verifyHmac")]
        public string VerifyHmac { get; set; }

        [JsonProperty("exAuth")]
        public string ExAuth { get; set; }

        [JsonProperty("funcList")]
        public string FuncList { get; set; }
    }
    /// <summary>
    /// 软件系统版本 与 说明 Software system version and description
    /// </summary>
    public class VersionProtect
    {
        VersionProtect()
        {
            Ver = "V2.2";  //2021-10-13
            _SoftWareName = "AI GUARD";
        }
        private static string _Ver { get; set; }
        public static string Ver
        {
            get
            {
                return _Ver;
            }
            set
            {
                _Ver = value;
            }
        }

        private static string _SoftWareName { get; set; }
        public static string SoftWareName
        {
            get
            {
                return _SoftWareName;
            }
            set
            {
                _SoftWareName = value;
            }
        }

        public static string GetSoftWareName(string LanguageCode)
        {
            LanguageCode = LanguageCode.ToLower();

            if (LanguageCode == "zh-CN")
            {
                return "AI GUARD";  //temp anme
            }

            if (LanguageCode == "zh-hk")
            {
                return "AI GUARD";  //temp anme
            }

            if (LanguageCode == "en-us")
            {
                return "AI GUARD";  //temp anme
            }

            return "AI GUARD";  //temp anme
        }
        public static string GetSoftWareUrl(string LanguageCode)
        {
            LanguageCode = LanguageCode.ToLower();

            if (LanguageCode == "zh-cn")
            {
                return "http://81.71.74.135:5002/zh-CN";  //temp anme
            }

            if (LanguageCode == "zh-hk")
            {
                return "http://81.71.74.135:5002/zh-HK";  //temp anme
            }

            if (LanguageCode == "en-us")
            {
                return "http://81.71.74.135:5002/en-US";  //temp anme
            }

            return "http://81.71.74.135:5002/";  //temp anme
        }
         
    } 
    /// <summary>
    /// 新版2022年8月13日 : SALFIN:1;ATT:1;ATTDEV:50;ATTEPLY:300;AIGEPLY:301;AIGDEV:50;DVR:6;CAM:3;MAIN:3;CON:6
    /// </summary>
    public class FUNCTION_LIST_V2
    {
        /// <summary>
        /// 外部传入的明文 function_list = SALFIN:1;ATT:1;ATTDEV:50;ATTEPLY:300;AIGEPLY:301;AIGDEV:50;DVR:6;CAM:3;MAIN:3;CON:6
        /// Plaintext from external: function_list
        /// </summary>
        /// <param name="function_list"></param>
        public FUNCTION_LIST_V2(string functionListStr)
        {
            try
            { 
                List<string> functionList = new List<string>();
                functionList = functionListStr.Split(";").ToList();

                SalFin = GetFieldValue(functionList, "SALFIN");
                Att = GetFieldValue(functionList, "ATT");
                AttDev = GetFieldValue(functionList, "ATTDEV");
                AttEply = GetFieldValue(functionList, "ATTEPLY");
                AigEply = GetFieldValue(functionList, "AIGEPLY");
                AigDev = GetFieldValue(functionList, "AIGDEV");
                Dvr = GetFieldValue(functionList, "DVR");
                Cam = GetFieldValue(functionList, "CAM");
                Main = GetFieldValue(functionList, "MAIN");
                Con = GetFieldValue(functionList, "CON");
            }
            catch
            {
                SalFin = 1;
                Att = 1;
                AttDev = 1;
                AttEply = 1;
                AigEply = 1;
                AigDev = 1;
                Dvr = 1;
                Cam = 1;
                Main = 1;
                Con = 1;
            }
        }
        public int SalFin { get; set; }
        public int Att { get; set; }
        public int AttDev { get; set; }
        public int AttEply { get; set; }
        public int AigEply { get; set; }
        public int AigDev { get; set; }
        public int Dvr { get; set; }
        public int Cam { get; set; }
        public int Main { get; set; }
        public int Con { get; set; }

        public static string GetSignDataOfFunctionList()
        {
            string AuthoriztionFileName = "AppAuth.key";
            string rootPath = Path.GetFullPath("./");
            string AppLicensePathFile = Path.Combine(rootPath, AuthoriztionFileName);

            bool bAppLicensePathFile = System.IO.File.Exists(AppLicensePathFile);
            

            if (bAppLicensePathFile == false)
            {
                string cDrive = System.Environment.GetEnvironmentVariable("SystemDrive");
                AppLicensePathFile = Path.Combine(cDrive, AuthoriztionFileName);
                bAppLicensePathFile = System.IO.File.Exists(AppLicensePathFile);
                if (bAppLicensePathFile == false)
                {
                    return string.Empty;
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC:GetSignDataOfFunctionList][{AppLicensePathFile}]");
                }
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}][FUNC:GetSignDataOfFunctionList][{AppLicensePathFile}]");
            }

            string SignData = System.IO.File.ReadAllText(AppLicensePathFile, System.Text.Encoding.UTF8);
            SignData = Common.CommonBase.Base64ToString(SignData);
            //------------------------------------------------------------------------------- 
            AppAuth appAuth = new AppAuth();
            appAuth = JsonConvert.DeserializeObject<AppAuth>(SignData);
            Console.WriteLine("------------------------------------------------------------------------------- ");
            Console.WriteLine("[FUNCTION_LIST][({0}...)]\n", appAuth.FuncList.Substring(0, 117)); //RSA最大存儲字符串
            Console.WriteLine(appAuth.FuncList);
            Console.WriteLine("------------------------------------------------------------------------------- ");
            return appAuth.FuncList;
        }
        public int GetFieldValue(List<string> list,string fieldName)
        {
            string keyPairStr = list.Where(c => c.Contains(fieldName)).FirstOrDefault();
            if (string.IsNullOrEmpty(keyPairStr))
                return 0;

            string[] keyPairArr = keyPairStr.Split(":");
            if(keyPairArr.Length<=1)
                return 0;

           if(int.TryParse(keyPairArr[1], out int val))
           {
                return val;
           }else
            {
                return 0;
            }
        }
    }
}
