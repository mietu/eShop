namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 订单支付成功集成事件处理程序
/// 负责接收订单支付成功的集成事件并更新订单状态为已支付
/// </summary>
public class OrderPaymentSucceededIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderPaymentSucceededIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderPaymentSucceededIntegrationEvent>
{
    /// <summary>
    /// 处理订单支付成功的集成事件
    /// </summary>
    /// <param name="event">订单支付成功事件数据</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderPaymentSucceededIntegrationEvent @event)
    {
        // 记录接收到的集成事件信息
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 创建设置订单为已支付状态的命令
        var command = new SetPaidOrderStatusCommand(@event.OrderId);

        // 记录发送命令的信息
        logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        // 通过中介者模式发送命令到对应的处理程序
        await mediator.Send(command);
    }
}
