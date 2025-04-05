namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态已更改为"等待验证"的集成事件
/// </summary>
/// <remarks>
/// 此事件在订单状态变为"等待验证"(AwaitingValidation)时触发。
/// 它包含订单ID和需要验证库存的商品列表信息。
/// 通常此事件会由订单服务发出，并由目录服务接收以验证商品库存是否充足。
/// </remarks>
/// <param name="OrderId">需要验证库存的订单ID</param>
/// <param name="OrderStockItems">需要验证库存的商品项列表，包含商品ID和数量</param>
public record OrderStatusChangedToAwaitingValidationIntegrationEvent(
    int OrderId,
    IEnumerable<OrderStockItem> OrderStockItems) : IntegrationEvent;
