namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 表示设置订单状态为"库存已确认"的命令
/// </summary>
/// <param name="OrderNumber">要更新状态的订单编号</param>
public record SetStockConfirmedOrderStatusCommand(int OrderNumber) : IRequest<bool>;
