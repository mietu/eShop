namespace eShop.IntegrationEventLogEF.Services;

/// <summary>
/// 集成事件日志服务，负责管理和记录集成事件的状态和生命周期
/// </summary>
/// <typeparam name="TContext">数据库上下文类型</typeparam>
public class IntegrationEventLogService<TContext> : IIntegrationEventLogService, IDisposable
    where TContext : DbContext
{
    private volatile bool _disposedValue;
    private readonly TContext _context;
    private readonly Type[] _eventTypes;

    /// <summary>
    /// 初始化集成事件日志服务的新实例
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public IntegrationEventLogService(TContext context)
    {
        _context = context;
        // 加载当前程序集中所有以"IntegrationEvent"结尾的类型
        _eventTypes = Assembly.Load(Assembly.GetEntryAssembly().FullName)
            .GetTypes()
            .Where(t => t.Name.EndsWith(nameof(IntegrationEvent)))
            .ToArray();
    }

    /// <summary>
    /// 检索指定事务ID下所有待发布的集成事件日志
    /// </summary>
    /// <param name="transactionId">事务ID</param>
    /// <returns>待发布的集成事件日志集合</returns>
    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var result = await _context.Set<IntegrationEventLogEntry>()
            .Where(e => e.TransactionId == transactionId && e.State == EventStateEnum.NotPublished)
            .ToListAsync();

        if (result.Count != 0)
        {
            return result.OrderBy(o => o.CreationTime)
                .Select(e => e.DeserializeJsonContent(_eventTypes.FirstOrDefault(t => t.Name == e.EventTypeShortName)));
        }

        return [];
    }

    /// <summary>
    /// 保存集成事件到数据库
    /// </summary>
    /// <param name="event">要保存的集成事件</param>
    /// <param name="transaction">数据库事务</param>
    /// <returns>表示异步操作的任务</returns>
    /// <exception cref="ArgumentNullException">当事务为空时抛出</exception>
    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));

        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId);

        _context.Database.UseTransaction(transaction.GetDbTransaction());
        _context.Set<IntegrationEventLogEntry>().Add(eventLogEntry);

        return _context.SaveChangesAsync();
    }

    /// <summary>
    /// 将事件标记为已发布状态
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    /// <summary>
    /// 将事件标记为处理中状态
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    /// <summary>
    /// 将事件标记为发布失败状态
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <returns>表示异步操作的任务</returns>
    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    /// <summary>
    /// 更新事件的状态
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <param name="status">新的事件状态</param>
    /// <returns>表示异步操作的任务</returns>
    private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
        eventLogEntry.State = status;

        // 当事件状态变为处理中时，增加发送次数计数
        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        return _context.SaveChangesAsync();
    }

    /// <summary>
    /// 释放资源的受保护方法
    /// </summary>
    /// <param name="disposing">表示是否正在主动释放资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
