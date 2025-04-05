namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单库存被拒绝的集成事件，当库存不足以满足订单时触发
/// </summary>
public record OrderStockRejectedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取被拒绝的订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单中的库存项目列表，包含每个产品的库存状态
    /// </summary>
    public List<ConfirmedOrderStockItem> OrderStockItems { get; }

    /// <summary>
    /// 初始化订单库存拒绝事件的新实例
    /// </summary>
    /// <param name="orderId">被拒绝的订单ID</param>
    /// <param name="orderStockItems">订单中的库存项目列表</param>
    public OrderStockRejectedIntegrationEvent(int orderId,
        List<ConfirmedOrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
    }
}

/// <summary>
/// 表示订单中已确认的库存项目及其状态
/// </summary>
public record ConfirmedOrderStockItem
{
    /// <summary>
    /// 获取产品ID
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// 获取产品是否有足够库存的标志
    /// </summary>
    public bool HasStock { get; }

    /// <summary>
    /// 初始化已确认订单库存项目的新实例
    /// </summary>
    /// <param name="productId">产品ID</param>
    /// <param name="hasStock">指示是否有足够库存的标志</param>
    public ConfirmedOrderStockItem(int productId, bool hasStock)
    {
        ProductId = productId;
        HasStock = hasStock;
    }
}
