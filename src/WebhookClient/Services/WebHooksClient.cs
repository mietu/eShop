namespace eShop.WebhookClient.Services;

/// <summary>
/// 客户端服务，用于与Webhook API进行通信，提供创建和检索webhook的功能。
/// </summary>
public class WebhooksClient(HttpClient client)
{
    /// <summary>
    /// 向API注册新的Webhook订阅。
    /// </summary>
    /// <param name="payload">包含Webhook订阅详情的请求对象，包括URL、令牌、事件类型和授权URL。</param>
    /// <returns>表示HTTP响应的任务对象，可用于检查操作结果。</returns>
    public Task<HttpResponseMessage> AddWebHookAsync(WebhookSubscriptionRequest payload)
    {
        return client.PostAsJsonAsync("/api/webhooks", payload);
    }

    /// <summary>
    /// 从API加载所有已注册的Webhook。
    /// </summary>
    /// <returns>注册的Webhook列表，如果没有或请求失败则返回空列表。</returns>
    public async Task<IEnumerable<WebhookResponse>> LoadWebhooks()
    {
        return await client.GetFromJsonAsync<IEnumerable<WebhookResponse>>("/api/webhooks") ?? [];
    }
}
