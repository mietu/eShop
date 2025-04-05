using System.Text.Json.Serialization;
using eShop.Basket.API.Model;

namespace eShop.Basket.API.Repositories;

/// <summary>
/// Redis购物篮仓储实现，用于在Redis中存储和检索购物篮数据
/// </summary>
public class RedisBasketRepository(ILogger<RedisBasketRepository> logger, IConnectionMultiplexer redis) : IBasketRepository
{
    private readonly IDatabase _database = redis.GetDatabase();

    // Redis键实现:

    // - /basket/{id} 每个唯一购物篮使用一个"string"类型键
    private static RedisKey BasketKeyPrefix = "/basket/"u8.ToArray();
    // 注意关于UTF8：库限制（待修复）- 前缀作为二进制数据更高效

    /// <summary>
    /// 根据用户ID生成购物篮的Redis键
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>完整的Redis键</returns>
    private static RedisKey GetBasketKey(string userId) => BasketKeyPrefix.Append(userId);

    /// <summary>
    /// 删除指定ID的购物篮
    /// </summary>
    /// <param name="id">购物篮ID（即用户ID）</param>
    /// <returns>如果删除成功返回true，否则返回false</returns>
    public async Task<bool> DeleteBasketAsync(string id)
    {
        return await _database.KeyDeleteAsync(GetBasketKey(id));
    }

    /// <summary>
    /// 获取指定客户ID的购物篮
    /// </summary>
    /// <param name="customerId">客户ID</param>
    /// <returns>客户的购物篮，如果不存在则返回null</returns>
    public async Task<CustomerBasket> GetBasketAsync(string customerId)
    {
        using var data = await _database.StringGetLeaseAsync(GetBasketKey(customerId));

        if (data is null || data.Length == 0)
        {
            return null;
        }
        return JsonSerializer.Deserialize(data.Span, BasketSerializationContext.Default.CustomerBasket);
    }

    /// <summary>
    /// 更新购物篮信息
    /// </summary>
    /// <param name="basket">要更新的购物篮对象</param>
    /// <returns>更新后的购物篮对象，如果保存失败则返回null</returns>
    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(basket, BasketSerializationContext.Default.CustomerBasket);
        var created = await _database.StringSetAsync(GetBasketKey(basket.BuyerId), json);

        if (!created)
        {
            logger.LogInformation("保存购物篮项目时出现问题。");
            return null;
        }

        logger.LogInformation("购物篮项目已成功保存。");
        return await GetBasketAsync(basket.BuyerId);
    }
}

/// <summary>
/// 购物篮JSON序列化上下文
/// 使用源生成的JSON序列化器提高性能
/// </summary>
[JsonSerializable(typeof(CustomerBasket))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class BasketSerializationContext : JsonSerializerContext
{
}
