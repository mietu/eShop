namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 订单状态变更为"等待验证"的领域事件
/// 当订单的宽限期确认后，系统会发布此事件
/// 通知相关处理程序执行后续业务逻辑，如库存检查、支付验证等
/// </summary>
public class OrderStatusChangedToAwaitingValidationDomainEvent
        : INotification
{
    /// <summary>
    /// 获取订单ID
    /// 用于标识哪个订单的状态发生了变更
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单项集合
    /// 包含订单中所有商品的详细信息，用于后续的库存验证
    /// </summary>
    public IEnumerable<OrderItem> OrderItems { get; }

    /// <summary>
    /// 初始化订单状态变更为"等待验证"的领域事件实例
    /// </summary>
    /// <param name="orderId">变更状态的订单ID</param>
    /// <param name="orderItems">订单中包含的商品项集合</param>
    public OrderStatusChangedToAwaitingValidationDomainEvent(int orderId,
        IEnumerable<OrderItem> orderItems)
    {
        OrderId = orderId;
        OrderItems = orderItems;
    }
}
