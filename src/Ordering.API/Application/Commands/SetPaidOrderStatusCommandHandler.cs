namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 处理设置订单为已支付状态的命令处理器
/// 直接响应 SetPaidOrderStatusCommand 请求
/// </summary>
public class SetPaidOrderStatusCommandHandler : IRequestHandler<SetPaidOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 初始化 SetPaidOrderStatusCommandHandler 的新实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    public SetPaidOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// 处理命令的方法，当支付服务确认支付后被调用
    /// 将订单状态更新为已支付
    /// </summary>
    /// <param name="command">包含订单编号的支付确认命令</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>更新操作是否成功的布尔值</returns>
    public async Task<bool> Handle(SetPaidOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 模拟验证支付的工作时间
        await Task.Delay(10000, cancellationToken);

        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            return false;
        }

        orderToUpdate.SetPaidStatus();
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 处理设置订单为已支付状态的幂等命令处理器
/// 用于确保命令处理的幂等性，防止重复处理相同的支付确认请求
/// </summary>
public class SetPaidIdentifiedOrderStatusCommandHandler : IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>
{
    /// <summary>
    /// 初始化 SetPaidIdentifiedOrderStatusCommandHandler 的新实例
    /// </summary>
    /// <param name="mediator">中介者接口，用于发送命令</param>
    /// <param name="requestManager">请求管理器，用于跟踪请求</param>
    /// <param name="logger">日志记录器</param>
    public SetPaidIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetPaidOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 创建重复请求的结果
    /// 当检测到重复的支付确认请求时返回的值
    /// </summary>
    /// <returns>对于重复的支付请求返回 true，表示操作成功</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略处理订单的重复请求，直接返回成功
    }
}
