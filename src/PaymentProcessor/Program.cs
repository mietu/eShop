// 创建Web应用程序构建器，用于配置应用程序服务和中间件
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认值，包括健康检查、日志记录和遥测等标准服务
builder.AddServiceDefaults();

// 配置RabbitMQ事件总线，用于处理微服务间的消息通信
// 添加对"订单状态变更为库存确认"事件的订阅以及对应的处理程序
builder.AddRabbitMqEventBus("EventBus")
    .AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();

// 注册PaymentOptions配置，从配置系统中绑定相关设置
// 这些设置可在appsettings.json或其他配置源中定义
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration(nameof(PaymentOptions));

// 构建应用程序实例
var app = builder.Build();

// 映射默认端点，如健康检查和度量等API端点
app.MapDefaultEndpoints();

// 异步运行应用程序，启动所有配置的服务
await app.RunAsync();
