namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态已更改为库存确认的集成事件
/// </summary>
public record OrderStatusChangedToStockConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单标识符
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单状态
    /// </summary>
    public OrderStatus OrderStatus { get; }

    /// <summary>
    /// 获取买家名称
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 获取买家身份标识符
    /// </summary>
    public string BuyerIdentityGuid { get; }

    /// <summary>
    /// 初始化订单状态已更改为库存确认的集成事件的新实例
    /// </summary>
    /// <param name="orderId">订单标识符</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家名称</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    public OrderStatusChangedToStockConfirmedIntegrationEvent(
        int orderId, OrderStatus orderStatus, string buyerName, string buyerIdentityGuid)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}
