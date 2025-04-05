namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 处理外部身份验证（如Google、Facebook等）的控制器
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class ExternalController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IEventService _events;
    private readonly ILogger<ExternalController> _logger;

    /// <summary>
    /// 构造函数，注入所需的服务
    /// </summary>
    public ExternalController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IEventService events,
        ILogger<ExternalController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _clientStore = clientStore;
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// 发起与外部身份验证提供程序的交互
    /// </summary>
    /// <param name="scheme">身份验证方案，如"Google"、"Facebook"等</param>
    /// <param name="returnUrl">认证成功后的重定向URL</param>
    /// <returns>重定向到外部登录提供商的Action结果</returns>
    [HttpGet]
    public IActionResult Challenge(string scheme, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        // 验证returnUrl - 必须是有效的OIDC URL或本地页面
        if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
        {
            // 用户可能点击了恶意链接 - 应该记录
            throw new Exception("invalid return URL");
        }

        // 启动挑战并传递returnUrl和scheme 
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                }
        };

        return Challenge(props, scheme);
    }

    /// <summary>
    /// 外部身份验证后的回调处理
    /// </summary>
    /// <returns>根据认证结果重定向到相应页面</returns>
    [HttpGet]
    public async Task<IActionResult> Callback()
    {
        // 从临时cookie中读取外部身份信息
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        // 调试级别记录外部声明信息
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // 查找用户和外部提供程序信息
        var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
        if (user == null)
        {
            // 如果用户不存在，自动创建新用户
            // 在实际应用中，可能需要自定义注册流程
            // 此示例简单地自动创建新用户
            user = await AutoProvisionUserAsync(provider, providerUserId, claims);
        }

        // 收集特定协议使用的额外声明或属性
        // 通常用于存储注销所需的数据
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

        // 为用户颁发身份验证cookie
        // 需要手动颁发cookie，不能使用SignInManager
        // 因为它没有提供API来从登录工作流中颁发额外的声明
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        additionalLocalClaims.AddRange(principal.Claims);
        var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

        var isuser = new IdentityServerUser(user.Id)
        {
            DisplayName = name,
            IdentityProvider = provider,
            AdditionalClaims = additionalLocalClaims
        };

        // 在本地登录用户
        await HttpContext.SignInAsync(isuser, localSignInProps);

        // 删除外部身份验证期间使用的临时cookie
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // 获取返回URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // 检查外部登录是否在OIDC请求的上下文中
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true, context?.Client.ClientId));

        // 对于原生客户端，调整响应方式以提供更好的用户体验
        if (context != null)
        {
            if (context.IsNativeClient())
            {
                return this.LoadingPage("Redirect", returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    /// <summary>
    /// 从外部提供程序查找用户
    /// </summary>
    /// <param name="result">身份验证结果</param>
    /// <returns>用户、提供程序、提供程序用户ID和声明的元组</returns>
    private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)>
        FindUserFromExternalProviderAsync(AuthenticateResult result)
    {
        var externalUser = result.Principal;

        // 尝试确定外部用户的唯一ID（由提供程序颁发）
        // 最常见的声明类型是sub声明和NameIdentifier
        // 根据外部提供程序的不同，可能使用其他声明类型
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        // 移除用户ID声明，以免在预置用户时将其作为额外声明包含
        var claims = externalUser.Claims.ToList();
        claims.Remove(userIdClaim);

        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;

        // 查找外部用户
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);

        return (user, provider, providerUserId, claims);
    }

    /// <summary>
    /// 自动创建新用户
    /// </summary>
    /// <param name="provider">身份验证提供程序</param>
    /// <param name="providerUserId">提供程序用户ID</param>
    /// <param name="claims">用户声明</param>
    /// <returns>新创建的用户</returns>
    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        // 创建要转入存储的声明列表
        var filtered = new List<Claim>();

        // 用户显示名称
        var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
            claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (name != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Name, name));
        }
        else
        {
            // 如果没有完整名称，尝试组合名字和姓氏
            var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
            if (first != null && last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
            }
            else if (first != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, first));
            }
            else if (last != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, last));
            }
        }

        // 电子邮件
        var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
           claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email != null)
        {
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        }

        // 创建新用户
        var user = new ApplicationUser
        {
            UserName = Guid.NewGuid().ToString(),
        };
        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        // 添加筛选后的声明
        if (filtered.Any())
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
        }

        // 添加外部登录信息
        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

        return user;
    }

    /// <summary>
    /// 处理登录回调
    /// 如果外部登录基于OIDC，需要保留某些信息以使注销正常工作
    /// 对于WS-Fed、SAML2p或其他协议，处理方式会有所不同
    /// </summary>
    /// <param name="externalResult">外部身份验证结果</param>
    /// <param name="localClaims">本地声明列表</param>
    /// <param name="localSignInProps">本地登录属性</param>
    private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // 如果外部系统发送了会话ID声明，复制它
        // 以便用于单点注销
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // 如果外部提供程序颁发了id_token，保留它用于注销
        var idToken = externalResult.Properties.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}
