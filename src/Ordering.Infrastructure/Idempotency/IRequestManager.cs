namespace eShop.Ordering.Infrastructure.Idempotency;

/// <summary>
/// 定义用于管理请求幂等性的接口。
/// 该接口负责跟踪和管理命令请求，确保相同的请求不会被处理多次。
/// </summary>
public interface IRequestManager
{
    /// <summary>
    /// 检查具有指定ID的请求是否已存在。
    /// </summary>
    /// <param name="id">要检查的请求的唯一标识符。</param>
    /// <returns>如果请求已存在，则返回true；否则返回false。</returns>
    Task<bool> ExistAsync(Guid id);

    /// <summary>
    /// 为指定类型的命令创建一个新的请求记录。
    /// </summary>
    /// <typeparam name="T">命令的类型。</typeparam>
    /// <param name="id">新请求的唯一标识符。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task CreateRequestForCommandAsync<T>(Guid id);
}
