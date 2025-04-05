namespace eShop.WebhookClient.Services;

/// <summary>
/// 表示从外部系统接收的 Webhook 数据
/// </summary>
public class WebhookData
{
    /// <summary>
    /// 获取或设置 Webhook 接收的时间
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的负载内容（通常为 JSON 格式）
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// 获取或设置 Webhook 的类型标识
    /// </summary>
    public string? Type { get; set; }
}
