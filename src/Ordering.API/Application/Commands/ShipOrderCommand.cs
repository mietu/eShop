namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 表示将订单标记为已发货的命令
/// </summary>
/// <param name="OrderNumber">要发货的订单编号</param>
public record ShipOrderCommand(int OrderNumber) : IRequest<bool>;
