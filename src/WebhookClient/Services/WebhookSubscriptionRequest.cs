namespace eShop.WebhookClient.Services;

/// <summary>
/// 表示向 webhook 服务订阅的请求信息
/// </summary>
public class WebhookSubscriptionRequest
{
    /// <summary>
    /// 获取或设置订阅者的回调 URL，用于接收事件通知
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 获取或设置用于验证 webhook 请求的安全令牌
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// 获取或设置要订阅的事件类型
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// 获取或设置授权 URL，用于获取访问令牌
    /// </summary>
    public string? GrantUrl { get; set; }
}
