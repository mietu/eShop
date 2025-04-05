namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 命令处理程序，负责将订单状态更新为"等待验证"状态
/// 实现了MediatR的IRequestHandler接口，处理SetAwaitingValidationOrderStatusCommand命令
/// </summary>
public class SetAwaitingValidationOrderStatusCommandHandler : IRequestHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository; // 订单仓储接口，用于访问和修改订单数据

    /// <summary>
    /// 构造函数，通过依赖注入接收订单仓储
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    public SetAwaitingValidationOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 处理命令的方法，当订单宽限期结束后执行
    /// 将指定订单的状态更新为"等待验证"状态
    /// </summary>
    /// <param name="command">包含要更新的订单编号的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功，如果订单存在并状态更新成功则返回true，否则返回false</returns>
    public async Task<bool> Handle(SetAwaitingValidationOrderStatusCommand command, CancellationToken cancellationToken)
    {
        // 从仓储中获取要更新的订单
        var orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
        if (orderToUpdate == null)
        {
            return false; // 如果订单不存在，返回失败
        }

        // 调用领域模型的方法设置订单状态为"等待验证"
        orderToUpdate.SetAwaitingValidationStatus();

        // 通过工作单元保存更改并返回结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 支持幂等性的命令处理程序，确保相同的命令不会被处理多次
/// 继承自IdentifiedCommandHandler基类，专门处理SetAwaitingValidationOrderStatusCommand命令
/// </summary>
public class SetAwaitingValidationIdentifiedOrderStatusCommandHandler : IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>
{
    /// <summary>
    /// 构造函数，通过依赖注入接收所需的服务
    /// </summary>
    /// <param name="mediator">MediatR中介者接口，用于发送命令</param>
    /// <param name="requestManager">请求管理器，用于检查和创建请求记录</param>
    /// <param name="logger">日志记录器</param>
    public SetAwaitingValidationIdentifiedOrderStatusCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<SetAwaitingValidationOrderStatusCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 当检测到重复请求时创建返回结果
    /// 重写基类方法以指定对重复请求的处理策略
    /// </summary>
    /// <returns>对于重复的订单处理请求，直接返回true表示成功</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略重复的订单处理请求，视为成功处理
    }
}
