namespace eShop.Ordering.Infrastructure.Idempotency;

/// <summary>
/// 请求管理器，负责处理命令的幂等性
/// 通过跟踪已处理的请求ID来确保相同的命令不会被多次执行
/// </summary>
public class RequestManager : IRequestManager
{
    /// <summary>
    /// 订单上下文，用于访问数据库
    /// </summary>
    private readonly OrderingContext _context;

    /// <summary>
    /// 初始化请求管理器的新实例
    /// </summary>
    /// <param name="context">订单数据库上下文</param>
    /// <exception cref="ArgumentNullException">当上下文为null时抛出</exception>
    public RequestManager(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 异步检查指定ID的请求是否已存在
    /// </summary>
    /// <param name="id">请求的唯一标识符</param>
    /// <returns>如果请求存在则返回true，否则返回false</returns>
    public async Task<bool> ExistAsync(Guid id)
    {
        var request = await _context.
            FindAsync<ClientRequest>(id);

        return request != null;
    }

    /// <summary>
    /// 为指定的命令类型创建新的请求记录
    /// </summary>
    /// <typeparam name="T">命令的类型</typeparam>
    /// <param name="id">请求的唯一标识符</param>
    /// <returns>创建请求的异步任务</returns>
    /// <exception cref="OrderingDomainException">当请求ID已存在时抛出</exception>
    public async Task CreateRequestForCommandAsync<T>(Guid id)
    {
        var exists = await ExistAsync(id);

        var request = exists ?
            throw new OrderingDomainException($"具有 {id} 的请求已存在") :
            new ClientRequest()
            {
                Id = id,
                Name = typeof(T).Name,
                Time = DateTime.UtcNow
            };

        _context.Add(request);

        await _context.SaveChangesAsync();
    }
}
