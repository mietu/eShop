namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为"已发货"的集成事件处理程序。
/// 当订单状态更改为已发货时，此处理程序会通知已订阅此类事件的所有webhook接收者。
/// </summary>
public class OrderStatusChangedToShippedIntegrationEventHandler(
    /// <summary>Webhook订阅检索服务，用于获取订阅了特定事件类型的Webhook</summary>
    IWebhooksRetriever retriever,
    /// <summary>Webhook发送服务，用于向订阅者发送通知</summary>
    IWebhooksSender sender,
    /// <summary>日志记录器，用于记录处理过程信息</summary>
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已发货的集成事件
    /// </summary>
    /// <param name="event">包含订单已发货状态信息的事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        // 获取所有订阅了"订单已发货"事件类型的webhook订阅
        var subscriptions = await retriever.GetSubscriptionsOfType(WebhookType.OrderShipped);

        // 记录收到的事件和找到的订阅数量
        logger.LogInformation("已接收 OrderStatusChangedToShippedIntegrationEvent 并已获取 {SubscriptionCount}个订阅进行处理", subscriptions.Count());

        // 创建webhook数据对象，包含事件类型和事件数据
        var whook = new WebhookData(WebhookType.OrderShipped, @event);

        // 向所有订阅者发送webhook通知
        await sender.SendAll(subscriptions, whook);
    }
}
