namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态变更为"等待验证"的集成事件
/// </summary>
public record OrderStatusChangedToAwaitingValidationIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单状态
    /// </summary>
    public OrderStatus OrderStatus { get; }

    /// <summary>
    /// 获取买家姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 获取买家身份标识符
    /// </summary>
    public string BuyerIdentityGuid { get; }

    /// <summary>
    /// 获取订单库存项集合
    /// </summary>
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    /// <summary>
    /// 初始化<see cref="OrderStatusChangedToAwaitingValidationIntegrationEvent"/>的新实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    /// <param name="orderStockItems">订单库存项集合</param>
    public OrderStatusChangedToAwaitingValidationIntegrationEvent(
        int orderId, OrderStatus orderStatus, string buyerName, string buyerIdentityGuid,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}

/// <summary>
/// 表示订单中的单个库存项
/// </summary>
public record OrderStockItem
{
    /// <summary>
    /// 获取产品ID
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// 获取产品数量
    /// </summary>
    public int Units { get; }

    /// <summary>
    /// 初始化<see cref="OrderStockItem"/>的新实例
    /// </summary>
    /// <param name="productId">产品ID</param>
    /// <param name="units">产品数量</param>
    public OrderStockItem(int productId, int units)
    {
        ProductId = productId;
        Units = units;
    }
}
