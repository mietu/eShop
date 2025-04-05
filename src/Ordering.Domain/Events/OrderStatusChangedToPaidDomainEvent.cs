namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 表示订单状态变更为已支付的领域事件。
/// 此事件在订单完成支付后触发，包含订单ID和订单项集合信息。
/// 作为领域事件，它实现了INotification接口，用于领域事件通知。
/// </summary>
public class OrderStatusChangedToPaidDomainEvent
    : INotification
{
    /// <summary>
    /// 获取订单ID。
    /// 标识触发此事件的订单。
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单项集合。
    /// 包含订单中所有的商品项明细。
    /// </summary>
    public IEnumerable<OrderItem> OrderItems { get; }

    /// <summary>
    /// 创建订单状态变更为已支付的领域事件实例。
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderItems">订单项集合</param>
    public OrderStatusChangedToPaidDomainEvent(int orderId,
        IEnumerable<OrderItem> orderItems)
    {
        OrderId = orderId;
        OrderItems = orderItems;
    }
}
