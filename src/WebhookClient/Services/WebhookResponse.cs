namespace eShop.WebhookClient.Services;

/// <summary>
/// 表示来自 Webhook 操作的响应信息
/// </summary>
public class WebhookResponse
{
    /// <summary>
    /// 获取或设置 Webhook 响应的日期时间
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的目标 URL
    /// </summary>
    public string? DestUrl { get; set; }

    /// <summary>
    /// 获取或设置用于验证 Webhook 的令牌
    /// </summary>
    public string? Token { get; set; }
}
