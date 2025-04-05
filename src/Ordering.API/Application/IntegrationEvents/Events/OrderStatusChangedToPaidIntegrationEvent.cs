namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 订单状态变更为已支付的集成事件
/// </summary>
public record OrderStatusChangedToPaidIntegrationEvent : IntegrationEvent
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
    /// 获取订单库存项目集合
    /// </summary>
    public IEnumerable<OrderStockItem> OrderStockItems { get; }

    /// <summary>
    /// 初始化订单状态变更为已支付的集成事件实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    /// <param name="orderStockItems">订单库存项目集合</param>
    public OrderStatusChangedToPaidIntegrationEvent(int orderId,
        OrderStatus orderStatus, string buyerName, string buyerIdentityGuid,
        IEnumerable<OrderStockItem> orderStockItems)
    {
        OrderId = orderId;
        OrderStockItems = orderStockItems;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}

