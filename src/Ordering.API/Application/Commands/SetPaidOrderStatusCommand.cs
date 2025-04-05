namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 设置订单为已支付状态的命令
/// </summary>
/// <param name="OrderNumber">需要更新状态的订单编号</param>
public record SetPaidOrderStatusCommand(int OrderNumber) : IRequest<bool>;
