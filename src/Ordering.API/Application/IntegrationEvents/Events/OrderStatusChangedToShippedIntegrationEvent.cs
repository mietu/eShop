namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态已更改为已发货的集成事件。
/// 当订单被标记为已发货时，此事件将被发布以通知其他服务。
/// </summary>
public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单的唯一标识符
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单的当前状态
    /// </summary>
    public OrderStatus OrderStatus { get; }

    /// <summary>
    /// 获取买家的名称
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 获取买家的身份标识符
    /// </summary>
    public string BuyerIdentityGuid { get; }

    /// <summary>
    /// 初始化 OrderStatusChangedToShippedIntegrationEvent 的新实例
    /// </summary>
    /// <param name="orderId">订单标识符</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家名称</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    public OrderStatusChangedToShippedIntegrationEvent(
        int orderId, OrderStatus orderStatus, string buyerName, string buyerIdentityGuid)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}
