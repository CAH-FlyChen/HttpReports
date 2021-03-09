﻿using HttpReports.Core;
using HttpReports.Dashboard.Abstractions;
using HttpReports.Dashboard.Handles; 
using HttpReports.Dashboard.Routes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Reflection; 

namespace HttpReports.Dashboard.Services
{
    public class AuthService:IAuthService
    { 
        private readonly ILogger<AuthService> _logger;

        List<AuthInfo> Members = new List<AuthInfo>();

        object sync = new object();

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }  

        public string BuildToken()
        {
            lock (sync)
            {
                var token = Guid.NewGuid().ToString().MD5();

                Members.Add(new AuthInfo
                {

                    Token = token,
                    ExpireTime = DateTime.Now.AddDays(7)

                });

                return token;

            } 

        }

        public bool ValidToken(string token)
        { 
            lock (sync)
            { 
                var user = Members.Where(x => x.Token == token).FirstOrDefault();

                if (user == null) return false; 

                if (user.ExpireTime <= DateTime.Now)
                {
                    Members.Remove(user);
                    return false;
                }

                return true;

            } 
        }

        public bool ValidToken(HttpContext httpContext, IDashboardHandle handle, DashboardRoute route)
        {  
            if (httpContext == null) return false; 

            if (handle.GetType().GetMethod(route.Action).GetCustomAttribute<AllowAnonymousAttribute>() != null)
            {
                return true;
            }

            if (httpContext.Request.Path.HasValue)
            {
                if (!BasicConfig.CurrentControllers.IsEmpty())
                {
                    if (!BasicConfig.CurrentControllers.Split(',').Select(x => x.ToLowerInvariant()).Contains(route.Handle.ToLowerInvariant()))
                    {
                        return true;
                    }
                }
            } 

            StringValues token; 

            if (!httpContext.Request.Headers.TryGetValue(BasicConfig.AuthToken,out token))
            { 
                return false;
            }

            return ValidToken(token.ToString()); 

        }   

    }

    public class AuthInfo
    {
        public string Token { get; set; }

        public DateTime ExpireTime { get; set; }

    } 

}
