namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 表示取消订单的命令
/// </summary>
/// <remarks>
/// 此命令用于触发取消订单的操作流程，传递需要被取消的订单编号
/// </remarks>
public record CancelOrderCommand(int OrderNumber) : IRequest<bool>
{
    /// <summary>
    /// 获取要取消的订单编号
    /// </summary>
    /// <remarks>
    /// 此属性是通过记录构造函数自动生成的
    /// </remarks>
    public int OrderNumber { get; } = OrderNumber;
}

