namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 表示订单被取消时发布的领域事件
/// 当订单被取消时，该事件会被发布并由相关处理程序处理
/// </summary>
public class OrderCancelledDomainEvent : INotification
{
    /// <summary>
    /// 获取被取消的订单
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// 初始化<see cref="OrderCancelledDomainEvent"/>类的新实例
    /// </summary>
    /// <param name="order">被取消的订单</param>
    public OrderCancelledDomainEvent(Order order)
    {
        Order = order;
    }
}

