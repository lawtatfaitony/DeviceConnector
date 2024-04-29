using System.ComponentModel;

namespace VideoGuard.ApiModels.SystemConfig
{
    #region GetSystemConfig
    public class GetSystemConfigInput : GlobalFieldSession
    {
        public GetSystemConfigInput() : base()
        {
        }
        [DefaultValue("StorageServer")]
        public string name { get; set; }
    }
    public class Config
    {
        public string Ip { get; set; }

        public int Port { get; set; }
    }
    public class Info
    {
        public string Name { get; set; }
        public Config Config { get; set; }
    }
    public class InfoStr
    {
        public string Name { get; set; }
        public string Config { get; set; }
    }
    public class GetSystemConfigReturn
    {
        public Info Info { get; set; }
        public int Code { get; set; }
        public string Msg { get; set; }
    }
    public class GetSystemConfigResult
    {
        public InfoStr Info { get; set; }
        public int Code { get; set; }
        public string Msg { get; set; }
    }
    #endregion GetSystemConfig

    #region UpdSystemConfig
    public class UpdSystemConfigModel : GlobalFieldSession
    {
        public UpdSystemConfigModel() : base()
        {
        }
        public string Name { get; set; }
        public string Config { get; set; }
        public string Remark { get; set; }
    }
    public class UpdSystemConfigResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
    }
    public class UpdConfig : Config
    {
        public string Remark { get; set; }
    }
    #endregion  UpdSystemConfig
}

