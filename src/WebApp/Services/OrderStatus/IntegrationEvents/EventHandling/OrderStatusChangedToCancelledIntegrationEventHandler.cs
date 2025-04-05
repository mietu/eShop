using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为已取消的集成事件的处理程序
/// </summary>
/// <remarks>
/// 当订单被取消时，此处理程序会通知相关买家订单状态的变更
/// </remarks>
public class OrderStatusChangedToCancelledIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToCancelledIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已取消的集成事件
    /// </summary>
    /// <param name="event">包含订单取消信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToCancelledIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
