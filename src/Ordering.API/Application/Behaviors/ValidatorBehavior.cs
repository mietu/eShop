namespace eShop.Ordering.API.Application.Behaviors;

/// <summary>
/// 验证行为类，实现MediatR的IPipelineBehavior接口
/// 用于在处理请求前对请求进行验证
/// </summary>
/// <typeparam name="TRequest">请求类型，必须是IRequest<TResponse>的实现</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// 构造函数，通过依赖注入获取所有适用于当前请求类型的验证器和日志记录器
    /// </summary>
    /// <param name="validators">适用于TRequest类型的所有验证器集合</param>
    /// <param name="logger">日志记录器</param>
    public ValidatorBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// 处理请求的方法，实现管道行为
    /// 在请求处理前执行所有验证器，如有验证错误则抛出异常
    /// </summary>
    /// <param name="request">待处理的请求</param>
    /// <param name="next">请求处理管道中的下一个处理器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>请求处理的结果</returns>
    /// <exception cref="OrderingDomainException">当验证失败时抛出，包含具体的验证错误信息</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 获取请求类型名称，用于日志记录
        var typeName = request.GetGenericTypeName();

        _logger.LogInformation("正在验证命令 {CommandType}", typeName);

        // 执行所有验证器，收集验证错误
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        // 如果存在验证错误，记录日志并抛出异常
        if (failures.Any())
        {
            _logger.LogWarning("验证错误 - {CommandType} - 命令: {@Command} - 错误: {@ValidationErrors}", typeName, request, failures);

            throw new OrderingDomainException(
                $"类型 {typeof(TRequest).Name} 命令验证错误", new ValidationException("Validation exception", failures));
        }

        // 验证通过，继续处理请求
        return await next();
    }
}
