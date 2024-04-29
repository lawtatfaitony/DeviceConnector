using Common;
using EnumCode;
using LanguageResource;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using VideoGuard.ApiModels;
using VideoGuard.ApiModels.Account; 

namespace VxGuardClient.Context
{
    /// <summary>
    /// 常量配置
    /// </summary>
    public class GlobalConfig
    {
        public GlobalConfig(string ParamAppId = "0")
        {
            string jsonFileName = string.Format("GlobalConfig_{0}.Json", ParamAppId);
            string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

            if (ParamAppId == "0")
            {
                jsonFileName = "GlobalConfig.Json";
            }

            string pathFileName = Path.Combine(baseDirectoryPath, jsonFileName);
            string jsonContent;

            if (MemoryCacheHelper.Contains(jsonFileName) == false)
            {
                if (!File.Exists(pathFileName))
                {
                    jsonFileName = "GlobalConfig.Json";
                    pathFileName = Path.Combine(baseDirectoryPath, jsonFileName);
                }
                jsonContent = CommonBase.ReadConfigJson(pathFileName);
                jsonContent = CommonBase.JsonFormat(jsonContent);
            }
            else
            {
                jsonContent = MemoryCacheHelper.GetCacheItem<string>(jsonFileName);
            }

            Dictionary<string, string> iniGlobalConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            _DataFolder = string.IsNullOrEmpty(iniGlobalConfig["dataFolder"]) ? "Data" : iniGlobalConfig["dataFolder"];
            _WebRootFolder = iniGlobalConfig["webRootFolder"];

            if (!string.IsNullOrEmpty(iniGlobalConfig["appId"]))
                _AppId = "admin";
            _Secret = string.IsNullOrEmpty(iniGlobalConfig["secret"]) ? "admin123" : iniGlobalConfig["secret"];

            #region 获取API SESSION 
            string APIConfigFileName = "APIConfig.json";
            string APIConfigPathFileName = Path.Combine(baseDirectoryPath, APIConfigFileName);
            string APIConfigJsonContent;

            if (MemoryCacheHelper.Contains(APIConfigFileName) == false)
            {
                APIConfigJsonContent = CommonBase.ReadConfigJson(APIConfigPathFileName);
                APIConfigJsonContent = CommonBase.JsonFormat(APIConfigJsonContent);
            }
            else
            {
                APIConfigJsonContent = MemoryCacheHelper.GetCacheItem<string>(APIConfigFileName);
            }

            APIConfig apiConfig = JsonConvert.DeserializeObject<APIConfig>(APIConfigJsonContent);
            _APIConfig = apiConfig;
            #endregion 获取API SESSION
            _Session = iniGlobalConfig["Session"];
        }
        private string _WebRootFolder;
        public string WebRootFolder
        {
            get
            {
                return _WebRootFolder;
            }
            set
            {
                _WebRootFolder = value;
            }
        }

        private string _DataFolder;
        public string DataFolder
        {
            get
            {
                return _DataFolder;
            }
            set
            {
                _DataFolder = value;
            }
        }
        private string _AppId;
        public string AppId
        {
            get
            {
                return _AppId;
            }
            set
            {
                _AppId = value;
            }
        }

        private string _Secret;
        public string Secret
        {
            get
            {
                return _Secret;
            }
            set
            {
                _Secret = value;
            }
        }
        private string _Session;
        public string Session
        {
            get
            {
                return _Session;
            }
            set
            {
                _Session = value;
            }
        }

        private DateTime _AuthExpires;
        public DateTime AuthExpires
        {
            get
            {
                return _AuthExpires;
            }
            set
            {
                _AuthExpires = value;
            }
        }
        private APIConfig _APIConfig;
        public APIConfig APIConfig
        {
            get
            {
                return _APIConfig;
            }
            set
            {
                _APIConfig = value;
            }
        } 
    }
    public class APIConfig
    {
        [JsonProperty("http_server")]
        public HttpApiIpAddress http_server;
        [JsonProperty("api_login")]
        public ApiLogin api_login;
        [JsonProperty("storage_server")]
        public PortServerIpAddress storage_server;
        [JsonProperty("capture_server")]
        public PortServerIpAddress capture_server;
        [JsonProperty("compare_server")]
        public PortServerIpAddress compare_server;
        [JsonProperty("upload_server")]
        public PortServerIpAddress upload_server;
    }

    public class HttpApiIpAddress
    {
        [JsonProperty("host")]
        public string Host;
        [JsonProperty("port")]
        public string Port;
        [JsonProperty("root")]
        public string Root;
    }
    public class PortServerIpAddress
    {
        [JsonProperty("ip")]
        public string Ip;
        [JsonProperty("port")]
        public string Port;
    }
    public class LoginResult : GlobalCodeMsg
    {
        public LoginResultInfo Info;
    }
    public class LoginResultInfo
    {
        [JsonProperty("session")]
        public string Session;
    }
    public class ApiLogin
    {
        [JsonProperty("user_name")]
        public string username;
        [JsonProperty("password")]
        public string password;
    }

}
