namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 表示订单状态变更为已发货的集成事件
/// </summary>
/// <param name="OrderId">订单ID</param>
/// <param name="OrderStatus">订单状态</param>
/// <param name="BuyerName">买家名称</param>
public record OrderStatusChangedToShippedIntegrationEvent(int OrderId, string OrderStatus, string BuyerName) : IntegrationEvent;
