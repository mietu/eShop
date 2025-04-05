namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为"已支付"的集成事件处理程序。
/// 当订单状态更改为已支付时，此处理程序负责通知所有订阅了此类事件的接收者。
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler(
    /// <summary>Webhook订阅检索服务，用于获取订阅了特定类型事件的接收者</summary>
    IWebhooksRetriever retriever,
    /// <summary>Webhook发送服务，用于向订阅者发送通知</summary>
    IWebhooksSender sender,
    /// <summary>日志记录器，用于记录处理事件的信息</summary>
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已支付的集成事件
    /// </summary>
    /// <param name="event">包含订单已支付状态信息的事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        // 获取所有订阅了"订单已支付"事件类型的接收者
        var subscriptions = await retriever.GetSubscriptionsOfType(WebhookType.OrderPaid);

        logger.LogInformation("已接收 OrderStatusChangedToShippedIntegrationEvent 并已获取 {SubscriptionsCount} 个订阅进行处理", subscriptions.Count());

        // 创建包含事件数据的Webhook负载
        var whook = new WebhookData(WebhookType.OrderPaid, @event);

        // 向所有订阅者发送webhook通知
        await sender.SendAll(subscriptions, whook);
    }
}
