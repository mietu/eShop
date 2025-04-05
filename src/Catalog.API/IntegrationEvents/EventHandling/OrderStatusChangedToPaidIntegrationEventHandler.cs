namespace eShop.Catalog.API.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单已支付状态变更的集成事件处理器
/// 当订单状态变为已支付时，更新目录项的库存数量
/// </summary>
public class OrderStatusChangedToPaidIntegrationEventHandler(
    CatalogContext catalogContext,  // 目录数据上下文，用于访问商品数据
    ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger) :  // 日志记录器
    IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>  // 实现了特定集成事件的处理接口
{
    /// <summary>
    /// 处理订单状态变为已支付的集成事件
    /// </summary>
    /// <param name="event">包含订单和库存项信息的集成事件</param>
    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        // 记录正在处理的集成事件信息
        logger.LogInformation("处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 注意：此系统不阻止库存/清单（即不预留库存）
        foreach (var orderStockItem in @event.OrderStockItems)
        {
            // 查找与订单中的商品ID对应的目录项
            var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

            // 从库存中减去已售出的单位数量
            catalogItem.RemoveStock(orderStockItem.Units);
        }

        // 保存所有库存变更到数据库
        await catalogContext.SaveChangesAsync();
    }
}
