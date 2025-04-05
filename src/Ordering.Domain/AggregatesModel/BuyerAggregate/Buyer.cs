using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// 表示订购系统中的买家聚合根
/// 负责管理买家身份和支付方式信息
/// </summary>
public class Buyer
    : Entity, IAggregateRoot
{
    /// <summary>
    /// 买家的唯一标识符
    /// </summary>
    [Required]
    public string IdentityGuid { get; private set; }

    /// <summary>
    /// 买家姓名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 买家的支付方式集合
    /// </summary>
    private List<PaymentMethod> _paymentMethods;

    /// <summary>
    /// 提供对支付方式的只读访问
    /// </summary>
    public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    /// <summary>
    /// 受保护的构造函数，用于EF Core
    /// </summary>
    protected Buyer()
    {
        _paymentMethods = new List<PaymentMethod>();
    }

    /// <summary>
    /// 创建买家实例
    /// </summary>
    /// <param name="identity">买家的身份标识</param>
    /// <param name="name">买家姓名</param>
    /// <exception cref="ArgumentNullException">当身份标识或姓名为空时抛出</exception>
    public Buyer(string identity, string name) : this()
    {
        IdentityGuid = !string.IsNullOrWhiteSpace(identity) ? identity : throw new ArgumentNullException(nameof(identity));
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// 验证或添加支付方式
    /// 如果支付方式已存在，则验证并返回现有支付方式
    /// 如果支付方式不存在，则创建并添加新的支付方式
    /// </summary>
    /// <param name="cardTypeId">卡类型ID</param>
    /// <param name="alias">支付方式别名</param>
    /// <param name="cardNumber">卡号</param>
    /// <param name="securityNumber">安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="expiration">过期日期</param>
    /// <param name="orderId">关联的订单ID</param>
    /// <returns>验证后的或新添加的支付方式</returns>
    public PaymentMethod VerifyOrAddPaymentMethod(
        int cardTypeId, string alias, string cardNumber,
        string securityNumber, string cardHolderName, DateTime expiration, int orderId)
    {
        var existingPayment = _paymentMethods
            .SingleOrDefault(p => p.IsEqualTo(cardTypeId, cardNumber, expiration));

        if (existingPayment != null)
        {
            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));

            return existingPayment;
        }

        var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

        _paymentMethods.Add(payment);

        AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

        return payment;
    }
}
