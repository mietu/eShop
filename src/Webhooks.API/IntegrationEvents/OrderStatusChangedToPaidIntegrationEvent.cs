namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 表示订单状态变更为已支付的集成事件
/// </summary>
/// <param name="OrderId">已支付订单的ID</param>
/// <param name="OrderStockItems">订单中包含的库存项集合</param>
public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
