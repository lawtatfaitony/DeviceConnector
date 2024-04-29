using LanguageResource;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VideoGuard.ApiModels.Account
{
    #region AccountApiModel 

    public class AccountModelInput : GlobalFieldSession
    {
        public AccountModelInput() : base()
        {
        }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [Newtonsoft.Json.JsonIgnore()]
        [DefaultValue("")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("remark")]
        [DefaultValue("")]
        public string Remark { get; set; }
    }

    #endregion AccountApiModel

    public class Login
    {
        [Required(ErrorMessageResourceName = "Login_UserName_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("username")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = "Login_Password_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
    public class PasswordModify
    {
        [Required(ErrorMessageResourceName = "Login_UserName_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("username")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = "Login_OrginalPassword_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("orginalPassword")]
        public string OrginalPassword { get; set; }

        [Required(ErrorMessageResourceName = "Login_Password_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "Login_ModifyComfirmed_Reqiured", ErrorMessageResourceType = typeof(ResourceLocalize))]
        [JsonProperty("ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
    public class LoginResultInfo
    {
        [JsonProperty("session")]
        public string Session;
    }
    public class LoginResult : GlobalCodeMsg
    {
        public LoginResultInfo Info;
    }
}

