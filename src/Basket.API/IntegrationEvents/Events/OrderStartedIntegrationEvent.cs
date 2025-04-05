namespace eShop.Basket.API.IntegrationEvents.EventHandling.Events;

/// <summary>
/// 订单开始集成事件
/// 当用户开始创建订单时触发，用于通知其他微服务或系统
/// </summary>
/// <remarks>
/// 集成事件注意事项:
/// - 事件表示"过去已经发生的事情"，因此其命名应该是过去时态
/// - 集成事件是可能对其他微服务、限界上下文或外部系统产生副作用的事件
/// </remarks>
public record OrderStartedIntegrationEvent(string UserId) : IntegrationEvent;
