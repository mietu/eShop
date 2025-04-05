namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单支付失败的集成事件
/// </summary>
public record OrderPaymentFailedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单标识符
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化订单支付失败的集成事件实例
    /// </summary>
    /// <param name="orderId">支付失败的订单标识符</param>
    public OrderPaymentFailedIntegrationEvent(int orderId) => OrderId = orderId;
}
