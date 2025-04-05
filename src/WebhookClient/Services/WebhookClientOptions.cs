namespace eShop.WebhookClient.Services;

/// <summary>
/// 配置选项类，用于存储Webhook客户端的配置信息。
/// </summary>
public class WebhookClientOptions
{
    /// <summary>
    /// 获取或设置用于验证Webhook请求的令牌。
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// 获取或设置当前Webhook客户端的URL地址，用于回调通知。
    /// </summary>
    public string? SelfUrl { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示是否验证接收到的Webhook请求的令牌。
    /// </summary>
    public bool ValidateToken { get; set; }
}
