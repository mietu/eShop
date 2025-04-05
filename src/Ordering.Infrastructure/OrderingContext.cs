using eShop.IntegrationEventLogEF;

namespace eShop.Ordering.Infrastructure;

/// <summary>
/// 订单上下文类，负责订单领域实体的持久化和事务管理
/// 实现IUnitOfWork接口，支持工作单元模式
/// </summary>
/// <remarks>
/// Add migrations using the following command inside the 'Ordering.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]
/// </remarks>
public class OrderingContext : DbContext, IUnitOfWork
{
    /// <summary>
    /// 订单实体集合
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// 订单项实体集合
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// 支付方式实体集合
    /// </summary>
    public DbSet<PaymentMethod> Payments { get; set; }

    /// <summary>
    /// 买家实体集合
    /// </summary>
    public DbSet<Buyer> Buyers { get; set; }

    /// <summary>
    /// 卡类型实体集合
    /// </summary>
    public DbSet<CardType> CardTypes { get; set; }

    /// <summary>
    /// 中介者实例，用于分发领域事件
    /// </summary>
    private readonly IMediator _mediator;

    /// <summary>
    /// 当前活动事务
    /// </summary>
    private IDbContextTransaction _currentTransaction;

    /// <summary>
    /// 简单构造函数，仅接受DbContext选项
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public OrderingContext(DbContextOptions<OrderingContext> options) : base(options) { }

    /// <summary>
    /// 获取当前活动事务
    /// </summary>
    /// <returns>当前事务实例</returns>
    public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

    /// <summary>
    /// 判断是否存在活动事务
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// 完整构造函数，接受DbContext选项和中介者实例
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    /// <param name="mediator">中介者实例，用于处理领域事件</param>
    /// <exception cref="ArgumentNullException">当mediator为null时抛出</exception>
    public OrderingContext(DbContextOptions<OrderingContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));


        System.Diagnostics.Debug.WriteLine("OrderingContext::ctor ->" + this.GetHashCode());
    }

    /// <summary>
    /// 配置实体模型和关系
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");
        modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CardTypeEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BuyerEntityTypeConfiguration());
        modelBuilder.UseIntegrationEventLogs();
    }

    /// <summary>
    /// 异步保存所有实体更改并处理领域事件
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>如果所有操作成功完成则返回true</returns>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        await _mediator.DispatchDomainEventsAsync(this);

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        _ = await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 开始新的数据库事务
    /// </summary>
    /// <returns>新创建的事务，如果已存在活动事务则返回null</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    /// <summary>
    /// 提交指定的事务
    /// </summary>
    /// <param name="transaction">要提交的事务</param>
    /// <exception cref="ArgumentNullException">当transaction为null时抛出</exception>
    /// <exception cref="InvalidOperationException">当指定的事务不是当前事务时抛出</exception>
    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// 回滚当前事务
    /// </summary>
    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

#nullable enable
