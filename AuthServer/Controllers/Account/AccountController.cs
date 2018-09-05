// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Components.Sugar;
using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    ///此示例控制器为本地和外部帐户实现典型的登录/注销/提供工作流程。
    ///登录服务封装了与用户数据存储的交互。 此数据存储仅在内存中，不能用于生产！
    ///交互服务为UI提供了一种与身份服务器通信以进行验证和上下文检索的方法
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        #region 原生代码
        /// <summary>
        /// 测试用户信息
        /// </summary>
        private readonly TestUserStore _users;
        /// <summary>
        /// 提供用户界面使用的服务与IdentityServer进行通信。
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;
        /// <summary>
        /// 检索客户端配置
        /// </summary>
        private readonly IClientStore _clientStore;
        /// <summary>
        /// 负责管理支持的authenticationSchemes。
        /// </summary>
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        /// <summary>
        /// 事件服务的接口
        /// </summary>
        private readonly IEventService _events;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            TestUserStore users = null)
        {
            //如果TestUserStore不在DI中，那么我们将只使用全局用户集合
            //这是您插入自己的自定义身份管理库的地方（例如ASP.NET身份）
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
        }

        /// <summary>
        /// 进入登录工作流程的入口点
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // 建立一个模型，以便我们知道登录页面上显示的内容
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // 我们只有一个登录选项，它是一个外部提供商
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login1(LoginInputModel model, string button)
        {
            // 检查我们是否在授权请求的上下文中
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // 用户点击了“取消”按钮
            if (button != "login")
            {
                if (context != null)
                {
                    //如果用户取消，则将结果发送回IdentityServer，就像它们一样
                    //拒绝同意（即使此客户不需要同意）。
                    //这将向客户端发回拒绝访问的OIDC错误响应。
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // 我们可以信任model.ReturnUrl，因为GetAuthorizationContextAsync返回非null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        //如果客户端是PKCE，那么我们假设它是原生的，所以这个改变如何
                        //返回响应是为了为最终用户提供更好的用户体验。
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // 因为我们没有有效的上下文，所以我们只需返回主页
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                // 验证内存存储中的用户名/密码
                if (_users.ValidateCredentials(model.Username, model.Password))
                {
                    //按用户名查找用户。
                    var user = _users.FindByUsername(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));

                    //如果用户选择“记住我”，则仅在此设置明确的到期日期。
                    //否则我们依赖于cookie中间件中配置的到期。
                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };

                    //使用用户名信息发出身份验证Cookie
                    await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

                    if (context != null)
                    {
                        if (await _clientStore.IsPkceClientAsync(context.ClientId))
                        {
                            //如果客户端是PKCE，那么我们假设它是原生的，所以这个改变如何
                            //返回响应是为了为最终用户提供更好的用户体验。
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        // 我们可以信任model.ReturnUrl，因为GetAuthorizationContextAsync返回非null
                        return Redirect(model.ReturnUrl);
                    }

                    // 重定向到登陆后的页面
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // 用户可能已经点击了恶意链接 - 应该被记录
                        throw new Exception("无效的返回网址");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "无效用户名或密码"));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // 出了错误，显示有错误的信息
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }
        /// <summary>
        /// 显示注销页面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // 构建模型，以便注销页面知道要显示的内容
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                //如果从IdentityServer正确验证了注销请求，那么
                //我们不需要显示提示，只能直接将用户注销。
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        ///处理注销页面回发
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            //构建模型，以便注销页面知道要显示的内容
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // 删除本地认证cookie
                await HttpContext.SignOutAsync();

                // 提交注销事件
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // 检查我们是否需要在上游身份提供商处触发注销
            if (vm.TriggerExternalSignout)
            {
                //构建一个返回URL，以便上游提供者重定向回来
                //在用户退出后向我们发送消息。 这让我们接受了
                //完成我们的单点登出处理。
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // 这会触发重定向到外部提供程序以进行注销
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }
            //正常退出直接重定向 不使用系统退出流程
            if (!string.IsNullOrEmpty(vm.PostLogoutRedirectUri))
            {
                return Redirect(vm.PostLogoutRedirectUri);
            }
            else
            {
                return Redirect("/home/index");
            }
            // return View("LoggedOut", vm);
        }



        /*****************************************/
        /* AccountController的帮助API */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                //这意味着短路UI并且仅触发一个外部IdP
                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                //如果用户未经过身份验证，则只显示已注销的页面
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {

                //自动注销是安全的
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            //显示注销提示。 这可以防止用户攻击
            //由另一个恶意网页自动注销。
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // 获取上下文信息（客户端名称，发布注销重定向URI和联合注销的iframe）
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            //如果没有当前的注销上下文，我们需要创建一个
                            //这会捕获当前登录用户的必要信息
                            //在我们退出并重定向到外部IdP以进行注销之前
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
        #endregion
        /// <summary>
        /// 自定义登录
        /// </summary>
        /// <param name="model"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // 检查我们是否在授权请求的上下文中
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            // 用户点击了“取消”按钮
            if (button != "login")
            {
                if (context != null)
                {
                    //如果用户取消，则将结果发送回IdentityServer，就像它们一样
                    //拒绝同意（即使此客户不需要同意）。
                    //这将向客户端发回拒绝访问的OIDC错误响应。
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // 我们可以信任model.ReturnUrl，因为GetAuthorizationContextAsync返回非null
                    if (await _clientStore.IsPkceClientAsync(context.ClientId))
                    {
                        //如果客户端是PKCE，那么我们假设它是原生的，所以这个改变如何
                        //返回响应是为了为最终用户提供更好的用户体验。
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }
                    return Redirect("http://localhost:5002/");
                }
                else
                {
                    // 因为我们没有有效的上下文，所以我们只需返回主页
                    return Redirect("~/");
                }
            }
            if (ModelState.IsValid)
            {
                // 验证内存存储中的用户名/密码
                var user = SqlSugarDbContext.Instance.ExecutedSql().Queryable<User>().Where(w => w.Phone == model.Username).First();
                if (user != null)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.Name, user.Id.ToString(), user.Name));

                    AuthenticationProperties props = null;
                    if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                        };
                    };
                    //使用用户名信息发出身份验证Cookie
                    await HttpContext.SignInAsync(user.Id.ToString(), user.Name, props);
                    //重定向到登陆后的页面
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "无效用户名或密码"));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // 出了错误，显示有错误的信息
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }
    }
}