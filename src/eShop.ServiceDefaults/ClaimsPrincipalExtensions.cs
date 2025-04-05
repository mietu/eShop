using System.Security.Claims;

namespace eShop.ServiceDefaults;

/// <summary>
/// 提供用于ClaimsPrincipal类的扩展方法，简化获取用户标识信息的操作
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// 从ClaimsPrincipal获取用户ID
    /// </summary>
    /// <param name="principal">用户凭据主体</param>
    /// <returns>用户ID，如果不存在则返回null</returns>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// 从ClaimsPrincipal获取用户名
    /// </summary>
    /// <param name="principal">用户凭据主体</param>
    /// <returns>用户名，如果不存在则返回null</returns>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
    }
}
