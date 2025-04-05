namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 标准命令处理器 - 处理库存被拒绝时订单状态的变更
/// </summary>
public class SetStockRejectedOrderStatusCommandHandler : IRequestHandler<SetStockRejectedOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 初始化命令处理器实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    public SetStockRejectedOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// 处理命令的核心方法 - 当库存服务拒绝请求时更新订单状态
    /// </summary>
    /// <param name="command">包含订单号和被拒绝库存项的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功的布尔值</returns>
    public async Task<bool> Handle(SetStockRejectedOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟处理库存拒绝的工作时间
        await Task.Delay(10000, cancellationToken);

        // 获取需要更新的订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            return false; // 订单不存在，返回失败
        }

        // 将订单状态设置为因库存问题被取消
        orderToUpdate.SetCancelledStatusWhenStockIsRejected(command.OrderStockItems);

        // 持久化更改并返回结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 幂等命令处理器 - 确保重复的订单状态变更命令不会导致多次处理
/// 通过请求标识符识别并处理重复请求
/// </summary>
public class SetStockRejectedOrderStatusIdentifiedCommandHandler : IdentifiedCommandHandler<SetStockRejectedOrderStatusCommand, bool>
{
    /// <summary>
    /// 初始化幂等命令处理器实例
    /// </summary>
    /// <param name="mediator">中介者接口，用于发送内部命令</param>
    /// <param name="requestManager">请求管理器，用于跟踪已处理的请求</param>
    /// <param name="logger">日志记录器</param>
    public SetStockRejectedOrderStatusIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetStockRejectedOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 为重复请求创建结果
    /// 在处理订单状态变更时，重复的请求可以安全地被忽略
    /// </summary>
    /// <returns>对重复请求的响应值</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 订单处理中忽略重复请求，返回成功
    }
}
