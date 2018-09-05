using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    /// <summary>
    /// IdentityServer配置
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 添加对OpenID Connect的支持
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(), //必须要添加，否则报无效的scope错误
                new IdentityResources.Profile()
            };
        }
        /// <summary>
        /// 定义系统中的API资源
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
            };
        }

        /// <summary>
        /// 客户端访问资源
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            // 客户端信息
            return new List<Client>
            {
             //自定义接口登录的客户端
             new Client
                {
                    //客户端ID名称
                    ClientId = "client1",
                    //客户端访问方式：密码验证
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    //用于认证的密码加密方式
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    //客户端有权访问的范围
                    AllowedScopes = { "api1",
                    IdentityServerConstants.StandardScopes.OpenId, //必须要添加，否则报403 forbidden错误
                    IdentityServerConstants.StandardScopes.Profile
                 }
                },
                //定义mvc客户端
                new Client
                {
                    //客户端ID名称
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    //访问类型
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    //关闭确认是否返回身份信息界面
                    RequireConsent=false,
                   // 登录成功后重定向地址
                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                   //注销成功后的重定向地址
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                     //用于认证的密码加密方式
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                     //客户端有权访问的范围
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1",//要访问的api名称
                    },
                }
            };
        }
    }
}
