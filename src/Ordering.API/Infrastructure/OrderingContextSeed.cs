namespace eShop.Ordering.API.Infrastructure;

using eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// 订单上下文数据库种子初始化器，负责为订单系统填充必要的初始数据
/// </summary>
public class OrderingContextSeed : IDbSeeder<OrderingContext>
{
    /// <summary>
    /// 执行数据库种子数据初始化
    /// </summary>
    /// <param name="context">订单数据库上下文</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SeedAsync(OrderingContext context)
    {
        // 检查卡类型表是否为空，如果为空则添加预定义的卡类型
        if (!context.CardTypes.Any())
        {
            context.CardTypes.AddRange(GetPredefinedCardTypes());

            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 获取预定义的支付卡类型集合
    /// </summary>
    /// <returns>卡类型集合</returns>
    private static IEnumerable<CardType> GetPredefinedCardTypes()
    {
        yield return new CardType { Id = 1, Name = "Amex" };
        yield return new CardType { Id = 2, Name = "Visa" };
        yield return new CardType { Id = 3, Name = "MasterCard" };
    }
}
