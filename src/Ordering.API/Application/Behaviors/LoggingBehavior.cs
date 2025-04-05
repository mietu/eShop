namespace eShop.Ordering.API.Application.Behaviors;
/// <summary>
/// 行为管道组件，用于在处理请求前后添加日志记录
/// 实现了MediatR的IPipelineBehavior接口，可以拦截请求并添加横切关注点
/// </summary>
/// <typeparam name="TRequest">请求类型，必须实现IRequest<TResponse>接口</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// 构造函数，通过依赖注入获取日志记录器
    /// </summary>
    /// <param name="logger">类型化的日志记录器实例</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    /// <summary>
    /// 处理请求的方法，在请求执行前后添加日志记录
    /// </summary>
    /// <param name="request">当前处理的请求对象</param>
    /// <param name="next">请求处理的委托，指向下一个处理器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理结果</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 记录请求开始处理的日志，包含命令名称和请求内容
        _logger.LogInformation("处理命令 {CommandName} ({@Command})", request.GetGenericTypeName(), request);

        // 调用下一个处理器处理请求
        var response = await next();

        // 记录请求处理完成的日志，包含命令名称和响应内容
        _logger.LogInformation("命令 {CommandName} 被处理 - 响应: {@Response}", request.GetGenericTypeName(), response);

        return response;
    }
}

