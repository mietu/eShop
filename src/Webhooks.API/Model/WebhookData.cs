namespace Webhooks.API.Model;

/// <summary>
/// 表示要发送给订阅者的 Webhook 事件数据
/// </summary>
public class WebhookData
{
    /// <summary>
    /// 获取 Webhook 事件发生的 UTC 时间
    /// </summary>
    public DateTime When { get; }

    /// <summary>
    /// 获取 Webhook 事件的序列化 JSON 负载
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// 获取 Webhook 事件的类型名称
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// 使用指定的事件类型和数据初始化 <see cref="WebhookData"/> 类的新实例
    /// </summary>
    /// <param name="hookType">Webhook 事件的类型</param>
    /// <param name="data">要序列化为 JSON 的事件数据对象</param>
    public WebhookData(WebhookType hookType, object data)
    {
        When = DateTime.UtcNow;
        Type = hookType.ToString();
        Payload = JsonSerializer.Serialize(data);
    }
}
