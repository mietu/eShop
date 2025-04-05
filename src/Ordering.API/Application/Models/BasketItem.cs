namespace eShop.Ordering.API.Application.Models;

/// <summary>
/// 表示购物篮中的单个商品项
/// </summary>
public class BasketItem
{
    /// <summary>
    /// 获取或初始化购物篮项的唯一标识符
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// 获取或初始化商品的唯一标识符
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// 获取或初始化商品的名称
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// 获取或初始化商品的当前单价
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// 获取或初始化商品的原始单价（用于比较或显示折扣信息）
    /// </summary>
    public decimal OldUnitPrice { get; init; }

    /// <summary>
    /// 获取或初始化购物篮中该商品的数量
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 获取或初始化商品图片的URL
    /// </summary>
    public string PictureUrl { get; init; }
}

