/// <summary>
/// 提供应用程序服务注册的扩展方法
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 向应用程序添加所需的服务
    /// </summary>
    /// <param name="builder">宿主应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 添加默认的身份验证服务
        builder.AddDefaultAuthentication();

        // 添加RabbitMQ事件总线，并配置事件订阅
        builder.AddRabbitMqEventBus("eventbus")
               .AddEventBusSubscriptions();

        // 添加PostgreSQL数据库上下文
        builder.AddNpgsqlDbContext<WebhooksContext>("webhooksdb");

        // 添加数据库迁移服务
        builder.Services.AddMigration<WebhooksContext>();

        // 注册应用服务
        builder.Services.AddTransient<IGrantUrlTesterService, GrantUrlTesterService>(); // URL授权测试服务
        builder.Services.AddTransient<IWebhooksRetriever, WebhooksRetriever>(); // Webhook检索服务
        builder.Services.AddTransient<IWebhooksSender, WebhooksSender>(); // Webhook发送服务
    }

    /// <summary>
    /// 为事件总线添加事件订阅
    /// </summary>
    /// <param name="eventBus">事件总线构建器</param>
    private static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        // 注册产品价格变更事件的处理器
        eventBus.AddSubscription<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();

        // 注册订单状态变为已发货事件的处理器
        eventBus.AddSubscription<OrderStatusChangedToShippedIntegrationEvent, OrderStatusChangedToShippedIntegrationEventHandler>();

        // 注册订单状态变为已付款事件的处理器
        eventBus.AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
    }
}
