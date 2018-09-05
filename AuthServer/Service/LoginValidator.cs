using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthServer.Service
{
    /// <summary>
    /// 用户登录校验
    /// </summary>
    public class LoginValidator: IResourceOwnerPasswordValidator
    {
        public LoginValidator()
        {

        }
        /// <summary>
        /// 登录用户校验
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //根据context.UserName和context.Password与数据库的数据做校验，判断是否合法
            if (context.UserName == "test" && context.Password == "123")
            {
                context.Result = new GrantValidationResult(
                 subject: context.UserName,
                 authenticationMethod: "custom",
                 claims: GetUserClaims());
            }
            else
            {

                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "账号密码错误");
            }
        }
        /// <summary>
        /// 可以根据需要设置相应的Claim
        /// </summary>
        /// <returns></returns>
        private Claim[] GetUserClaims()
        {
            return new Claim[]
            {
                new Claim("UserId", 1.ToString()),
                new Claim(JwtClaimTypes.Name,"test"),
                new Claim(JwtClaimTypes.GivenName, "jaycewu"),
                new Claim(JwtClaimTypes.FamilyName, "yyy"),
                new Claim(JwtClaimTypes.Email, "test@qq.com"),
                new Claim(JwtClaimTypes.Role,"admin")
            };
        }
    }
}