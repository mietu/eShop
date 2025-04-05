namespace Webhooks.API.Services;

/// <summary>
/// Webhook发送服务，负责将Webhook事件数据发送到订阅者的URL端点
/// </summary>
/// <remarks>
/// 该服务使用HttpClient发送POST请求，并在请求头中包含认证令牌（如果提供）
/// </remarks>
public class WebhooksSender(IHttpClientFactory httpClientFactory, ILogger<WebhooksSender> logger) : IWebhooksSender
{
    /// <summary>
    /// 向所有订阅者发送webhook数据
    /// </summary>
    /// <param name="receivers">接收者订阅列表</param>
    /// <param name="data">要发送的webhook事件数据</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SendAll(IEnumerable<WebhookSubscription> receivers, WebhookData data)
    {
        var client = httpClientFactory.CreateClient();
        var json = JsonSerializer.Serialize(data);
        var tasks = receivers.Select(r => OnSendData(r, json, client));
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 向单个订阅者发送webhook数据
    /// </summary>
    /// <param name="subs">webhook订阅信息</param>
    /// <param name="jsonData">序列化为JSON的webhook数据</param>
    /// <param name="client">用于发送HTTP请求的客户端</param>
    /// <returns>表示异步发送操作的任务</returns>
    private Task OnSendData(WebhookSubscription subs, string jsonData, HttpClient client)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(subs.DestUrl, UriKind.Absolute),
            Method = HttpMethod.Post,
            Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(subs.Token))
        {
            request.Headers.Add("X-eshop-whtoken", subs.Token);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Sending hook to {DestUrl} of type {Type}", subs.DestUrl, subs.Type);
        }

        return client.SendAsync(request);
    }

}
