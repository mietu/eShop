using eShop.Basket.API.Model;

namespace eShop.Basket.API.Repositories;

/// <summary>
/// 购物篮仓储接口，提供购物篮数据的访问操作
/// </summary>
public interface IBasketRepository
{
    /// <summary>
    /// 根据客户ID异步获取购物篮
    /// </summary>
    /// <param name="customerId">客户的唯一标识符</param>
    /// <returns>客户的购物篮对象，如果不存在则返回null</returns>
    Task<CustomerBasket> GetBasketAsync(string customerId);

    /// <summary>
    /// 异步更新购物篮
    /// </summary>
    /// <param name="basket">要更新的购物篮对象</param>
    /// <returns>更新后的购物篮对象</returns>
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);

    /// <summary>
    /// 根据ID异步删除购物篮
    /// </summary>
    /// <param name="id">要删除的购物篮的客户ID</param>
    /// <returns>删除成功返回true，否则返回false</returns>
    Task<bool> DeleteBasketAsync(string id);
}
