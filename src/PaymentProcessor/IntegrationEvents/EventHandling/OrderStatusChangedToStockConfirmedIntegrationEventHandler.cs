namespace eShop.PaymentProcessor.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变更为库存确认的集成事件处理程序
/// </summary>
/// <remarks>
/// 此处理程序接收订单库存确认事件并模拟支付处理流程。
/// 根据配置的支付结果，它将发布支付成功或失败的相应集成事件。
/// </remarks>
public class OrderStatusChangedToStockConfirmedIntegrationEventHandler(
    /// <summary>事件总线，用于发布支付结果事件</summary>
    IEventBus eventBus,
    /// <summary>支付选项配置，用于决定支付是否成功</summary>
    IOptionsMonitor<PaymentOptions> options,
    /// <summary>日志记录器</summary>
    ILogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStatusChangedToStockConfirmedIntegrationEvent>
{
    /// <summary>
    /// 处理订单状态变更为库存确认的集成事件
    /// </summary>
    /// <param name="event">包含订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToStockConfirmedIntegrationEvent @event)
    {
        logger.LogInformation("处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        IntegrationEvent orderPaymentIntegrationEvent;

        // Business feature comment:
        // When OrderStatusChangedToStockConfirmed Integration Event is handled.
        // Here we're simulating that we'd be performing the payment against any payment gateway
        // Instead of a real payment we just take the env. var to simulate the payment 
        // The payment can be successful or it can fail

        if (options.CurrentValue.PaymentSucceeded)
        {
            orderPaymentIntegrationEvent = new OrderPaymentSucceededIntegrationEvent(@event.OrderId);
        }
        else
        {
            orderPaymentIntegrationEvent = new OrderPaymentFailedIntegrationEvent(@event.OrderId);
        }

        logger.LogInformation("发布集成事件: {IntegrationEventId} - ({@IntegrationEvent})", orderPaymentIntegrationEvent.Id, orderPaymentIntegrationEvent);

        await eventBus.PublishAsync(orderPaymentIntegrationEvent);
    }
}
