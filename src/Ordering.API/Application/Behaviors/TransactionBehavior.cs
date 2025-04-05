namespace eShop.Ordering.API.Application.Behaviors;

using Microsoft.Extensions.Logging;

/// <summary>
/// 事务行为处理器，负责为MediatR请求处理过程中提供事务支持
/// 确保请求处理和集成事件发布在同一事务中完成
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly OrderingContext _dbContext;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 初始化事务行为处理器
    /// </summary>
    /// <param name="dbContext">订单数据库上下文</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentException">当任何依赖项为null时抛出</exception>
    public TransactionBehavior(OrderingContext dbContext,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentException(nameof(OrderingContext));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentException(nameof(orderingIntegrationEventService));
        _logger = logger ?? throw new ArgumentException(nameof(ILogger));
    }

    /// <summary>
    /// 处理请求并在事务中执行操作
    /// </summary>
    /// <param name="request">MediatR请求</param>
    /// <param name="next">下一个请求处理委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理结果</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var typeName = request.GetGenericTypeName();

        try
        {
            // 如果上下文已有活动事务，则跳过创建新事务
            if (_dbContext.HasActiveTransaction)
            {
                return await next();
            }

            // 创建数据库执行策略（支持重试等机制）
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            // 使用执行策略包装事务处理
            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                // 开始数据库事务
                await using var transaction = await _dbContext.BeginTransactionAsync();
                using (_logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                {
                    _logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction.TransactionId, typeName, request);

                    // 执行实际的命令处理
                    response = await next();

                    _logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", transaction.TransactionId, typeName);

                    // 提交事务
                    await _dbContext.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }

                // 事务提交后发布集成事件
                await _orderingIntegrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);

            // 保持原始异常栈
            throw;
        }
    }
}
