namespace eShop.Basket.API.Model;

/// <summary>
/// 表示客户购物篮的数据模型
/// </summary>
public class CustomerBasket
{
    /// <summary>
    /// 获取或设置购买者的唯一标识符
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 获取或设置购物篮中的商品列表
    /// </summary>
    public List<BasketItem> Items { get; set; } = [];

    /// <summary>
    /// 初始化 <see cref="CustomerBasket"/> 类的新实例
    /// </summary>
    public CustomerBasket() { }

    /// <summary>
    /// 使用指定的客户ID初始化 <see cref="CustomerBasket"/> 类的新实例
    /// </summary>
    /// <param name="customerId">客户的唯一标识符</param>
    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
    }
}
