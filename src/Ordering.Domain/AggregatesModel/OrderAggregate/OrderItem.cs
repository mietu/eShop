using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 表示订单中的一个商品项。
/// 这个类封装了订单项的所有属性和业务规则。
/// 作为实体，它继承自Entity基类，拥有唯一标识符。
/// </summary>
public class OrderItem
    : Entity
{
    /// <summary>
    /// 获取或设置商品名称。
    /// 此属性是必需的，不能为null或空。
    /// </summary>
    [Required]
    public string ProductName { get; private set; }

    /// <summary>
    /// 获取或设置商品图片URL。
    /// </summary>
    public string PictureUrl { get; private set; }

    /// <summary>
    /// 获取或设置商品单价。
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// 获取或设置商品折扣金额。
    /// </summary>
    public decimal Discount { get; private set; }

    /// <summary>
    /// 获取或设置商品数量。
    /// </summary>
    public int Units { get; private set; }

    /// <summary>
    /// 获取或设置商品ID。
    /// </summary>
    public int ProductId { get; private set; }

    /// <summary>
    /// 受保护的无参构造函数，用于ORM映射。
    /// </summary>
    protected OrderItem() { }

    /// <summary>
    /// 创建新的订单项实例。
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="productName">商品名称</param>
    /// <param name="unitPrice">商品单价</param>
    /// <param name="discount">折扣金额</param>
    /// <param name="pictureUrl">商品图片URL</param>
    /// <param name="units">商品数量，默认为1</param>
    /// <exception cref="OrderingDomainException">
    /// 当商品数量小于或等于0时抛出异常。
    /// 当折扣金额大于商品总价时抛出异常。
    /// </exception>
    public OrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        if (units <= 0)
        {
            throw new OrderingDomainException("无效的商品数量");
        }

        if ((unitPrice * units) < discount)
        {
            throw new OrderingDomainException("订单商品的总和低于应用的折扣");
        }

        ProductId = productId;

        ProductName = productName;
        UnitPrice = unitPrice;
        Discount = discount;
        Units = units;
        PictureUrl = pictureUrl;
    }

    /// <summary>
    /// 设置新的折扣金额。
    /// </summary>
    /// <param name="discount">新的折扣金额</param>
    /// <exception cref="OrderingDomainException">当折扣金额为负数时抛出异常</exception>
    public void SetNewDiscount(decimal discount)
    {
        if (discount < 0)
        {
            throw new OrderingDomainException("折扣无效");
        }

        Discount = discount;
    }

    /// <summary>
    /// 增加商品数量。
    /// </summary>
    /// <param name="units">要增加的商品数量</param>
    /// <exception cref="OrderingDomainException">当增加的数量为负数时抛出异常</exception>
    public void AddUnits(int units)
    {
        if (units < 0)
        {
            throw new OrderingDomainException("无效商品");
        }

        Units += units;
    }
}
