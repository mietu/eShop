using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为已支付的集成事件处理器
/// 当订单状态变为已支付时，通知相关订阅者状态变更
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler(
    /// <summary>订单状态通知服务，用于通知订阅者订单状态变更</summary>
    OrderStatusNotificationService orderStatusNotificationService,
    /// <summary>日志记录器，用于记录处理过程</summary>
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已支付的集成事件
    /// </summary>
    /// <param name="event">包含订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
