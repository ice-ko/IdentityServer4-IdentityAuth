using Components.Base.Models;
using Components.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Components.Base
{
    /// <summary>
    /// 通用登录/菜单信息
    /// </summary>
    public class Authentication
    {
        private readonly HttpContext _httpContext;

        public Authentication(HttpContext httpContext)
        {
            _httpContext = httpContext;

        }

        /// <summary>
        /// 用户key
        /// </summary>
        public const string UserSessionKey = "UserInfo";

        /// <summary>
        /// 写入验证信息
        /// </summary>
        /// <param name="uInfo"></param>
        /// <param name="isPersistent">是否保存</param>
        public void SetAuth(BaseUserResult uInfo, bool isPersistent)
        {
            //当前登录的用户信息
            var claims = new List<Claim>
            {
                new Claim("UserId", uInfo.Id.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //可以使用HttpContext.SignInAsync方法的重载来定义持久化cookie存储用户认证信息，例如下面的代码就定义了用户登录后60分钟内cookie都会保留在客户端计算机硬盘上，
            //即便用户关闭了浏览器，60分钟内再次访问站点仍然是处于登录状态，除非调用Logout方法注销登录。
            //_httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            _httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity),
               new AuthenticationProperties()
               {
                   IsPersistent = isPersistent,
                   ExpiresUtc = isPersistent ? DateTimeOffset.Now.AddDays(7) : DateTime.Now.AddHours(6)
               });
            SetSession(uInfo);
        }

        /// <summary>
        ///保存用户状态
        /// </summary>
        /// <param name="uInfo"></param>
        public void SetSession(BaseUserResult uInfo)
        {
            bool setFlag = false;
            if (_httpContext.Session == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(_httpContext.Session.GetString(UserSessionKey)))
            {
                setFlag = true;
            }
            else
            {
                var user = JsonConvert.DeserializeObject<BaseUserResult>(_httpContext.Session.GetString(UserSessionKey));
                if (user != null && user.Id != uInfo.Id)
                {
                    setFlag = true;
                };
            }

            if (setFlag)
            {
                try
                {
                    _httpContext.Session.SetString(UserSessionKey, JsonConvert.SerializeObject(uInfo));
                }
                catch
                {
                    // ignored
                }
            }
        }
        /// <summary>
        /// 当前用户信息
        /// </summary>
        public BaseUserResult CurrentUser
        {
            get
            {
                if (!_httpContext.User.Identity.IsAuthenticated)
                {
                    return null;
                }
                var claimIdentity = (ClaimsIdentity)_httpContext.User.Identity;
                var claimsPrincipal = claimIdentity.Claims as List<Claim>;
                if (!string.IsNullOrEmpty(claimsPrincipal[0].Value))
                {
                    if (string.IsNullOrEmpty(_httpContext.Session.GetString(UserSessionKey)))
                    {
                        if (CacheHelper.GetInstance._memoryCache.TryGetValue("userInfo", out BaseUserResult baseUserResult))
                        {
                            return baseUserResult;
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 登出
        /// </summary>
        public void SignOut()
        {
            //注销登录的用户，相当于ASP.NET中的FormsAuthentication.SignOut 
            _httpContext.Session.Clear();
            _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        /// <summary>
        /// 用户标识
        /// </summary>
        public int ActiveUser
        {
            get
            {
                if (_httpContext.User.Identity.IsAuthenticated)
                {
                    var claimIdentity = (ClaimsIdentity)_httpContext.User.Identity;
                    var claimsPrincipal = claimIdentity.Claims as List<Claim>;
                    return int.Parse(claimsPrincipal[0].Value);
                }
                return 0;
            }
        }
        /// <summary>
        /// 用户token
        /// </summary>
        //public string ApiToken
        //{
        //    get
        //    {
        //        if (_httpContext.User.Identity.IsAuthenticated)
        //        {
        //            var claimIdentity = (ClaimsIdentity)_httpContext.User.Identity;
        //            var claimsPrincipal = claimIdentity.Claims as List<Claim>;
        //            return claimsPrincipal[0].Value ?? System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"];
        //        }
        //        return System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"];
        //    }
        //}
    }
}
