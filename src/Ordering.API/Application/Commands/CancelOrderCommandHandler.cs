namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 处理取消订单命令的处理器。
/// 实现了MediatR的IRequestHandler接口，用于处理CancelOrderCommand命令。
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 初始化取消订单命令处理器的新实例。
    /// </summary>
    /// <param name="orderRepository">订单仓储接口，用于访问和修改订单状态</param>
    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// 处理取消订单命令。
    /// 当客户从应用程序中执行取消订单操作时，此处理器负责更新订单状态。
    /// </summary>
    /// <param name="command">包含订单编号的取消订单命令</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>
    /// 布尔值表示操作是否成功:
    /// - true: 订单成功取消
    /// - false: 订单不存在或无法取消
    /// </returns>
    public async Task<bool> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        // 从仓储中获取要更新的订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 如果订单不存在，返回失败
            return false;
        }

        // 设置订单为已取消状态
        orderToUpdate.SetCancelledStatus();

        // 保存更改并返回操作结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 用于实现幂等性的取消订单命令处理器。
/// 继承自IdentifiedCommandHandler基类，确保相同命令不会被重复处理。
/// </summary>
/// <remarks>
/// 当需要确保同一请求不会被多次处理时使用此处理器。
/// 例如，当网络不稳定时客户端可能会多次发送相同的取消订单请求。
/// </remarks>
public class CancelOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CancelOrderCommand, bool>
{
    /// <summary>
    /// 初始化幂等性取消订单命令处理器的新实例。
    /// </summary>
    /// <param name="mediator">中介者接口，用于向下游发送命令</param>
    /// <param name="requestManager">请求管理器，用于跟踪和管理请求ID</param>
    /// <param name="logger">日志接口，用于记录处理过程</param>
    public CancelOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<CancelOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 为重复的请求创建结果。
    /// 当检测到请求ID已经被处理过时，此方法将被调用以返回适当的结果。
    /// </summary>
    /// <returns>对于取消订单操作，重复请求返回true，表示操作视为成功</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复的订单处理请求，视为成功。
    }
}
