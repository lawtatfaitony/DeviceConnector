using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VxGuardClient
{
    public interface IAuthenticateService
    {
        bool IsAuthenticated(LoginRequestDTO request, out string token);

        string GetApiSession(string Password);

        string GetAuthentUser();

        bool TryCreateAccessToken(string userName,out string token);

        public ClaimsPrincipal GetPrincipalFromAccessToken(string token);
        public string GenerateRefreshToken();

        public string GetPasswordMD5(string password);
    }
}
