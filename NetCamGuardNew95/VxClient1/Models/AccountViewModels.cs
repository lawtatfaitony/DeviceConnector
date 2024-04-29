using LanguageResource;
using System;
using System.ComponentModel.DataAnnotations;
namespace VxGuardClient.ModelView
{
    public class LoginViewModel
    {
        [Required]
        [Display(Description = "User_UserName", ResourceType = typeof(ResourceLocalize))]
        [RegularExpression(@"^\w{2,10}$", ErrorMessageResourceName = "User_UserName_Format", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Description = "User_Password", ResourceType = typeof(ResourceLocalize))]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class LoginSuccessCookieModel
    { 
        public string UserName { get; set; } 
        public string ApiSession { get; set; }
        public long Expires { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Description = "User_UserName", ResourceType = typeof(ResourceLocalize))]
        [RegularExpression(@"^\w{2,10}$", ErrorMessageResourceName = "User_UserName_Format", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string UserName { get; set; }

        [Required]
        [StringLength(30, ErrorMessageResourceName = "User_Password_Format_TooShort", ErrorMessageResourceType = typeof(ResourceLocalize), MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Description = "User_Password", ResourceType = typeof(ResourceLocalize))]
        public string Password { get; set; }
        [DataType(DataType.Password)]

        [Display(Description = "User_Password_Comfirmed", ResourceType = typeof(ResourceLocalize))]
        [Compare("Password", ErrorMessageResourceName = "User_ComfirmedPassword", ErrorMessageResourceType = typeof(ResourceLocalize))]
        public string ConfirmPassword { get; set; }
    }
}