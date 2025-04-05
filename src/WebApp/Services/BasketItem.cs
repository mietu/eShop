namespace eShop.WebApp.Services;

/// <summary>
/// 表示购物篮中的单个商品项目
/// </summary>
public class BasketItem
{
    /// <summary>
    /// 获取或设置购物篮项目的唯一标识符
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 获取或设置关联产品的唯一标识符
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 获取或设置产品名称
    /// </summary>
    public required string ProductName { get; set; }

    /// <summary>
    /// 获取或设置产品的当前单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 获取或设置产品的原始单价，通常用于显示折扣信息
    /// </summary>
    public decimal OldUnitPrice { get; set; }

    /// <summary>
    /// 获取或设置购物篮中该产品的数量
    /// </summary>
    public int Quantity { get; set; }
}
