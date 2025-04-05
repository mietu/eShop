namespace eShop.Basket.API.Model;

/// <summary>
/// 表示购物篮中的单个商品项
/// 实现IValidatableObject接口以提供自定义验证逻辑
/// </summary>
public class BasketItem : IValidatableObject
{
    /// <summary>
    /// 购物篮项目的唯一标识符
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 商品的ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// 商品的名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 商品的当前单价
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 商品的原始单价，用于显示折扣信息
    /// </summary>
    public decimal OldUnitPrice { get; set; }

    /// <summary>
    /// 购物篮中该商品的数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 商品图片的URL
    /// </summary>
    public string PictureUrl { get; set; }

    /// <summary>
    /// 验证当前购物篮项目的有效性
    /// </summary>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证结果的集合，如果验证通过则返回空集合</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // 验证商品数量必须至少为1
        if (Quantity < 1)
        {
            results.Add(new ValidationResult("无效的商品数量", ["Quantity"]));
        }

        return results;
    }
}
