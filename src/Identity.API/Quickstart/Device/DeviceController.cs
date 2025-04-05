// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 设备授权控制器
/// 用于处理设备码流程(Device Flow)的用户交互部分，允许用户在单独的设备上授权客户端应用
/// </summary>
[Authorize]
[SecurityHeaders]
public class DeviceController : Controller
{
    private readonly IDeviceFlowInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IOptions<IdentityServerOptions> _options;
    private readonly ILogger<DeviceController> _logger;

    /// <summary>
    /// 构造函数，注入所需的服务
    /// </summary>
    /// <param name="interaction">设备流交互服务，用于获取和处理授权请求</param>
    /// <param name="eventService">事件服务，用于记录授权相关事件</param>
    /// <param name="options">Identity Server配置选项</param>
    /// <param name="logger">日志服务</param>
    public DeviceController(
        IDeviceFlowInteractionService interaction,
        IEventService eventService,
        IOptions<IdentityServerOptions> options,
        ILogger<DeviceController> logger)
    {
        _interaction = interaction;
        _events = eventService;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 处理设备授权页面的初始请求
    /// 如果URL中包含用户码参数，则直接显示确认页面；否则显示用户码输入页面
    /// </summary>
    /// <returns>用户码输入视图或用户码确认视图</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string userCodeParamName = _options.Value.UserInteraction.DeviceVerificationUserCodeParameter;
        string userCode = Request.Query[userCodeParamName];
        if (string.IsNullOrWhiteSpace(userCode)) return View("UserCodeCapture");

        var vm = await BuildViewModelAsync(userCode);
        if (vm == null) return View("Error");

        vm.ConfirmUserCode = true;
        return View("UserCodeConfirmation", vm);
    }

    /// <summary>
    /// 处理用户提交的用户码
    /// 验证用户码并显示授权确认页面
    /// </summary>
    /// <param name="userCode">用户输入的设备码</param>
    /// <returns>确认视图或错误视图</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserCodeCapture(string userCode)
    {
        var vm = await BuildViewModelAsync(userCode);
        if (vm == null) return View("Error");

        return View("UserCodeConfirmation", vm);
    }

    /// <summary>
    /// 处理用户对授权请求的响应（同意或拒绝）
    /// </summary>
    /// <param name="model">包含用户选择的授权输入模型</param>
    /// <returns>成功视图或错误视图</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Callback(DeviceAuthorizationInputModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var result = await ProcessConsent(model);
        if (result.HasValidationError) return View("Error");

        return View("Success");
    }

    /// <summary>
    /// 处理用户的授权同意选择
    /// </summary>
    /// <param name="model">授权输入模型，包含用户的选择</param>
    /// <returns>处理结果，包含后续操作信息</returns>
    private async Task<ProcessConsentResult> ProcessConsent(DeviceAuthorizationInputModel model)
    {
        var result = new ProcessConsentResult();

        var request = await _interaction.GetAuthorizationContextAsync(model.UserCode);
        if (request == null) return result;

        ConsentResponse grantedConsent = null;

        // 用户点击"拒绝"- 返回标准的"access_denied"响应
        if (model.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            // 发送拒绝授权事件
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
        }
        // 用户点击"同意"- 验证数据
        else if (model.Button == "yes")
        {
            // 如果用户同意了某些作用域，构建响应模型
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                var scopes = model.ScopesConsented;
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

                // 发送授权同意事件
                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
            }
            else
            {
                result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
            }
        }
        else
        {
            result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
        }

        if (grantedConsent != null)
        {
            // 将授权结果发送回IdentityServer
            await _interaction.HandleRequestAsync(model.UserCode, grantedConsent);

            // 指示可以重定向回授权端点
            result.RedirectUri = model.ReturnUrl;
            result.Client = request.Client;
        }
        else
        {
            // 需要重新显示授权同意UI
            result.ViewModel = await BuildViewModelAsync(model.UserCode, model);
        }

        return result;
    }

    /// <summary>
    /// 根据用户码构建授权视图模型
    /// </summary>
    /// <param name="userCode">用户输入的设备码</param>
    /// <param name="model">可选的授权输入模型，包含用户之前的选择</param>
    /// <returns>设备授权视图模型或null（如果找不到对应请求）</returns>
    private async Task<DeviceAuthorizationViewModel> BuildViewModelAsync(string userCode, DeviceAuthorizationInputModel model = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(userCode);
        if (request != null)
        {
            return CreateConsentViewModel(userCode, model, request);
        }

        return null;
    }

    /// <summary>
    /// 创建同意页面的视图模型
    /// </summary>
    /// <param name="userCode">用户码</param>
    /// <param name="model">授权输入模型</param>
    /// <param name="request">设备流授权请求</param>
    /// <returns>设备授权视图模型</returns>
    private DeviceAuthorizationViewModel CreateConsentViewModel(string userCode, DeviceAuthorizationInputModel model, DeviceFlowAuthorizationRequest request)
    {
        var vm = new DeviceAuthorizationViewModel
        {
            UserCode = userCode,
            Description = model?.Description,

            RememberConsent = model?.RememberConsent ?? true,
            ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),

            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };

        // 添加身份资源作用域
        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources.Select(x => CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

        // 添加API资源作用域
        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, vm.ScopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }

        // 添加离线访问作用域（如果启用）
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    /// <summary>
    /// 为身份资源创建作用域视图模型
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
    /// 为API作用域创建作用域视图模型
    /// </summary>
    /// <param name="parsedScopeValue">解析后的作用域值</param>
    /// <param name="apiScope">API作用域</param>
    /// <param name="check">是否默认选中</param>
    /// <returns>作用域视图模型</returns>
    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = apiScope.DisplayName ?? apiScope.Name,
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
    /// <returns>作用域视图模型</returns>
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
