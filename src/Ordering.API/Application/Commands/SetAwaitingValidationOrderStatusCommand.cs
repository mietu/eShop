namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 表示将订单状态设置为"等待验证"的命令
/// </summary>
/// <remarks>
/// 此命令用于在订单流程中将指定订单标记为等待验证状态，
/// 通常在订单支付完成后、最终确认前使用。
/// </remarks>
/// <param name="OrderNumber">需要更新状态的订单编号</param>
public record SetAwaitingValidationOrderStatusCommand(int OrderNumber) : IRequest<bool>;
