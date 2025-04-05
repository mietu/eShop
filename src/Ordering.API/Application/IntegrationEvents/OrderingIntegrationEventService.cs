namespace eShop.Ordering.API.Application.IntegrationEvents;

/// <summary>
/// 订单集成事件服务，负责订单相关集成事件的保存和发布
/// </summary>
/// <remarks>
/// 该服务处理事件的持久化和通过事件总线发布事件，同时确保事件状态的正确跟踪
/// </remarks>
public class OrderingIntegrationEventService(IEventBus eventBus,
    OrderingContext orderingContext,
    IIntegrationEventLogService integrationEventLogService,
    ILogger<OrderingIntegrationEventService> logger) : IOrderingIntegrationEventService
{
    /// <summary>
    /// 事件总线，用于向其他服务发布集成事件
    /// </summary>
    private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

    /// <summary>
    /// 订单数据上下文，提供与订单数据库的交互
    /// </summary>
    private readonly OrderingContext _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));

    /// <summary>
    /// 集成事件日志服务，用于事件的持久化和状态管理
    /// </summary>
    private readonly IIntegrationEventLogService _eventLogService = integrationEventLogService ?? throw new ArgumentNullException(nameof(integrationEventLogService));

    /// <summary>
    /// 日志记录器，用于记录服务操作日志
    /// </summary>
    private readonly ILogger<OrderingIntegrationEventService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// 通过事件总线发布指定事务中的所有待处理事件
    /// </summary>
    /// <param name="transactionId">包含要发布事件的事务ID</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        // 检索指定事务中所有待发布的事件
        var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        foreach (var logEvt in pendingLogEvents)
        {
            _logger.LogInformation("正在发布集成事件: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt.IntegrationEvent);

            try
            {
                // 将事件标记为处理中
                await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                // 通过事件总线发布事件
                await _eventBus.PublishAsync(logEvt.IntegrationEvent);
                // 事件发布成功后，将其标记为已发布
                await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                // 记录发布失败的错误信息
                _logger.LogError(ex, "发布集成事件时出错: {IntegrationEventId}", logEvt.EventId);
                // 将事件标记为发布失败
                await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    /// <summary>
    /// 添加并保存集成事件到事件日志
    /// </summary>
    /// <param name="evt">要保存的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task AddAndSaveEventAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("将集成事件 {IntegrationEventId} 加入存储库 ({@IntegrationEvent})", evt.Id, evt);

        // 将事件保存到事件日志，并关联到当前的数据库事务
        await _eventLogService.SaveEventAsync(evt, _orderingContext.GetCurrentTransaction());
    }
}
