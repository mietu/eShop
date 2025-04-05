/// <summary>
/// 提供用于配置应用程序服务的扩展方法
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 向依赖注入容器中添加应用程序所需的所有服务
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder实例</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // 添加认证服务到依赖注入容器
        builder.AddDefaultAuthentication();

        // 添加OrderingContext数据库上下文
        // 注意：禁用了DbContext池化，因为OrderingContext不满足池化的构造函数要求
        // DbContext池化要求类型具有接受单个DbContextOptions参数的公共构造函数
        services.AddDbContext<OrderingContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("orderingdb"));
        });
        // 使用扩展方法增强Npgsql DbContext配置
        builder.EnrichNpgsqlDbContext<OrderingContext>();

        // 添加数据库迁移和种子数据支持
        services.AddMigration<OrderingContext, OrderingContextSeed>();

        // 添加集成事件日志服务（使用OrderingContext）
        services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<OrderingContext>>();

        // 添加订单集成事件服务
        services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();

        // 添加RabbitMQ事件总线并配置事件订阅
        builder.AddRabbitMqEventBus("eventbus")
               .AddEventBusSubscriptions();

        // 添加HTTP上下文访问器，用于获取当前请求的HTTP上下文信息
        services.AddHttpContextAccessor();
        // 添加身份服务，用于处理用户身份验证和授权
        services.AddTransient<IIdentityService, IdentityService>();

        // 配置MediatR，用于实现CQRS模式和命令处理
        services.AddMediatR(cfg =>
        {
            // 从程序集注册所有处理程序
            cfg.RegisterServicesFromAssemblyContaining(typeof(Program));

            // 添加横切关注点行为
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));    // 日志记录行为
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));  // 验证行为
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>)); // 事务处理行为
        });

        // 注册命令验证器（基于FluentValidation库）
        services.AddSingleton<IValidator<CancelOrderCommand>, CancelOrderCommandValidator>();
        services.AddSingleton<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
        services.AddSingleton<IValidator<IdentifiedCommand<CreateOrderCommand, bool>>, IdentifiedCommandValidator>();
        services.AddSingleton<IValidator<ShipOrderCommand>, ShipOrderCommandValidator>();

        // 添加查询和仓储服务
        services.AddScoped<IOrderQueries, OrderQueries>();           // 订单查询服务
        services.AddScoped<IBuyerRepository, BuyerRepository>();     // 买家仓储
        services.AddScoped<IOrderRepository, OrderRepository>();     // 订单仓储
        services.AddScoped<IRequestManager, RequestManager>();       // 请求管理器（用于幂等性处理）
    }

    /// <summary>
    /// 向事件总线添加订阅，配置系统如何响应各种集成事件
    /// </summary>
    /// <param name="eventBus">事件总线构建器</param>
    private static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        // 订阅宽限期确认事件
        eventBus.AddSubscription<GracePeriodConfirmedIntegrationEvent, GracePeriodConfirmedIntegrationEventHandler>();
        // 订阅库存确认事件
        eventBus.AddSubscription<OrderStockConfirmedIntegrationEvent, OrderStockConfirmedIntegrationEventHandler>();
        // 订阅库存拒绝事件
        eventBus.AddSubscription<OrderStockRejectedIntegrationEvent, OrderStockRejectedIntegrationEventHandler>();
        // 订阅支付失败事件
        eventBus.AddSubscription<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();
        // 订阅支付成功事件
        eventBus.AddSubscription<OrderPaymentSucceededIntegrationEvent, OrderPaymentSucceededIntegrationEventHandler>();
    }
}
