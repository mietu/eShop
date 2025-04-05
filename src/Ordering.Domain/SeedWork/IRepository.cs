namespace eShop.Ordering.Domain.Seedwork;

/// <summary>
/// 仓储接口，定义了对聚合根实体进行持久化操作的基本契约
/// </summary>
/// <typeparam name="T">必须实现IAggregateRoot接口的聚合根实体类型</typeparam>
/// <remarks>
/// 该接口作为领域驱动设计(DDD)中仓储模式的基础定义，
/// 用于封装数据访问逻辑并提供对聚合根的持久化操作。
/// </remarks>
public interface IRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// 获取工作单元实例，用于管理事务和持久化操作
    /// </summary>
    /// <remarks>
    /// 工作单元负责跟踪变更并将它们作为单个事务提交到数据库
    /// </remarks>
    IUnitOfWork UnitOfWork { get; }
}
