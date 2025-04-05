namespace eShop.Ordering.Domain.Seedwork;

/// <summary>
/// 表示工作单元的接口，用于管理事务和持久化实体更改
/// </summary>
/// <remarks>
/// 工作单元模式用于确保多个数据库操作作为一个事务执行，
/// 保持数据的一致性和完整性
/// </remarks>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// 异步保存所有在上下文中的更改
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步保存实体更改并处理领域事件
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>
    /// 如果所有操作成功完成则返回 true，
    /// 否则返回 false
    /// </returns>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
