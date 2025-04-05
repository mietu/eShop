using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebhookClient.Endpoints;

/// <summary>
/// 提供身份验证相关的终结点映射
/// </summary>
public static class AuthenticationEndpoints
{
    /// <summary>
    /// 将身份验证相关的终结点映射到应用程序的路由系统
    /// </summary>
    /// <param name="app">应用程序的终结点路由构建器</param>
    /// <returns>配置后的终结点路由构建器以支持方法链</returns>
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        // 映射注销端点
        app.MapPost("/logout", async (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            // 验证请求以防止跨站请求伪造(CSRF)攻击
            await antiforgery.ValidateRequestAsync(httpContext);

            // 从Cookie身份验证方案中注销用户
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 从OpenID Connect身份验证方案中注销用户
            // 这将通知身份提供者用户已退出
            await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

        return app;
    }
}
