namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 订单支付成功集成事件
/// 当订单支付成功时发布此事件，以便其他服务可以做出相应的处理
/// </summary>
public record OrderPaymentSucceededIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化订单支付成功集成事件
    /// </summary>
    /// <param name="orderId">已完成支付的订单ID</param>
    public OrderPaymentSucceededIntegrationEvent(int orderId) => OrderId = orderId;
}
