namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示订单库存已被拒绝的集成事件
/// </summary>
/// <param name="OrderId">被拒绝库存的订单ID</param>
/// <param name="OrderStockItems">订单中的库存项目列表及其库存状态</param>
public record OrderStockRejectedIntegrationEvent(int OrderId, List<ConfirmedOrderStockItem> OrderStockItems) : IntegrationEvent;
