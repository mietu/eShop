namespace Webhooks.API.Services;

/// <summary>
/// 定义用于检索 Webhook 订阅信息的服务接口
/// </summary>
public interface IWebhooksRetriever
{
    /// <summary>
    /// 根据指定的 Webhook 类型获取相关的订阅列表
    /// </summary>
    /// <param name="type">要检索的 Webhook 类型</param>
    /// <returns>指定类型的 Webhook 订阅集合</returns>
    Task<IEnumerable<WebhookSubscription>> GetSubscriptionsOfType(WebhookType type);
}
