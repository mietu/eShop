namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 当订单中的商品库存已确认时发布的集成事件
/// </summary>
/// <param name="OrderId">已确认库存的订单ID</param>
public record OrderStockConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
