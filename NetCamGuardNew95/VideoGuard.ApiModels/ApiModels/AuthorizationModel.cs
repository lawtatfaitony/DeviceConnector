using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace VideoGuard.ApiModels
{
    #region RequestToken

    public class RequestToken
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("tokenType")]
        public string TokenType { get; set; } = "Bearer";

        [JsonProperty("authProfile")]
        public AuthProfile AuthProfile { get; set; }
    }
     
    
    public class AuthProfile
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("apiSession")]
        public string ApiSession { get; set; }

        
        [JsonProperty("authTime")]
        public long AuthTime { get; set; }

        [JsonProperty("accessExpiration")]
        public long AccessExpiration { get; set; }

        [JsonProperty("tokenExpiredTimeOut")]
        public long TokenExpiredTimeOut { get; set; }
    }

    #endregion RequestToken

    #region  RefreshToken
    public class RefreshTokenRequest
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
    public class UserRefreshToken
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string UserName { get; set; }
        public bool Active => DateTime.Now <= Expires;
    }
    #endregion

}

