#nullable enable

namespace eShop.Basket.API.Extensions;

/// <summary>
/// 提供用于从gRPC ServerCallContext中提取用户身份信息的扩展方法
/// </summary>
internal static class ServerCallContextIdentityExtensions
{
    /// <summary>
    /// 从ServerCallContext中获取用户的唯一标识（sub声明）
    /// </summary>
    /// <param name="context">gRPC服务调用上下文</param>
    /// <returns>用户的唯一标识，如果未找到则返回null</returns>
    public static string? GetUserIdentity(this ServerCallContext context)
    {
        return context.GetHttpContext().User.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// 从ServerCallContext中获取用户名
    /// </summary>
    /// <param name="context">gRPC服务调用上下文</param>
    /// <returns>用户名，如果未找到则返回null</returns>
    public static string? GetUserName(this ServerCallContext context)
    {
        return context.GetHttpContext().User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
    }
}
