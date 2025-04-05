// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 同意控制器 - 处理用户授权同意界面和流程
/// </summary>
[SecurityHeaders]
[Authorize]
public class ConsentController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<ConsentController> _logger;

    /// <summary>
    /// 同意控制器构造函数
    /// </summary>
    /// <param name="interaction">IdentityServer交互服务，用于处理授权请求</param>
    /// <param name="events">事件服务，用于记录授权相关事件</param>
    /// <param name="logger">日志记录器</param>
    public ConsentController(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<ConsentController> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// 显示同意授权页面
    /// </summary>
    /// <param name="returnUrl">授权完成后的返回URL</param>
    /// <returns>同意页面视图或错误视图</returns>
    [HttpGet]
    public async Task<IActionResult> Index(string returnUrl)
    {
        // 构建同意页面视图模型
        var vm = await BuildViewModelAsync(returnUrl);
        if (vm != null)
        {
            return View("Index", vm);
        }

        return View("Error");
    }

    /// <summary>
    /// 处理用户提交的同意授权决定
    /// </summary>
    /// <param name="model">用户提交的同意输入模型</param>
    /// <returns>重定向到授权源或显示错误</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConsentInputModel model)
    {
        // 处理用户提交的同意决定
        var result = await ProcessConsent(model);

        // 处理重定向场景
        if (result.IsRedirect)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            if (context?.IsNativeClient() == true)
            {
                // 如果是原生客户端，使用加载页面提供更好的用户体验
                return this.LoadingPage("Redirect", result.RedirectUri);
            }

            return Redirect(result.RedirectUri);
        }

        // 处理验证错误
        if (result.HasValidationError)
        {
            ModelState.AddModelError(string.Empty, result.ValidationError);
        }

        // 需要重新显示同意页面
        if (result.ShowView)
        {
            return View("Index", result.ViewModel);
        }

        return View("Error");
    }

    /*****************************************/
    /* 同意控制器的辅助方法 */
    /*****************************************/

    /// <summary>
    /// 处理用户的授权同意决定
    /// </summary>
    /// <param name="model">用户提交的同意输入模型</param>
    /// <returns>处理结果，包含重定向信息或错误信息</returns>
    private async Task<ProcessConsentResult> ProcessConsent(ConsentInputModel model)
    {
        var result = new ProcessConsentResult();

        // 验证返回URL是否仍然有效
        var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
        if (request == null) return result;

        ConsentResponse grantedConsent = null;

        // 用户点击"拒绝" - 返回标准的"访问被拒绝"响应
        if (model?.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            // 触发拒绝同意事件
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
        }
        // 用户点击"同意" - 验证数据
        else if (model?.Button == "yes")
        {
            // 如果用户同意了某些权限范围，构建响应模型
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                var scopes = model.ScopesConsented;
                // 如果禁用了离线访问，过滤掉离线访问权限
                if (ConsentOptions.EnableOfflineAccess == false)
                {
                    scopes = scopes.Where(x => x != IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                grantedConsent = new ConsentResponse
                {
                    RememberConsent = model.RememberConsent,
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = model.Description
                };

                // 触发授予同意事件
                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
            }
            else
            {
                // 用户没有选择任何权限范围
                result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
            }
        }
        else
        {
            // 未知的提交按钮值
            result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
        }

        if (grantedConsent != null)
        {
            // 将同意结果传达给IdentityServer
            await _interaction.GrantConsentAsync(request, grantedConsent);

            // 表明可以重定向回授权端点
            result.RedirectUri = model.ReturnUrl;
            result.Client = request.Client;
        }
        else
        {
            // 需要重新显示同意UI
            result.ViewModel = await BuildViewModelAsync(model.ReturnUrl, model);
        }

        return result;
    }

    /// <summary>
    /// 构建同意页面的视图模型
    /// </summary>
    /// <param name="returnUrl">授权完成后的返回URL</param>
    /// <param name="model">可选的用户输入模型，用于保持表单状态</param>
    /// <returns>同意页面视图模型</returns>
    private async Task<ConsentViewModel> BuildViewModelAsync(string returnUrl, ConsentInputModel model = null)
    {
        // 获取授权上下文
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (request != null)
        {
            return CreateConsentViewModel(model, returnUrl, request);
        }
        else
        {
            _logger.LogError("找不到与请求匹配的同意请求: {0}", returnUrl);
        }

        return null;
    }

    /// <summary>
    /// 创建同意页面视图模型
    /// </summary>
    /// <param name="model">用户输入模型</param>
    /// <param name="returnUrl">返回URL</param>
    /// <param name="request">授权请求</param>
    /// <returns>包含所有需要展示信息的视图模型</returns>
    private ConsentViewModel CreateConsentViewModel(
        ConsentInputModel model, string returnUrl,
        AuthorizationRequest request)
    {
        // 创建视图模型并设置基本属性
        var vm = new ConsentViewModel
        {
            // 默认记住同意选择，除非模型中指定
            RememberConsent = model?.RememberConsent ?? true,
            // 用户同意的作用域，或空集合
            ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
            Description = model?.Description,

            ReturnUrl = returnUrl,

            // 客户端信息
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };

        // 处理身份资源作用域
        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources.Select(x =>
            CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

        // 处理API资源作用域
        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope,
                    vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }

        // 处理离线访问作用域
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(
                vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    /// <summary>
    /// 创建身份资源的作用域视图模型
    /// </summary>
    /// <param name="identity">身份资源</param>
    /// <param name="check">是否默认选中</param>
    /// <returns>作用域视图模型</returns>
    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    /// <summary>
    /// 创建API资源的作用域视图模型
    /// </summary>
    /// <param name="parsedScopeValue">解析的作用域值</param>
    /// <param name="apiScope">API作用域</param>
    /// <param name="check">是否默认选中</param>
    /// <returns>作用域视图模型</returns>
    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        // 如果有参数，则将参数添加到显示名称
        if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    /// <summary>
    /// 创建离线访问作用域的视图模型
    /// </summary>
    /// <param name="check">是否默认选中</param>
    /// <returns>离线访问作用域视图模型</returns>
    private ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
