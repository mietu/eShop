namespace Webhooks.API.Services;

/// <summary>
/// 定义用于发送 Webhook 通知的服务
/// </summary>
public interface IWebhooksSender
{
    /// <summary>
    /// 向多个接收者发送相同的 Webhook 事件数据
    /// </summary>
    /// <param name="receivers">要接收通知的 Webhook 订阅集合</param>
    /// <param name="data">要发送的 Webhook 事件数据</param>
    /// <returns>表示异步操作的任务</returns>
    Task SendAll(IEnumerable<WebhookSubscription> receivers, WebhookData data);
}
