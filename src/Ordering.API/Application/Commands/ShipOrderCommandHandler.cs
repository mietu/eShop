namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 处理发货订单命令的常规处理程序
/// 该处理程序负责将订单状态更新为已发货
/// </summary>
public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, bool>
{
    /// <summary>
    /// 订单仓储接口，用于访问和操作订单数据
    /// </summary>
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 初始化 ShipOrderCommandHandler 的新实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    public ShipOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// 处理发货订单命令的方法
    /// 当管理员从应用程序执行订单发货操作时，此方法被调用
    /// </summary>
    /// <param name="command">包含要发货的订单编号的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功返回true，失败返回false</returns>
    public async Task<bool> Handle(ShipOrderCommand command, CancellationToken cancellationToken)
    {
        // 通过订单编号获取订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            // 如果订单不存在，返回失败
            return false;
        }

        // 设置订单状态为已发货
        orderToUpdate.SetShippedStatus();

        // 保存更改并返回结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 带有幂等性支持的发货订单命令处理程序
/// 用于确保相同的发货命令不会被重复处理，保证操作的幂等性
/// </summary>
public class ShipOrderIdentifiedCommandHandler : IdentifiedCommandHandler<ShipOrderCommand, bool>
{
    /// <summary>
    /// 初始化 ShipOrderIdentifiedCommandHandler 的新实例
    /// </summary>
    /// <param name="mediator">中介者，用于转发内部命令</param>
    /// <param name="requestManager">请求管理器，用于跟踪和管理请求ID</param>
    /// <param name="logger">日志记录器</param>
    public ShipOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<ShipOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 为重复请求创建结果
    /// 如果相同ID的请求已经被处理过，则返回此方法的结果
    /// </summary>
    /// <returns>对于发货命令，重复请求返回true，表示发货成功（幂等操作）</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复的处理订单请求，返回成功
    }
}
