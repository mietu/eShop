// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI
{
    /// <summary>
    /// 账户控制器，处理用户认证相关的所有操作，包括登录、注销和拒绝访问
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IAuthenticationHandlerProvider _handlerProvider;
        private readonly IEventService _events;

        /// <summary>
        /// 构造函数，注入所需的服务依赖
        /// </summary>
        /// <param name="userManager">ASP.NET Identity用户管理器</param>
        /// <param name="signInManager">ASP.NET Identity登录管理器</param>
        /// <param name="interaction">IdentityServer交互服务，用于处理授权请求和登出</param>
        /// <param name="clientStore">客户端存储，用于获取注册的客户端信息</param>
        /// <param name="schemeProvider">认证方案提供程序，用于获取可用的认证方案</param>
        /// <param name="handlerProvider">认证处理程序提供程序，用于获取认证处理程序</param>
        /// <param name="events">事件服务，用于记录认证事件</param>
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IAuthenticationHandlerProvider handlerProvider,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _handlerProvider = handlerProvider;
            _events = events;
        }

        /// <summary>
        /// 登录流程的入口点，显示登录页面
        /// </summary>
        /// <param name="returnUrl">登录成功后重定向的URL</param>
        /// <returns>登录视图或重定向到外部登录提供程序</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // 构建视图模型，用于显示登录页面
            var vm = await BuildLoginViewModelAsync(returnUrl);

            ViewData["ReturnUrl"] = returnUrl;

            if (vm.IsExternalLoginOnly)
            {
                // 如果只有一个外部登录选项，直接重定向到该外部提供程序
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// 处理用户名/密码登录表单提交
        /// </summary>
        /// <param name="model">登录输入模型，包含用户名、密码等</param>
        /// <param name="button">提交的按钮值，用于确定是登录还是取消</param>
        /// <returns>根据登录结果重定向到合适的页面</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // 检查是否在授权请求上下文中
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // 用户点击了"取消"按钮
            if (button != "login")
            {
                if (context != null)
                {
                    // 如果用户取消，向IdentityServer发送结果，表示拒绝同意
                    // 即使此客户端不需要同意，也会发送访问拒绝的OIDC错误响应
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // 我们可以信任model.ReturnUrl，因为GetAuthorizationContextAsync返回了非空值
                    if (context.IsNativeClient())
                    {
                        // 客户端是原生应用，这种响应返回方式可以为最终用户提供更好的体验
                        return this.LoadingPage("Redirect", model.ReturnUrl);
                    }

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // 由于没有有效的上下文，返回首页
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // 客户端是原生应用，这种响应返回方式可以为最终用户提供更好的体验
                            return this.LoadingPage("Redirect", model.ReturnUrl);
                        }

                        // 我们可以信任model.ReturnUrl，因为GetAuthorizationContextAsync返回了非空值
                        return Redirect(model.ReturnUrl);
                    }

                    // 请求本地页面
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
                        // 用户可能点击了恶意链接 - 应该记录
                        throw new Exception("无效的返回URL");
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "无效的凭据", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // 出现错误，显示带有错误信息的表单
            var vm = await BuildLoginViewModelAsync(model);

            ViewData["ReturnUrl"] = model.ReturnUrl;

            return View(vm);
        }

        /// <summary>
        /// 显示注销页面
        /// </summary>
        /// <param name="logoutId">注销操作的ID</param>
        /// <returns>注销视图或直接执行注销</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // 构建视图模型，用于显示注销页面
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // 如果来自IdentityServer的注销请求已通过身份验证，
                // 则不需要显示提示，可以直接注销用户
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// 处理注销页面的表单提交
        /// </summary>
        /// <param name="model">注销输入模型</param>
        /// <returns>注销完成视图或重定向到外部身份提供商进行注销</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // 构建视图模型，用于显示注销完成页面
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // 删除本地身份验证Cookie
                await _signInManager.SignOutAsync();

                // 触发注销事件
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // 检查是否需要在上游身份提供商触发注销
            if (vm.TriggerExternalSignout)
            {
                // 构建返回URL，以便上游提供商在用户注销后重定向回来
                // 这允许我们完成单点注销处理
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // 触发重定向到外部提供商进行注销
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// 显示访问被拒绝页面
        /// </summary>
        /// <returns>访问被拒绝视图</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* AccountController的辅助方法 */
        /*****************************************/

        /// <summary>
        /// 构建登录视图模型
        /// </summary>
        /// <param name="returnUrl">登录成功后重定向的URL</param>
        /// <returns>配置好的登录视图模型</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // 这是为了简化UI，只触发一个外部IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
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

        /// <summary>
        /// 基于输入模型构建登录视图模型
        /// </summary>
        /// <param name="model">登录输入模型</param>
        /// <returns>配置好的登录视图模型</returns>
        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        /// <summary>
        /// 构建注销视图模型
        /// </summary>
        /// <param name="logoutId">注销操作的ID</param>
        /// <returns>配置好的注销视图模型</returns>
        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // 如果用户未通过身份验证，则只显示注销完成页面
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // 安全地自动注销
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // 显示注销提示。这可以防止用户被其他恶意网页自动注销的攻击。
            return vm;
        }

        /// <summary>
        /// 构建注销完成视图模型
        /// </summary>
        /// <param name="logoutId">注销操作的ID</param>
        /// <returns>配置好的注销完成视图模型</returns>
        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // 获取上下文信息（客户端名称、注销后重定向URI和用于联合注销的iframe）
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
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var handler = await _handlerProvider.GetHandlerAsync(HttpContext, idp);
                    if (handler is IAuthenticationSignOutHandler)
                    {
                        if (vm.LogoutId == null)
                        {
                            // 如果没有当前的注销上下文，需要创建一个
                            // 这会在注销并重定向到外部IdP进行注销之前
                            // 捕获当前已登录用户的必要信息
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}
