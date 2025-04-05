namespace eShop.IntegrationEventLogEF.Utilities;

/// <summary>
/// 提供弹性数据库事务执行的辅助类。
/// 该类封装了 EF Core 的执行策略，以便在发生暂时性故障时能够自动重试事务操作。
/// </summary>
public class ResilientTransaction
{
    private readonly DbContext _context;

    /// <summary>
    /// 私有构造函数，防止直接实例化。请使用 <see cref="New"/> 工厂方法创建实例。
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <exception cref="ArgumentNullException">当 context 为 null 时抛出</exception>
    private ResilientTransaction(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// 创建 <see cref="ResilientTransaction"/> 的新实例。
    /// </summary>
    /// <param name="context">用于事务操作的数据库上下文</param>
    /// <returns>新的 ResilientTransaction 实例</returns>
    public static ResilientTransaction New(DbContext context) => new(context);

    /// <summary>
    /// 在弹性事务中执行指定的操作。
    /// 如果发生暂时性故障，将根据 EF Core 的执行策略自动重试。
    /// </summary>
    /// <param name="action">要在事务中执行的操作</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task ExecuteAsync(Func<Task> action)
    {
        //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await action();
            await transaction.CommitAsync();
        });
    }
}
