namespace eShop.Catalog.API.IntegrationEvents;

/// <summary>
/// 目录集成事件服务，负责管理事件的持久化和发布
/// 实现了ICatalogIntegrationEventService接口，提供事件保存和发布功能
/// 同时实现IDisposable接口，确保资源正确释放
/// </summary>
public sealed class CatalogIntegrationEventService(ILogger<CatalogIntegrationEventService> logger,
    IEventBus eventBus,
    CatalogContext catalogContext,
    IIntegrationEventLogService integrationEventLogService)
    : ICatalogIntegrationEventService, IDisposable
{
    /// <summary>
    /// 标记资源是否已释放的标志，使用volatile确保多线程访问的可见性
    /// </summary>
    private volatile bool disposedValue;

    /// <summary>
    /// 通过事件总线发布集成事件
    /// 包含事件发布的完整流程：标记进行中 -> 发布 -> 标记已发布/失败
    /// </summary>
    /// <param name="evt">待发布的集成事件</param>
    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            logger.LogInformation("发布集成事件: {IntegrationEventId_published} - ({@IntegrationEvent})", evt.Id, evt);

            // 标记事件为进行中状态
            await integrationEventLogService.MarkEventAsInProgressAsync(evt.Id);
            // 通过事件总线发布事件
            await eventBus.PublishAsync(evt);
            // 发布成功后标记事件为已发布状态
            await integrationEventLogService.MarkEventAsPublishedAsync(evt.Id);
        }
        catch (Exception ex)
        {
            // 发布失败时记录错误并标记事件为失败状态
            logger.LogError(ex, "发布集成事件时出错: {IntegrationEventId} - ({@IntegrationEvent})", evt.Id, evt);
            await integrationEventLogService.MarkEventAsFailedAsync(evt.Id);
        }
    }

    /// <summary>
    /// 保存事件并提交目录上下文的更改
    /// 使用本地事务确保目录数据库操作与集成事件日志之间的原子性
    /// </summary>
    /// <param name="evt">待保存的集成事件</param>
    public async Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt)
    {
        logger.LogInformation("CatalogIntegrationEventService - 保存更改和集成事件: {IntegrationEventId}", evt.Id);

        //使用EF Core弹性策略，在显式BeginTransaction()中使用多个DbContext:
        //参见: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
        await ResilientTransaction.New(catalogContext).ExecuteAsync(async () =>
        {
            // 通过本地事务实现原始目录数据库操作和IntegrationEventLog之间的原子性
            await catalogContext.SaveChangesAsync();
            await integrationEventLogService.SaveEventAsync(evt, catalogContext.Database.CurrentTransaction);
        });
    }

    /// <summary>
    /// 释放资源的内部方法
    /// </summary>
    /// <param name="disposing">是否正在主动释放资源</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 如果integrationEventLogService实现了IDisposable接口，则释放它
                (integrationEventLogService as IDisposable)?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// 实现IDisposable接口的Dispose方法
    /// 释放资源并禁止终结器调用
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
