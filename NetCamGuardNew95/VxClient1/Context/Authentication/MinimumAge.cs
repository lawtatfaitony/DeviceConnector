using LanguageResource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks; 
using VxGuardClient.Context;

namespace VxGuardClient.Context
{ 
    internal class MinimumAgeAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "MinimumAge";
        public MinimumAgeAuthorizeAttribute(int age) => Age = age;
        // Get or set the Age property by manipulating the underlying Policy property
        public int Age
        {
            get
            {
                if (int.TryParse(Policy.Substring(POLICY_PREFIX.Length), out var age))
                {
                    return age;
                }
                return default(int);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }
    }
}