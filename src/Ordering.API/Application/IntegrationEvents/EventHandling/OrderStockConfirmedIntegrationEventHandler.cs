namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 订单库存确认集成事件处理程序
/// 负责处理订单库存已确认的集成事件，并将相应订单状态更新为库存已确认
/// </summary>
public class OrderStockConfirmedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderStockConfirmedIntegrationEventHandler> logger) :
    IIntegrationEventHandler<OrderStockConfirmedIntegrationEvent>
{
    /// <summary>
    /// 处理订单库存确认集成事件
    /// </summary>
    /// <param name="event">包含已确认库存订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStockConfirmedIntegrationEvent @event)
    {
        // 记录正在处理的集成事件信息
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 创建设置订单状态为库存已确认的命令
        var command = new SetStockConfirmedOrderStatusCommand(@event.OrderId);

        // 记录即将发送的命令信息
        logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        // 通过中介者模式发送命令，更新订单状态
        await mediator.Send(command);
    }
}
