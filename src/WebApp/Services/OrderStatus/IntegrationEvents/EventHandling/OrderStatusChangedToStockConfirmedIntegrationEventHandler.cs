using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为库存确认的集成事件处理程序
/// </summary>
/// <remarks>
/// 该处理程序接收到库存确认事件后，通知相关买家订单状态已更新，
/// 以便买家可以实时了解他们的订单处理进度。
/// </remarks>
public class OrderStatusChangedToStockConfirmedIntegrationEventHandler(
    /// <summary>订单状态通知服务，用于通知买家订单状态变更</summary>
    OrderStatusNotificationService orderStatusNotificationService,
    /// <summary>日志记录器</summary>
    ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为库存确认的集成事件
    /// </summary>
    /// <param name="event">包含订单信息的库存确认集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
