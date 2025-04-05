using eShop.Basket.API.Grpc;
using GrpcBasketClient = eShop.Basket.API.Grpc.Basket.BasketClient;
using GrpcBasketItem = eShop.Basket.API.Grpc.BasketItem;

namespace eShop.WebApp.Services;

/// <summary>
/// 购物篮服务，用于处理用户购物篮的数据操作
/// 通过 gRPC 客户端与购物篮 API 进行通信
/// </summary>
public class BasketService(GrpcBasketClient basketClient)
{
    /// <summary>
    /// 获取当前用户的购物篮内容
    /// </summary>
    /// <returns>购物篮中商品及其数量的只读集合</returns>
    public async Task<IReadOnlyCollection<BasketQuantity>> GetBasketAsync()
    {
        var result = await basketClient.GetBasketAsync(new());
        return MapToBasket(result);
    }

    /// <summary>
    /// 删除当前用户的购物篮
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public async Task DeleteBasketAsync()
    {
        await basketClient.DeleteBasketAsync(new DeleteBasketRequest());
    }

    /// <summary>
    /// 更新购物篮的内容
    /// </summary>
    /// <param name="basket">要更新到购物篮中的商品集合</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task UpdateBasketAsync(IReadOnlyCollection<BasketQuantity> basket)
    {
        var updatePayload = new UpdateBasketRequest();

        foreach (var item in basket)
        {
            var updateItem = new GrpcBasketItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            };
            updatePayload.Items.Add(updateItem);
        }

        await basketClient.UpdateBasketAsync(updatePayload);
    }

    /// <summary>
    /// 将 gRPC 响应对象转换为应用程序使用的购物篮模型
    /// </summary>
    /// <param name="response">gRPC 服务返回的购物篮响应</param>
    /// <returns>购物篮商品数量列表</returns>
    private static List<BasketQuantity> MapToBasket(CustomerBasketResponse response)
    {
        var result = new List<BasketQuantity>();
        foreach (var item in response.Items)
        {
            result.Add(new BasketQuantity(item.ProductId, item.Quantity));
        }

        return result;
    }
}

/// <summary>
/// 表示购物篮中的商品及其数量
/// </summary>
/// <param name="ProductId">商品的唯一标识符</param>
/// <param name="Quantity">商品的数量</param>
public record BasketQuantity(int ProductId, int Quantity);
