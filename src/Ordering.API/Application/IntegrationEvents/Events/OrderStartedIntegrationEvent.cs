namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

// Integration Events notes:
// An Event is "something that has happened in the past", therefore its name has to be
// An Integration Event is an event that can cause side effects to other microservices, Bounded-Contexts or external systems.

/// <summary>
/// 表示订单已开始的集成事件，用于通知其他微服务或系统用户已开始下单
/// </summary>
public record OrderStartedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取或设置下单用户的唯一标识
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// 初始化订单开始集成事件的新实例
    /// </summary>
    /// <param name="userId">下单用户的唯一标识</param>
    public OrderStartedIntegrationEvent(string userId)
        => UserId = userId;
}
