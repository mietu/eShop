using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为"已发货"的集成事件处理程序。
/// 当订单状态更改为已发货时，此处理程序负责通知相关买家。
/// </summary>
public class OrderStatusChangedToShippedIntegrationEventHandler(
    /// <summary>订单状态通知服务，用于通知买家订单状态变更</summary>
    OrderStatusNotificationService orderStatusNotificationService,
    /// <summary>日志记录器，用于记录处理事件的信息</summary>
    ILogger<OrderStatusChangedToShippedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToShippedIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已发货的集成事件
    /// </summary>
    /// <param name="event">包含订单已发货状态信息的事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToShippedIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
