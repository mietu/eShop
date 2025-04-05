namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 产品价格变更集成事件
/// 当产品价格发生变化时触发，用于通知其他微服务或外部系统
/// </summary>
/// <remarks>
/// 集成事件说明: 
/// 事件是"过去已经发生的事情"，因此其名称必须使用过去时态
/// 集成事件是可能对其他微服务、有界上下文或外部系统产生副作用的事件
/// </remarks>
public record ProductPriceChangedIntegrationEvent(int ProductId, decimal NewPrice, decimal OldPrice) : IntegrationEvent;
