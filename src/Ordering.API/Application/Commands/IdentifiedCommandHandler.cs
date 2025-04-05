namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 提供用于处理重复请求并确保幂等更新的基本实现，适用于客户端发送请求ID用于检测重复请求的情况。
/// </summary>
/// <typeparam name="T">如果请求不重复，执行操作的命令处理程序的类型</typeparam>
/// <typeparam name="R">内部命令处理程序的返回值类型</typeparam>
public abstract class IdentifiedCommandHandler<T, R> : IRequestHandler<IdentifiedCommand<T, R>, R>
    where T : IRequest<R>
{
    private readonly IMediator _mediator;            // 中介者对象，用于发送命令到对应的处理程序
    private readonly IRequestManager _requestManager; // 请求管理器，用于检查和创建请求记录
    private readonly ILogger<IdentifiedCommandHandler<T, R>> _logger; // 日志记录器

    /// <summary>
    /// 构造函数，初始化命令处理程序
    /// </summary>
    /// <param name="mediator">中介者实例，用于发送内部命令</param>
    /// <param name="requestManager">请求管理器，用于处理请求去重</param>
    /// <param name="logger">日志记录器</param>
    public IdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<T, R>> logger)
    {
        ArgumentNullException.ThrowIfNull(logger); // 验证日志记录器不为空
        _mediator = mediator;
        _requestManager = requestManager;
        _logger = logger;
    }

    /// <summary>
    /// 为重复请求创建结果值
    /// </summary>
    /// <returns>对于重复请求应返回的值</returns>
    protected abstract R CreateResultForDuplicateRequest();

    /// <summary>
    /// 处理命令的方法。确保不存在具有相同ID的其他请求，如果是新请求，则将原始内部命令加入队列。
    /// </summary>
    /// <param name="message">包含原始命令和请求ID的IdentifiedCommand</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>内部命令的返回值，或者如果找到相同ID的请求则返回默认值</returns>
    public async Task<R> Handle(IdentifiedCommand<T, R> message, CancellationToken cancellationToken)
    {
        // 检查请求是否已存在
        var alreadyExists = await _requestManager.ExistAsync(message.Id);
        if (alreadyExists)
        {
            // 如果请求已存在，返回为重复请求创建的结果
            return CreateResultForDuplicateRequest();
        }
        else
        {
            // 如果是新请求，创建请求记录
            await _requestManager.CreateRequestForCommandAsync<T>(message.Id);
            try
            {
                // 获取内部命令
                var command = message.Command;
                var commandName = command.GetGenericTypeName(); // 获取命令类型名称
                var idProperty = string.Empty; // 标识属性名
                var commandId = string.Empty;  // 标识属性值

                // 根据命令类型确定标识属性
                switch (command)
                {
                    case CreateOrderCommand createOrderCommand:
                        idProperty = nameof(createOrderCommand.UserId);
                        commandId = createOrderCommand.UserId;
                        break;

                    case CancelOrderCommand cancelOrderCommand:
                        idProperty = nameof(cancelOrderCommand.OrderNumber);
                        commandId = $"{cancelOrderCommand.OrderNumber}";
                        break;

                    case ShipOrderCommand shipOrderCommand:
                        idProperty = nameof(shipOrderCommand.OrderNumber);
                        commandId = $"{shipOrderCommand.OrderNumber}";
                        break;

                    default:
                        idProperty = "Id?";
                        commandId = "n/a";
                        break;
                }

                // 记录发送命令的日志
                _logger.LogInformation(
                    "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    commandName,
                    idProperty,
                    commandId,
                    command);

                // 通过中介者发送内部业务命令，以便运行相关的CommandHandler 
                var result = await _mediator.Send(command, cancellationToken);

                // 记录命令结果的日志
                _logger.LogInformation(
                    "命令结果: {@Result} - {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                    result,
                    commandName,
                    idProperty,
                    commandId,
                    command);

                return result;
            }
            catch
            {
                // 发生异常时返回默认值
                return default;
            }
        }
    }
}
