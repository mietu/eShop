namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 领域事件：订单状态变更为库存已确认
/// </summary>
public class OrderStatusChangedToStockConfirmedDomainEvent
    : INotification
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化<see cref="OrderStatusChangedToStockConfirmedDomainEvent"/>类的新实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    public OrderStatusChangedToStockConfirmedDomainEvent(int orderId)
        => OrderId = orderId;
}
