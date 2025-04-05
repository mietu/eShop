using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace eShop.WebhookClient.Extensions;

/// <summary>
/// 包含应用程序服务和身份验证配置的扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 向应用程序添加所需的服务
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 添加身份验证相关服务
        builder.AddAuthenticationServices();

        // 添加应用程序配置选项并绑定到配置
        builder.Services.AddOptions<WebhookClientOptions>().BindConfiguration(nameof(WebhookClientOptions));

        // 注册仓储服务作为单例
        builder.Services.AddSingleton<HooksRepository>();

        // 注册 HTTP 客户端并配置
        builder.Services.AddHttpClient<WebhooksClient>(o => o.BaseAddress = new("http://webhooks-api"))
            .AddApiVersion(1.0)  // 添加 API 版本
            .AddAuthToken();     // 添加身份验证令牌
    }

    /// <summary>
    /// 配置身份验证服务，包括 Cookie 和 OpenID Connect
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        // 从配置中获取必要的身份验证参数
        var identityUrl = configuration.GetRequiredValue("IdentityUrl");
        var callBackUrl = configuration.GetRequiredValue("CallBackUrl");
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // 添加授权和身份验证服务
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            // 设置默认的身份验证方案为 Cookie
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // 设置默认的质询方案为 OpenID Connect
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            // 配置 Cookie 过期时间
            options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime);

            // 设置唯一的 Cookie 名称以避免在本地开发时与其他应用冲突
            options.Cookie.Name = ".AspNetCore.WebHooksClientIdentity";
        })
        .AddOpenIdConnect(options =>
        {
            // 配置 OpenID Connect 参数
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = identityUrl.ToString();          // 身份服务器地址
            options.SignedOutRedirectUri = callBackUrl.ToString(); // 登出后重定向地址
            options.ClientId = "webhooksclient";                 // 客户端 ID
            options.ClientSecret = "secret";                     // 客户端密钥
            options.ResponseType = "code";                       // 授权码流程
            options.SaveTokens = true;                           // 保存令牌
            options.GetClaimsFromUserInfoEndpoint = true;        // 从用户信息端点获取声明
            options.RequireHttpsMetadata = false;                // 不要求 HTTPS 元数据
            options.Scope.Add("openid");                         // 添加 OpenID 范围
            options.Scope.Add("webhooks");                       // 添加 webhooks 范围
        });

        // 配置 Blazor 身份验证状态
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }
}
