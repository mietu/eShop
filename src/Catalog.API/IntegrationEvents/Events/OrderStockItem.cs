namespace eShop.Catalog.API.IntegrationEvents.Events;

/// <summary>
/// 表示订单中的库存项目信息
/// </summary>
/// <param name="ProductId">产品ID</param>
/// <param name="Units">订购的数量</param>
public record OrderStockItem(int ProductId, int Units);
