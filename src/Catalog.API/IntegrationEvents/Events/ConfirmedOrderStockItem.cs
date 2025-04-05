namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示已确认订单库存项的记录类型
/// </summary>
/// <param name="ProductId">产品ID</param>
/// <param name="HasStock">是否有库存</param>
public record ConfirmedOrderStockItem(int ProductId, bool HasStock);
