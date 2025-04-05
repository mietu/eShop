namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate;

/// <summary>
/// 表示支付卡类型的值对象
/// </summary>
public sealed class CardType
{
    /// <summary>
    /// 获取或初始化卡类型的唯一标识符
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 获取或初始化卡类型的名称
    /// </summary>
    public required string Name { get; init; }
}
