namespace eShop.Ordering.API.Application.Models;

/// <summary>
/// 表示用户购物篮，包含买家标识和购物项列表
/// </summary>
public class CustomerBasket
{
    /// <summary>
    /// 获取或设置购物篮所属买家的唯一标识符
    /// </summary>
    public string BuyerId { get; set; }

    /// <summary>
    /// 获取或设置购物篮中的商品项列表
    /// </summary>
    public List<BasketItem> Items { get; set; }

    /// <summary>
    /// 初始化 <see cref="CustomerBasket"/> 类的新实例
    /// </summary>
    /// <param name="buyerId">购物篮所属买家的唯一标识符</param>
    /// <param name="items">购物篮中的商品项列表</param>
    public CustomerBasket(string buyerId, List<BasketItem> items)
    {
        BuyerId = buyerId;
        Items = items;
    }
}
