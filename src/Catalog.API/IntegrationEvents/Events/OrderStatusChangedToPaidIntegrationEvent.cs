namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态变更为已支付的集成事件
/// 当订单完成支付后，此事件被发布到事件总线，以便其他微服务（如目录服务）可以执行相关操作
/// 例如：更新产品库存数量
/// </summary>
/// <param name="OrderId">已支付订单的ID</param>
/// <param name="OrderStockItems">订单中包含的商品项目集合，每项包含产品ID和数量</param>
public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
