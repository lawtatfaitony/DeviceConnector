using Common;
using DataBaseBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VxGuardClient
{
    public class UserService : IUserService
    {
        public bool IsValid(LoginRequestDTO req)
        {
            bool loginResult = Login(req);
            return loginResult;
        }
        public bool Login(LoginRequestDTO loginRequestDTO)
        { 
            using (BusinessContext businessContext = new BusinessContext())
            {
                if (loginRequestDTO.Password.Length != 32 && !CommonBase.IsMd5LowerCase(loginRequestDTO.Password)) //For App 基於 DGX 系統 明文密碼登錄的情況
                {
                    loginRequestDTO.Password = CommonBase.MD5Encrypt(loginRequestDTO.Password);
                }
                var ftUser = businessContext.FtUser.Where(c => c.Name == loginRequestDTO.UserName && c.Password == loginRequestDTO.Password).FirstOrDefault();
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
    }
}
