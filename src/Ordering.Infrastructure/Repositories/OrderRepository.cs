namespace eShop.Ordering.Infrastructure.Repositories;

/// <summary>
/// 订单仓储实现类，负责处理订单的持久化操作
/// </summary>
/// <remarks>
/// 该类实现IOrderRepository接口，提供订单的增删改查功能
/// 并通过OrderingContext实现对Entity Framework Core的集成
/// </remarks>
public class OrderRepository
    : IOrderRepository
{
    private readonly OrderingContext _context;

    /// <summary>
    /// 获取工作单元接口实现，用于管理事务和持久化操作
    /// </summary>
    public IUnitOfWork UnitOfWork => _context;

    /// <summary>
    /// 构造函数，通过依赖注入接收数据上下文
    /// </summary>
    /// <param name="context">订单数据上下文</param>
    /// <exception cref="ArgumentNullException">当context为null时抛出</exception>
    public OrderRepository(OrderingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 添加新订单到数据库
    /// </summary>
    /// <param name="order">要添加的订单实体</param>
    /// <returns>添加后的订单实体（包含生成的Id）</returns>
    public Order Add(Order order)
    {
        return _context.Orders.Add(order).Entity;
    }

    /// <summary>
    /// 根据订单ID异步获取订单及其订单项
    /// </summary>
    /// <param name="orderId">要查询的订单ID</param>
    /// <returns>查询到的订单实体，如果不存在则返回null</returns>
    /// <remarks>
    /// 该方法使用显式加载模式加载订单项集合
    /// </remarks>
    public async Task<Order> GetAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order != null)
        {
            await _context.Entry(order)
                .Collection(i => i.OrderItems).LoadAsync();
        }

        return order;
    }

    /// <summary>
    /// 更新现有订单实体
    /// </summary>
    /// <param name="order">包含更新内容的订单实体</param>
    /// <remarks>
    /// 该方法将订单实体状态标记为已修改，以便在SaveChanges时更新到数据库
    /// </remarks>
    public void Update(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
    }
}
