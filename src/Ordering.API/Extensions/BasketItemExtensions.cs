namespace eShop.Ordering.API.Extensions;

/// <summary>
/// 提供用于BasketItem对象转换到OrderItemDTO的扩展方法
/// 将购物篮商品项转换为订单商品项数据传输对象
/// </summary>
public static class BasketItemExtensions
{
    /// <summary>
    /// 将购物篮商品项集合转换为订单商品项DTO集合
    /// </summary>
    /// <param name="basketItems">需要转换的购物篮商品项集合</param>
    /// <returns>转换后的订单商品项DTO集合</returns>
    public static IEnumerable<OrderItemDTO> ToOrderItemsDTO(this IEnumerable<BasketItem> basketItems)
    {
        foreach (var item in basketItems)
        {
            yield return item.ToOrderItemDTO();
        }
    }

    /// <summary>
    /// 将单个购物篮商品项转换为订单商品项DTO
    /// </summary>
    /// <param name="item">需要转换的购物篮商品项</param>
    /// <returns>转换后的订单商品项DTO</returns>
    public static OrderItemDTO ToOrderItemDTO(this BasketItem item)
    {
        return new OrderItemDTO()
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            PictureUrl = item.PictureUrl,
            UnitPrice = item.UnitPrice,
            Units = item.Quantity
        };
    }
}
