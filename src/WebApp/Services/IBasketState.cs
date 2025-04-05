using eShop.WebAppComponents.Catalog;

namespace eShop.WebApp.Services
{
    /// <summary>
    /// 购物篮状态管理接口
    /// 负责管理用户购物篮中的商品项并提供相关操作
    /// </summary>
    public interface IBasketState
    {
        /// <summary>
        /// 获取购物篮中的所有商品项
        /// </summary>
        /// <returns>购物篮中商品项的只读集合</returns>
        public Task<IReadOnlyCollection<BasketItem>> GetBasketItemsAsync();

        /// <summary>
        /// 向购物篮中添加商品
        /// </summary>
        /// <param name="item">要添加到购物篮的商品</param>
        /// <returns>表示异步操作的任务</returns>
        public Task AddAsync(CatalogItem item);
    }
}
