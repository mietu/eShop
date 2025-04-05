namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 表示订单已发货的领域事件
/// 当订单状态变更为"已发货"时触发此事件
/// 用于通知系统其他部分处理与订单发货相关的后续操作
/// </summary>
public class OrderShippedDomainEvent : INotification
{
    /// <summary>
    /// 获取与此事件关联的订单实体
    /// 包含了订单的完整信息，如订单项、配送地址等
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// 创建订单已发货领域事件的实例
    /// </summary>
    /// <param name="order">已发货的订单实体，不能为null</param>
    public OrderShippedDomainEvent(Order order)
    {
        Order = order;
    }
}
