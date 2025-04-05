namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 处理产品价格变更集成事件的处理程序
/// </summary>
public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;
    private readonly IWebhooksRetriever _webhooksRetriever;
    private readonly IWebhooksSender _webhooksSender;

    /// <summary>
    /// 初始化<see cref="ProductPriceChangedIntegrationEventHandler"/>类的新实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="webhooksRetriever">Webhook检索服务</param>
    /// <param name="webhooksSender">Webhook发送服务</param>
    public ProductPriceChangedIntegrationEventHandler(
        ILogger<ProductPriceChangedIntegrationEventHandler> logger,
        IWebhooksRetriever webhooksRetriever,
        IWebhooksSender webhooksSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webhooksRetriever = webhooksRetriever ?? throw new ArgumentNullException(nameof(webhooksRetriever));
        _webhooksSender = webhooksSender ?? throw new ArgumentNullException(nameof(webhooksSender));
    }

    /// <summary>
    /// 处理产品价格变更事件
    /// </summary>
    /// <param name="event">产品价格变更事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(ProductPriceChangedIntegrationEvent @event)
    {
        _logger.LogInformation("处理产品价格变更事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        var webhooks = await _webhooksRetriever.GetSubscriptionsOfType(WebhookType.ProductPriceChanged);

        _logger.LogInformation("找到 {WebhooksCount} 个订阅了产品价格变更的Webhook", webhooks.Count());

        var whContent = new WebhookData(WebhookType.ProductPriceChanged, @event);

        await _webhooksSender.SendAll(webhooks, whContent);
    }
}
