using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VxGuardClient
{
    public class LoginRequestDTO
    {
        [Required]
        [JsonProperty("username")]
        public string UserName { get; set; }


        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
