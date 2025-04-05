using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace eShop.WebApp.Services;

/// <summary>
/// 提供用户登出相关功能的服务
/// </summary>
public class LogOutService
{
    /// <summary>
    /// 执行用户登出操作
    /// </summary>
    /// <param name="httpContext">当前HTTP上下文</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 此方法会同时从Cookie认证和OpenID Connect认证中登出用户
    /// </remarks>
    public async Task LogOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}
