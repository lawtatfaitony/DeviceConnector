using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FunctionProtected
{
    public enum SOFTWARE_SYSTEM
    {
        AI_GUARD = 0,

        DATAGUARD_XCORE = 1
    }
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

        public static string GetSoftWareName(string LanguageCode, SOFTWARE_SYSTEM sYSTEM)
        {
            LanguageCode = LanguageCode.ToLower();

            if (sYSTEM == SOFTWARE_SYSTEM.AI_GUARD)
            {
                if (LanguageCode == "zh-cn")
                {
                    return "AI GUARD";
                }
                if (LanguageCode == "zh-hk")
                {
                    return "AI GUARD";
                }
                if (LanguageCode == "en-us")
                {
                    return "AI GUARD";
                }
                return "AI GUARD";
            }
            else
            {
                if (LanguageCode == "zh-cn")
                {
                    return "DATA GUARD X CORE";
                }
                if (LanguageCode == "zh-hk")
                {
                    return "DATA GUARD X CORE";
                }
                if (LanguageCode == "en-us")
                {
                    return "DATA GUARD X CORE";
                }
                return "DATA GUARD X CORE";
            }

        }

        /// <summary>
        /// LIKE : http://81.71.74.135:5002/zh-CN
        /// </summary>
        /// <param name="LanguageCode"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 分號分割KayPair
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
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
                    tStr += string.Format("{0}:{1};", name, value);
                }
                else
                {
                    GetProperties(value);
                }
            }
            return tStr;
        }

        public static string ReadDataFromJson(string PathFileName)
        {
            if (File.Exists(PathFileName))
            {
                string content = File.ReadAllText(PathFileName, Encoding.UTF8);
                return content;
            }
            else
            {
                return string.Empty;
            }
        }
        public static bool SaveDataJson(string FileContent, string PathFile)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(PathFile, false, System.Text.Encoding.GetEncoding("UTF-8"));
                writer.Write(FileContent);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return true;
        }
    }

    /// <summary>
    //功能名稱	"編碼格式 1字母+1位數字" 
    //DGX薪酬財務系統開啟      S1
    //DGX考勤設備連接數        A1
    //AIG系統設備連接數        A2
    //DGX考勤系統人數          A3
    //AIG智能設備系統人數      A4
    //AIG 網絡錄像服務器       D1
    //AIG鏡頭接入數量          C1
    //總公司數量               M1
    //子公司數量               C2
    //海康多設備               H1
    //TEXT : DGX薪酬財務系統開啟S1;DGX考勤設備連接數 A1;AIG系統設備連接數 A2;DGX考勤系統人數 A3;AIG智能設備系統人數A4;AIG 網絡錄像服務器D1;AIG鏡頭接入數量C1;總公司數量M1;子公司數量C2;海康多設備H1;擴展E1;擴展E2;擴展E3;擴展E4;擴展E5;擴展E6;擴展E7;擴展E8
    //TEXT : S1;A1;A2;A3;A4;D1;C1;M1;C2;H1;E1;E2;E3;E4;E5;E6;E7;E8
    /// </summary>
    public class FunctionList
    {
        /// <summary>
        /// 外部传入的明文 function_list = SALFIN:1;ATT:1;ATTDEV:50;ATTEPLY:300;AIGEPLY:301;AIGDEV:50;DVR:6;CAM:3;MAIN:3;CON:6
        /// Plaintext from external: function_list
        /// </summary>
        /// <param name="function_list"></param>
        public static FunctionList Get(string functionListStr)
        {
            FunctionList f = new FunctionList();
            try
            {
                List<string> functionList = new List<string>();
                functionList = functionListStr.Split(";").ToList();

                f.S1 = GetFieldValue(functionList, "S1");
                f.A1 = GetFieldValue(functionList, "A1");
                f.A2 = GetFieldValue(functionList, "A2");
                f.A3 = GetFieldValue(functionList, "A3");
                f.A4 = GetFieldValue(functionList, "A4");
                f.D1 = GetFieldValue(functionList, "D1");
                f.C1 = GetFieldValue(functionList, "C1");
                f.M1 = GetFieldValue(functionList, "M1");
                f.C2 = GetFieldValue(functionList, "C2");
                f.H1 = GetFieldValue(functionList, "H1");
                f.E1 = GetFieldValue(functionList, "E1");
                f.E2 = GetFieldValue(functionList, "E2");
                f.E3 = GetFieldValue(functionList, "E3");
                f.E4 = GetFieldValue(functionList, "E4");
                f.E5 = GetFieldValue(functionList, "E5");
                f.E6 = GetFieldValue(functionList, "E6");
                f.E7 = GetFieldValue(functionList, "E7");
                f.E8 = GetFieldValue(functionList, "E8");
            }
            catch
            {
                f.S1 = 0;
                f.A1 = 1;
                f.A2 = 1;
                f.A3 = 1;
                f.A4 = 1;
                f.D1 = 1;
                f.C1 = 1;
                f.M1 = 1;
                f.C2 = 1;
                f.H1 = 1;
                f.E1 = 0;
                f.E2 = 0;
                f.E3 = 0;
                f.E4 = 0;
                f.E5 = 0;
                f.E6 = 0;
                f.E7 = 0;
                f.E8 = 0;
            }
            return f;
        }

        public static bool Save(string mainComId, FunctionList func)
        {
            if (string.IsNullOrEmpty(mainComId) || func == null)
                return false;

            string FunctionListFolder = Path.GetFullPath("./FunctionList");
            if (!Directory.Exists(FunctionListFolder))
                Directory.CreateDirectory(FunctionListFolder);

            var setting = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            };

            string fileName = $"FunctionList{mainComId}.json";
            string pathFilename = Path.Combine(FunctionListFolder, fileName);
            string functionListJson = JsonConvert.SerializeObject(func, setting);

            return VersionProtect.SaveDataJson(functionListJson, pathFilename);
        }

        /// <summary>
        /// 絕對返回 功能列表 從已經設定的JSON 功能列表 或從軟件系統中獲取
        /// 由于不引用Encryption。dll 所以funclistPlainText是系统证书 使用外部引用Encryption来获取明文 
        /// </summary>
        /// <param name="mainComId"></param>
        /// <returns></returns>
        public static FunctionList FromSoftwareLicenseOrJsonCfg(string mainComId, string funclistPlainText)
        {
            FunctionList functionList = Get(funclistPlainText);

            functionList = FromJson(mainComId);
            if (functionList != null) //JSON存在则用JSON的
            {
                return functionList;
            }
            return functionList;
        }
        /// <summary>
        /// 用Get函數判斷M1總公司數量是否大於等於3即屬於平台性質,
        /// 則從FunctionList6000014.json獲取,配置文件有後台人員生成配置並保存到平台的FunctionList目錄下
        /// </summary>
        /// <param name="mainComId"></param>
        /// <returns></returns>
        public static FunctionList FromJson(string mainComId)
        {
            if (string.IsNullOrEmpty(mainComId))
                return null;

            string FunctionListFolder = Path.GetFullPath("./FunctionList");
            if (!Directory.Exists(FunctionListFolder))
                Directory.CreateDirectory(FunctionListFolder);

            string fileName = $"FunctionList{mainComId}.json";
            string pathFilename = Path.Combine(FunctionListFolder, fileName);
            string functionListJson = VersionProtect.ReadDataFromJson(pathFilename);

            FunctionList functionList = JsonConvert.DeserializeObject<FunctionList>(functionListJson);
            return functionList;
        }

        /// <summary>
        /// GET PLAIN TEXT FUNCTION LIST
        /// </summary>
        /// <returns></returns>
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
            SignData = Base64ToString(SignData);
            //------------------------------------------------------------------------------- 
            AppAuth appAuth = new AppAuth();
            appAuth = JsonConvert.DeserializeObject<AppAuth>(SignData);
            Console.WriteLine("------------------------------------------------------------------------------- ");
            Console.WriteLine("[FUNCTION_LIST][({0}...)]\n", appAuth.FuncList.Substring(0, 117)); //RSA最大存儲字符串
            Console.WriteLine(appAuth.FuncList);
            Console.WriteLine("------------------------------------------------------------------------------- ");
            return appAuth.FuncList;
        }
        public static int GetFieldValue(List<string> list, string fieldName)
        {
            string keyPairStr = list.Where(c => c.Contains(fieldName)).FirstOrDefault();
            if (string.IsNullOrEmpty(keyPairStr))
                return 0;

            string[] keyPairArr = keyPairStr.Split(":");
            if (keyPairArr.Length <= 1)
                return 0;

            if (int.TryParse(keyPairArr[1], out int val))
            {
                return val;
            }
            else
            {
                return 0;
            }
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
        /// 薪酬財務系統開啟
        /// </summary>
        [Description("DGX薪酬財務系統開啟")]
        public int S1 { get; set; }
        /// <summary>
        /// 考勤設備連接數
        /// </summary>
        [Description("DGX考勤設備連接數")]
        public int A1 { get; set; }
        /// <summary>
        /// 系統設備連接數
        /// </summary>
        [Description("AIG系統設備連接數")]
        public int A2 { get; set; }
        /// <summary>
        /// 考勤系統人數
        /// </summary>
        [Description("DGX考勤系統人數")]
        public int A3 { get; set; }
        /// <summary>
        /// 智能設備系統人數
        /// </summary>
        [Description("AIG智能設備系統人數")]
        public int A4 { get; set; }
        /// <summary>
        /// AIG 網絡錄像服務器
        /// </summary>
        [Description("AIG 網絡錄像服務器")]
        public int D1 { get; set; }
        /// <summary>
        /// 鏡頭接入數量
        /// </summary>
        [Description("AIG鏡頭接入數量")]
        public int C1 { get; set; }
        /// <summary>
        /// 總公司數量 | 大於等於3切換到本地json獲取平台客戶的功能配置
        /// </summary>
        [Description("AIG/DGX 總公司數量")]
        public int M1 { get; set; }
        /// <summary>
        /// 子公司數量
        /// </summary>
        [Description("子公司數量")]
        public int C2 { get; set; }
        /// <summary>
        /// 海康多設備 
        /// </summary>
        [Description("AIG/DGX海康多設備")]
        public int H1 { get; set; }

        /// <summary>
        /// EX預留擴展功能1
        /// </summary>
        [Description("EX預留擴展功能1")]
        public int E1 { get; set; }

        /// <summary>
        /// EX預留擴展功能2
        /// </summary>
        [Description("EX預留擴展功能2")]
        public int E2 { get; set; }

        /// <summary>
        /// EX預留擴展功能3
        /// </summary>
        [Description("EX預留擴展功能3")]
        public int E3 { get; set; }

        /// <summary>
        /// EX預留擴展功能4
        /// </summary>
        [Description("EX預留擴展功能4")]
        public int E4 { get; set; }

        /// <summary>
        /// EX預留擴展功能5
        /// </summary>
        [Description("EX預留擴展功能5")]
        public int E5 { get; set; }

        /// <summary>
        /// EX預留擴展功能6
        /// </summary>
        [Description("EX預留擴展功能6")]
        public int E6 { get; set; }

        /// <summary>
        /// EX預留擴展功能7
        /// </summary>
        [Description("EX預留擴展功能7")]
        public int E7 { get; set; }

        /// <summary>
        /// EX預留擴展功能8
        /// </summary>
        [Description("EX預留擴展功能8")]
        public int E8 { get; set; }
    }

    public static class DescExtend
    {
        /// <summary>
        /// error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDesc<T>(this T value)
        {
            //var type = typeof(T);
            //var memberInfo = type.GetMember(value.ToString()).FirstOrDefault();
            //var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            //if (descriptionAttribute == null)
            //    return string.Empty;
            //return descriptionAttribute.Description;

            return "-";
        }
    }
}
