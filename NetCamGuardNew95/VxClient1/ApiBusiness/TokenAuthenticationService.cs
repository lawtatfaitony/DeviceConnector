using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataBaseBusiness.Models;
using Common; 
using log4net;
using System.Security.Cryptography;
using VideoGuard.ApiModels;

namespace VxGuardClient
{
    public class TokenAuthenticationService : IAuthenticateService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TokenAuthenticationService));
        public ILog logger
        {
            get
            {
                return _logger;
            }
        }
         
        private readonly IUserService _userService;
        private readonly TokenManagement _tokenManagement;
        private readonly BusinessContext businessContext;
        public TokenAuthenticationService(IUserService userService, IOptions<TokenManagement> tokenManagement, BusinessContext dataBaseContext)
        {
            _userService = userService;
            _tokenManagement = tokenManagement.Value;
            businessContext = dataBaseContext;
        }

        public string GetAuthentUser()
        {
            NameValueCollection userlist = new NameValueCollection
                {
                    { "username", "AppleChan" },
                    { "username2", "AppleChan2" }
                };
            return JsonConvert.SerializeObject(userlist);
        }

        public bool IsAuthenticated(LoginRequestDTO request, out string token)
        {
            token = string.Empty;
            bool IsValidAccess = _userService.IsValid(request);  //------------------

            if (!IsValidAccess)
            {
                return false;
            }
            else
            {
                bool createTokenResult = TryCreateAccessToken(request.UserName, out token);
                return createTokenResult;
            }
        }
        public bool TryCreateAccessToken(string userName,out string token)
        {
            try
            {
                Claim[] claims = new[] { new Claim(ClaimTypes.Name, userName) };
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
                SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                JwtSecurityToken jwtToken = new JwtSecurityToken(_tokenManagement.Issuer, _tokenManagement.Audience, claims, expires: DateTime.Now.AddMinutes(_tokenManagement.AccessExpiration), signingCredentials: credentials);
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return true;

            }catch(Exception e)
            {
                this.logger.Info("[TryCreateAccessToken] {0}",e);
                Console.WriteLine("[TryCreateAccessToken] {0}", e.Message);
                token = string.Empty;
                return false;
            }  
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenManagement.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidIssuer = _tokenManagement.Issuer,
                ValidAudience = _tokenManagement.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.Info(string.Format("[GetPrincipalFromExpiredToken] [INVALID TOKEN] [TOKEN: {0}]", token));
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        /// <summary>
        /// 从Token中获取用户身份
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal GetPrincipalFromAccessToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret)),
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 生成刷新Token
        /// </summary>
        /// <returns></returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public bool LoginIsvalid(string UserName, string Password)
        {
            using (BusinessContext businessContext = new BusinessContext())
            {
                bool f = businessContext.Database.CanConnect();
                var ftUser = businessContext.FtUser.Where(c => c.Name == UserName && c.Password == Password).FirstOrDefault();
                if (ftUser != null)
                { 
                    return true ;
                }
                else
                { 
                    return false;
                }
            }
        }

        public string GetApiSession(string password)
        { 
            return CommonBase.MD5Encrypt(password);
        }
        public string GetPasswordMD5(string password)
        { 
            return CommonBase.MD5Encrypt(password);
        }
    }
}
