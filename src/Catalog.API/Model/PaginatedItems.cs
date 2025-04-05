namespace eShop.Catalog.API.Model;

/// <summary>
/// 表示分页数据集合的通用类
/// </summary>
/// <typeparam name="TEntity">集合中包含的实体类型</typeparam>
/// <param name="pageIndex">当前页索引（从0开始）</param>
/// <param name="pageSize">每页项目数量</param>
/// <param name="count">总项目数</param>
/// <param name="data">当前页的数据项</param>
public class PaginatedItems<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data) where TEntity : class
{
    /// <summary>
    /// 获取当前页索引（从0开始）
    /// </summary>
    public int PageIndex { get; } = pageIndex;

    /// <summary>
    /// 获取每页项目数量
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// 获取总项目数
    /// </summary>
    public long Count { get; } = count;

    /// <summary>
    /// 获取当前页的数据项集合
    /// </summary>
    public IEnumerable<TEntity> Data { get; } = data;
}
