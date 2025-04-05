namespace eShop.Catalog.API.IntegrationEvents;

/// <summary>
/// 定义目录服务集成事件处理的接口
/// 负责保存事件并发布到事件总线
/// </summary>
public interface ICatalogIntegrationEventService
{
    /// <summary>
    /// 保存集成事件并提交目录上下文的更改
    /// </summary>
    /// <param name="evt">要保存的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    Task SaveEventAndCatalogContextChangesAsync(IntegrationEvent evt);

    /// <summary>
    /// 通过事件总线发布集成事件
    /// </summary>
    /// <param name="evt">要发布的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    Task PublishThroughEventBusAsync(IntegrationEvent evt);
}
