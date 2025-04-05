namespace eShop.Ordering.API.Application.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单宽限期确认集成事件的处理程序
/// </summary>
public class GracePeriodConfirmedIntegrationEventHandler(
    IMediator mediator,
    ILogger<GracePeriodConfirmedIntegrationEventHandler> logger) : IIntegrationEventHandler<GracePeriodConfirmedIntegrationEvent>
{
    /// <summary>
    /// 事件处理方法，确认订单宽限期已完成，订单不会被初始取消。
    /// 因此，订单流程继续进行验证。
    /// </summary>
    /// <param name="event">包含已确认宽限期的订单信息的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(GracePeriodConfirmedIntegrationEvent @event)
    {
        logger.LogInformation("正在处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        var command = new SetAwaitingValidationOrderStatusCommand(@event.OrderId);

        logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.OrderNumber),
            command.OrderNumber,
            command);

        await mediator.Send(command);
    }
}
