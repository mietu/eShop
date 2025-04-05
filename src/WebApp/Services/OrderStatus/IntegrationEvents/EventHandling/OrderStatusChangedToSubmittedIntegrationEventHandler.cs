using eShop.EventBus.Abstractions;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 处理订单状态变更为已提交的集成事件的处理程序
/// </summary>
/// <param name="orderStatusNotificationService">订单状态通知服务，用于通知买家订单状态变更</param>
/// <param name="logger">日志记录器</param>
public class OrderStatusChangedToSubmittedIntegrationEventHandler(
    OrderStatusNotificationService orderStatusNotificationService,
    ILogger<OrderStatusChangedToSubmittedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStatusChangedToSubmittedIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为已提交的集成事件
    /// </summary>
    /// <param name="event">包含订单状态变更信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToSubmittedIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);
        await orderStatusNotificationService.NotifyOrderStatusChangedAsync(@event.BuyerIdentityGuid);
    }
}
