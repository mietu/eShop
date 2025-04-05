namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 表示订单中的库存项目，用于库存相关的集成事件。
/// </summary>
/// <param name="ProductId">产品的唯一标识符</param>
/// <param name="Units">订购的产品数量</param>
public record OrderStockItem(int ProductId, int Units);
