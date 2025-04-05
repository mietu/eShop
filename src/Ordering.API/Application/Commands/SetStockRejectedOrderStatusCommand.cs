namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 表示设置订单为"库存被拒绝"状态的命令
/// 当订单中的库存项目无法满足时使用此命令
/// </summary>
/// <param name="OrderNumber">需要更新状态的订单编号</param>
/// <param name="OrderStockItems">被拒绝的库存项目ID列表</param>
public record SetStockRejectedOrderStatusCommand(int OrderNumber, List<int> OrderStockItems)
    : IRequest<bool>;
