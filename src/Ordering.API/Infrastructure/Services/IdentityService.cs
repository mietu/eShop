namespace eShop.Ordering.API.Infrastructure.Services;

/// <summary>
/// 提供用户身份信息的服务
/// </summary>
/// <remarks>
/// 此服务通过HttpContext获取当前用户的身份信息，用于订单系统中的用户身份识别
/// </remarks>
public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    /// <summary>
    /// 获取当前用户的唯一标识（sub声明）
    /// </summary>
    /// <returns>用户的唯一标识，如果未找到则返回null</returns>
    public string GetUserIdentity()
        => context.HttpContext?.User.FindFirst("sub")?.Value;

    /// <summary>
    /// 获取当前用户的名称
    /// </summary>
    /// <returns>用户名，如果未找到则返回null</returns>
    public string GetUserName()
        => context.HttpContext?.User.Identity?.Name;
}
