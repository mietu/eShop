namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 处理订单库存确认状态更新的命令处理器
/// 当库存服务确认订单中的商品可用时，通过此处理器更新订单状态
/// </summary>
public class SetStockConfirmedOrderStatusCommandHandler : IRequestHandler<SetStockConfirmedOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 构造函数，通过依赖注入获取订单仓储接口
    /// </summary>
    /// <param name="orderRepository">订单仓储接口，用于获取和更新订单</param>
    public SetStockConfirmedOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// 处理库存确认命令的核心方法
    /// 当库存服务确认请求后，将订单状态更新为"库存已确认"
    /// </summary>
    /// <param name="command">包含需要更新状态的订单编号的命令</param>
    /// <param name="cancellationToken">取消令牌，用于支持异步操作取消</param>
    /// <returns>处理成功返回true，失败返回false</returns>
    public async Task<bool> Handle(SetStockConfirmedOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟库存确认过程的工作时间（10秒）
        // 在实际生产环境中，这里可能是与库存系统的真实交互
        await Task.Delay(10000, cancellationToken);

        // 根据订单编号获取订单实体
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            return false; // 如果订单不存在，返回处理失败
        }

        // 调用领域实体的方法设置订单状态为"库存已确认"
        // 这符合DDD模式，状态变更逻辑封装在领域实体中
        orderToUpdate.SetStockConfirmedStatus();

        // 通过工作单元保存实体变更，实现事务性操作
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 用于处理具有幂等性的订单库存确认命令
/// 继承自通用的幂等命令处理器，确保同一命令不会被重复处理
/// </summary>
public class SetStockConfirmedOrderStatusIdentifiedCommandHandler : IdentifiedCommandHandler<SetStockConfirmedOrderStatusCommand, bool>
{
    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="mediator">中介者接口，用于发送内部命令</param>
    /// <param name="requestManager">请求管理器，用于跟踪请求以确保幂等性</param>
    /// <param name="logger">日志记录器，用于记录处理过程</param>
    public SetStockConfirmedOrderStatusIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetStockConfirmedOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 为重复请求创建结果
    /// 重写基类方法以定义当检测到重复请求时应返回的结果
    /// </summary>
    /// <returns>对于重复的库存确认请求，返回true表示成功，避免重复处理</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 对于处理订单的重复请求，忽略并返回成功
    }
}
