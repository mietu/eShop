namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 表示买家和支付方式已验证的领域事件
/// 当买家的支付方式成功验证后触发此事件
/// </summary>
public class BuyerAndPaymentMethodVerifiedDomainEvent
    : INotification
{
    /// <summary>
    /// 获取关联的买家信息
    /// </summary>
    public Buyer Buyer { get; private set; }

    /// <summary>
    /// 获取已验证的支付方式
    /// </summary>
    public PaymentMethod Payment { get; private set; }

    /// <summary>
    /// 获取关联的订单ID
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// 创建买家和支付方式验证事件的新实例
    /// </summary>
    /// <param name="buyer">已验证的买家</param>
    /// <param name="payment">已验证的支付方式</param>
    /// <param name="orderId">关联的订单ID</param>
    public BuyerAndPaymentMethodVerifiedDomainEvent(Buyer buyer, PaymentMethod payment, int orderId)
    {
        Buyer = buyer;
        Payment = payment;
        OrderId = orderId;
    }
}
