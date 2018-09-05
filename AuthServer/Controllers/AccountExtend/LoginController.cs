using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Weibo;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Quickstart.UI;
using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AuthServer.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>
        /// 测试用户信息
        /// </summary>
        private readonly TestUserStore _users;
        /// <summary>
        /// 提供用户界面使用的服务与IdentityServer进行通信。
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;
        /// <summary>
        /// 事件服务的接口
        /// </summary>
        private readonly IEventService _events;
        public LoginController(IIdentityServerInteractionService interaction, IEventService events, TestUserStore users = null)
        {
            //如果TestUserStore不在DI中，那么我们将只使用全局用户集合
            //这是您插入自己的自定义身份管理库的地方（例如ASP.NET身份）
            _users = users ?? new TestUserStore(TestUsers.Users);
            _interaction = interaction;
            _events = events;
        }
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 启动到外部身份验证提供程序的往返
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and scheme 
                var props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(Callback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // 从临时cookie读取外部标识
            //var result = await HttpContext.AuthenticateAsync("QQ");
            var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // 检索外部用户的索赔
            var externalUser = result.Principal;
            var claims = externalUser.Claims.ToList();
            // 尝试确定外部用户的唯一ID（由提供商发布）
            // 最普通类型的索赔，索赔是子和nameidentifier
            // 根据外部提供商，可能会使用其他一些声明类型
            var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (userIdClaim == null)
            {
                userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }
            if (userIdClaim == null)
            {
                throw new Exception("Unknown userid");
            }

            // 从声明集合中删除用户标识声明并移至userId属性
            //还设置外部身份验证提供程序的名称
            claims.Remove(userIdClaim);
            var provider = result.Properties.Items["scheme"];
            var userId = userIdClaim.Value;

            //这是最有可能需要自定义逻辑来匹配您的用户的地方
            //外部提供商的身份验证结果，并根据需要配置用户。
            //
            //检查外部用户是否已配置内部用户
            var user = _users.FindByExternalProvider(provider, userId);
            if (user == null)
            {
                //此示例只是自动提供新的外部用户
                //另一种常见方法是首先启动注册工作流程
                user = _users.AutoProvisionUser(provider, userId, claims);
            }

            var additionalClaims = new List<Claim>();

            //如果外部系统发送了会话ID声明，请复制它
            //所以我们可以用它来进行单点登出
            var sid = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                additionalClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            //如果外部提供者发出了id_token，我们会保留它以进行注销
            AuthenticationProperties props = null;
            var id_token = result.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                props = new AuthenticationProperties();
                props.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }

            // 为用户发出身份验证Cookie
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, userId, user.SubjectId, user.Username));
            await HttpContext.SignInAsync(user.SubjectId, user.Username, provider, props, additionalClaims.ToArray());
            //删除外部认证期间使用的临时cookie
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            //验证返回URL并重定向回授权端点或本地页面
            var returnUrl = result.Properties.Items["returnUrl"];

            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }
        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, treating windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("Callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }

    }
}