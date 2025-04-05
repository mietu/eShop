namespace Webhooks.API.Services;

/// <summary>
/// 提供测试授权URL功能的服务接口
/// </summary>
public interface IGrantUrlTesterService
{
    /// <summary>
    /// 测试授权URL是否有效
    /// </summary>
    /// <param name="urlHook">Webhook URL或标识符</param>
    /// <param name="url">要测试的目标URL</param>
    /// <param name="token">用于授权验证的令牌</param>
    /// <returns>如果URL测试成功返回true，否则返回false</returns>
    Task<bool> TestGrantUrl(string urlHook, string url, string token);
}
