namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;
/// <summary>
/// 处理订单库存拒绝集成事件的处理程序
/// 当库存微服务确认某些商品没有足够库存时，此处理程序被触发
/// </summary>
public class OrderStockRejectedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderStockRejectedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStockRejectedIntegrationEvent>
{
    /// <summary>
    /// 处理订单库存拒绝事件
    /// </summary>
    /// <param name="event">包含被拒绝订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStockRejectedIntegrationEvent @event)
    {
        // 记录收到的集成事件
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 从事件中筛选出没有库存的商品ID列表
        var orderStockRejectedItems = @event.OrderStockItems
            .FindAll(c => !c.HasStock)
            .Select(c => c.ProductId)
            .ToList();

        // 创建命令以将订单状态设置为库存拒绝
        var command = new SetStockRejectedOrderStatusCommand(@event.OrderId, orderStockRejectedItems);

        // 记录即将发送的命令
        logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        // 通过中介者发送命令
        await mediator.Send(command);
    }
}
