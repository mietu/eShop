using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变为"等待验证"的集成事件的处理程序
/// 当订单状态变为等待验证时，通知相关买家
/// </summary>
public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,  // 用于通知订单状态变更的服务
    ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger)  // 用于记录日志的服务
    : IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
{
    /// <summary>
    /// 处理订单等待验证事件
    /// </summary>
    /// <param name="event">包含订单和买家信息的事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
