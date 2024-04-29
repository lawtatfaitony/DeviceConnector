using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VxGuardClient
{
    public interface IUserService
    {
        bool IsValid(LoginRequestDTO req);

        bool Login(LoginRequestDTO req);
    }
     
}
