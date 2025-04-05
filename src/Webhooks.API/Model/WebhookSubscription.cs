namespace Webhooks.API.Model;

/// <summary>
/// 表示系统中的一个 Webhook 订阅
/// </summary>
public class WebhookSubscription
{
    /// <summary>
    /// 获取或设置订阅的唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的事件类型
    /// </summary>
    public WebhookType Type { get; set; }

    /// <summary>
    /// 获取或设置订阅创建的日期和时间
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 获取或设置接收 Webhook 通知的目标 URL
    /// </summary>
    [Required]
    public string DestUrl { get; set; }

    /// <summary>
    /// 获取或设置用于验证 Webhook 的安全令牌
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 获取或设置订阅所属用户的唯一标识符
    /// </summary>
    [Required]
    public string UserId { get; set; }
}
