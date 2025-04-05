using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// 表示买家的支付方式实体
/// 作为买家聚合的一部分，记录支付卡的相关信息
/// </summary>
public class PaymentMethod : Entity
{
    /// <summary>
    /// 支付方式别名
    /// </summary>
    [Required]
    private string _alias;

    /// <summary>
    /// 卡号
    /// </summary>
    [Required]
    private string _cardNumber;

    /// <summary>
    /// 安全码
    /// </summary>
    private string _securityNumber;

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    [Required]
    private string _cardHolderName;

    /// <summary>
    /// 卡片过期日期
    /// </summary>
    private DateTime _expiration;

    /// <summary>
    /// 卡类型ID
    /// </summary>
    private int _cardTypeId;

    /// <summary>
    /// 卡类型信息
    /// </summary>
    public CardType CardType { get; private set; }

    /// <summary>
    /// 受保护的无参构造函数，供EF Core使用
    /// </summary>
    protected PaymentMethod() { }

    /// <summary>
    /// 创建新的支付方式
    /// </summary>
    /// <param name="cardTypeId">卡类型ID</param>
    /// <param name="alias">支付方式别名</param>
    /// <param name="cardNumber">卡号</param>
    /// <param name="securityNumber">安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="expiration">过期日期</param>
    /// <exception cref="OrderingDomainException">当参数无效时抛出异常</exception>
    public PaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber, string cardHolderName, DateTime expiration)
    {
        _cardNumber = !string.IsNullOrWhiteSpace(cardNumber) ? cardNumber : throw new OrderingDomainException(nameof(cardNumber));
        _securityNumber = !string.IsNullOrWhiteSpace(securityNumber) ? securityNumber : throw new OrderingDomainException(nameof(securityNumber));
        _cardHolderName = !string.IsNullOrWhiteSpace(cardHolderName) ? cardHolderName : throw new OrderingDomainException(nameof(cardHolderName));

        if (expiration < DateTime.UtcNow)
        {
            throw new OrderingDomainException(nameof(expiration));
        }

        _alias = alias;
        _expiration = expiration;
        _cardTypeId = cardTypeId;
    }

    /// <summary>
    /// 判断当前支付方式是否与提供的参数相匹配
    /// </summary>
    /// <param name="cardTypeId">卡类型ID</param>
    /// <param name="cardNumber">卡号</param>
    /// <param name="expiration">过期日期</param>
    /// <returns>如果参数与当前支付方式匹配，则返回true；否则返回false</returns>
    public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
    {
        return _cardTypeId == cardTypeId
            && _cardNumber == cardNumber
            && _expiration == expiration;
    }
}
