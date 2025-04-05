namespace eShop.Catalog.API.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单状态变为"等待验证"的集成事件的处理程序
/// 负责验证订单中的商品是否有足够库存，并发布相应的确认或拒绝事件
/// </summary>
public class OrderStatusChangedToAwaitingValidationIntegrationEventHandler(
    CatalogContext catalogContext,                                           // 用于访问商品数据的数据库上下文
    ICatalogIntegrationEventService catalogIntegrationEventService,          // 用于保存和发布集成事件的服务
    ILogger<OrderStatusChangedToAwaitingValidationIntegrationEventHandler> logger) : // 用于记录日志的服务
    IIntegrationEventHandler<OrderStatusChangedToAwaitingValidationIntegrationEvent>
{
    /// <summary>
    /// 处理订单等待验证事件
    /// </summary>
    /// <param name="event">包含需要验证库存的订单信息的事件</param>
    public async Task Handle(OrderStatusChangedToAwaitingValidationIntegrationEvent @event)
    {
        // 记录正在处理的集成事件信息
        logger.LogInformation("处理集成事件: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id, @event);

        // 创建一个列表，用于存储每个订单商品的库存确认结果
        var confirmedOrderStockItems = new List<ConfirmedOrderStockItem>();

        // 遍历订单中的每个商品
        foreach (var orderStockItem in @event.OrderStockItems)
        {
            // 从数据库中查找商品信息
            var catalogItem = catalogContext.CatalogItems.Find(orderStockItem.ProductId);

            // 检查商品是否有足够的库存满足订单需求
            var hasStock = catalogItem.AvailableStock >= orderStockItem.Units;

            // 创建库存确认项，记录商品ID和是否有足够库存
            var confirmedOrderStockItem = new ConfirmedOrderStockItem(catalogItem.Id, hasStock);

            // 将确认结果添加到列表
            confirmedOrderStockItems.Add(confirmedOrderStockItem);
        }

        // 根据所有商品的库存状况，决定创建哪种类型的集成事件
        // 如果任何一个商品库存不足，创建订单库存拒绝事件
        // 否则，创建订单库存确认事件
        var confirmedIntegrationEvent = confirmedOrderStockItems.Any(c => !c.HasStock)
            ? (IntegrationEvent)new OrderStockRejectedIntegrationEvent(@event.OrderId, confirmedOrderStockItems)
            : new OrderStockConfirmedIntegrationEvent(@event.OrderId);

        // 保存集成事件到数据库
        await catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(confirmedIntegrationEvent);

        // 通过事件总线发布集成事件
        await catalogIntegrationEventService.PublishThroughEventBusAsync(confirmedIntegrationEvent);
    }
}
