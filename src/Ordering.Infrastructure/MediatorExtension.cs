namespace eShop.Ordering.Infrastructure;

/// <summary>
/// 提供用于MediatR的扩展方法，主要用于处理领域事件的分发
/// </summary>
static class MediatorExtension
{
    /// <summary>
    /// 分发所有待处理的领域事件
    /// </summary>
    /// <param name="mediator">中介者实例</param>
    /// <param name="ctx">订单数据上下文</param>
    /// <returns>一个表示异步操作的任务</returns>
    /// <remarks>
    /// 此方法会从EF Core的ChangeTracker中检索所有具有待处理领域事件的实体，
    /// 收集这些事件，清除实体上的原始事件以防止重复处理，
    /// 然后通过mediator发布所有收集到的事件。
    /// 通常在工作单元的SaveChanges操作中调用此方法，以确保在持久化更改后处理领域事件。
    /// </remarks>
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, OrderingContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}
