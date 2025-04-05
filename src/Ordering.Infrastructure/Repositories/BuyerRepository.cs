namespace eShop.Ordering.Infrastructure.Repositories;

/// <summary>
/// 买家仓储实现类，提供对买家聚合根的CRUD操作
/// 用于管理买家实体的持久化，包括添加、更新和查询功能
/// </summary>
public class BuyerRepository
    : IBuyerRepository
{
    /// <summary>
    /// 订单上下文，提供数据库访问能力
    /// </summary>
    private readonly OrderingContext _context;

    /// <summary>
    /// 获取工作单元实例，用于事务管理和持久化操作
    /// </summary>
    public IUnitOfWork UnitOfWork => _context;

    /// <summary>
    /// 初始化买家仓储的新实例
    /// </summary>
    /// <param name="context">订单上下文实例</param>
    /// <exception cref="ArgumentNullException">当上下文为null时抛出</exception>
    public BuyerRepository(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 添加新买家到仓储
    /// </summary>
    /// <param name="buyer">要添加的买家实体</param>
    /// <returns>添加后的买家实体</returns>
    /// <remarks>
    /// 只有当买家是临时状态(未持久化)时才会添加到数据库
    /// </remarks>
    public Buyer Add(Buyer buyer)
    {
        if (buyer.IsTransient())
        {
            return _context.Buyers
                .Add(buyer)
                .Entity;
        }

        return buyer;
    }

    /// <summary>
    /// 更新现有买家信息
    /// </summary>
    /// <param name="buyer">包含更新信息的买家实体</param>
    /// <returns>更新后的买家实体</returns>
    public Buyer Update(Buyer buyer)
    {
        return _context.Buyers
                .Update(buyer)
                .Entity;
    }

    /// <summary>
    /// 根据身份标识查找买家
    /// </summary>
    /// <param name="identity">买家的身份标识</param>
    /// <returns>匹配的买家实体，如果未找到则返回null</returns>
    public async Task<Buyer> FindAsync(string identity)
    {
        var buyer = await _context.Buyers
            .Include(b => b.PaymentMethods)
            .Where(b => b.IdentityGuid == identity)
            .SingleOrDefaultAsync();

        return buyer;
    }

    /// <summary>
    /// 根据ID查找买家
    /// </summary>
    /// <param name="id">买家ID</param>
    /// <returns>匹配的买家实体，如果未找到则返回null</returns>
    public async Task<Buyer> FindByIdAsync(int id)
    {
        var buyer = await _context.Buyers
            .Include(b => b.PaymentMethods)
            .Where(b => b.Id == id)
            .SingleOrDefaultAsync();

        return buyer;
    }
}
