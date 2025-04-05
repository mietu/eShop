namespace eShop.WebhookClient.Services;

/// <summary>
/// 表示从外部系统接收到的Webhook通知数据
/// </summary>
public class WebHookReceived
{
    /// <summary>
    /// 获取或设置Webhook接收的时间戳
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    /// 获取或设置Webhook传递的数据内容
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// 获取或设置用于验证Webhook有效性的令牌
    /// </summary>
    public string? Token { get; set; }
}
