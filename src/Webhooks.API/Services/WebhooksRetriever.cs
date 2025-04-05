namespace Webhooks.API.Services;

/// <summary>
/// 提供从数据库检索 Webhook 订阅信息的服务
/// </summary>
/// <remarks>
/// 该服务负责根据指定条件查询 Webhook 订阅数据
/// </remarks>
public class WebhooksRetriever(WebhooksContext db) : IWebhooksRetriever
{
    /// <summary>
    /// 获取指定类型的所有 Webhook 订阅
    /// </summary>
    /// <param name="type">要检索的 Webhook 类型</param>
    /// <returns>匹配指定类型的 Webhook 订阅集合</returns>
    public async Task<IEnumerable<WebhookSubscription>> GetSubscriptionsOfType(WebhookType type)
    {
        return await db.Subscriptions.Where(s => s.Type == type).ToListAsync();
    }
}
