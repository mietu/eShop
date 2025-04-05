namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 订单支付失败集成事件处理程序
/// 负责在支付失败时取消相关订单
/// </summary>
public class OrderPaymentFailedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderPaymentFailedIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
{
    /// <summary>
    /// 处理订单支付失败集成事件
    /// </summary>
    /// <param name="event">包含支付失败订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderPaymentFailedIntegrationEvent @event)
    {
        // 记录收到的集成事件信息
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 创建取消订单命令，使用事件中的订单ID
        var command = new CancelOrderCommand(@event.OrderId);

        // 记录即将发送的命令信息
        logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        // 通过中介者模式发送取消订单命令
        await mediator.Send(command);
    }
}
