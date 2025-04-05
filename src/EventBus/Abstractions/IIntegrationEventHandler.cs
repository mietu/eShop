namespace eShop.EventBus.Abstractions;

/// <summary>
/// 定义用于处理特定类型集成事件的处理程序接口
/// </summary>
/// <typeparam name="TIntegrationEvent">要处理的集成事件类型</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 处理特定类型的集成事件
    /// </summary>
    /// <param name="event">要处理的集成事件实例</param>
    /// <returns>表示异步操作的任务</returns>
    Task Handle(TIntegrationEvent @event);

    /// <summary>
    /// 实现非泛型接口方法，将基础事件类型转换为特定类型并调用泛型处理方法
    /// </summary>
    /// <param name="event">要处理的基础集成事件实例</param>
    /// <returns>表示异步操作的任务</returns>
    Task IIntegrationEventHandler.Handle(IntegrationEvent @event)
    {
        return Handle((TIntegrationEvent)@event);
    }
}

/// <summary>
/// 定义所有集成事件处理程序的基础接口
/// </summary>
public interface IIntegrationEventHandler
{
    /// <summary>
    /// 处理集成事件的基础方法
    /// </summary>
    /// <param name="event">要处理的集成事件实例</param>
    /// <returns>表示异步操作的任务</returns>
    Task Handle(IntegrationEvent @event);
}
