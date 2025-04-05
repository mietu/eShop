namespace eShop.IntegrationEventLogEF.Services;

/// <summary>
/// 提供集成事件日志操作的服务接口，用于管理事件的持久化和状态跟踪
/// </summary>
public interface IIntegrationEventLogService
{
    /// <summary>
    /// 异步检索在指定事务中待发布的所有集成事件日志
    /// </summary>
    /// <param name="transactionId">要检索事件的事务ID</param>
    /// <returns>待发布的集成事件日志集合</returns>
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);

    /// <summary>
    /// 异步保存集成事件到事件日志中
    /// </summary>
    /// <param name="event">要保存的集成事件</param>
    /// <param name="transaction">事件关联的数据库事务</param>
    /// <returns>表示异步操作的任务</returns>
    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);

    /// <summary>
    /// 将指定的事件标记为已发布状态
    /// </summary>
    /// <param name="eventId">要标记的事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    Task MarkEventAsPublishedAsync(Guid eventId);

    /// <summary>
    /// 将指定的事件标记为处理中状态
    /// </summary>
    /// <param name="eventId">要标记的事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    Task MarkEventAsInProgressAsync(Guid eventId);

    /// <summary>
    /// 将指定的事件标记为发布失败状态
    /// </summary>
    /// <param name="eventId">要标记的事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    Task MarkEventAsFailedAsync(Guid eventId);
}
