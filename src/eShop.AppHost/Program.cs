using eShop.AppHost;

// 创建一个分布式应用程序构建器
var builder = DistributedApplication.CreateBuilder(args);

// 添加转发的HTTP头，用于处理代理和负载均衡环境
builder.AddForwardedHeaders();

// 添加Redis资源
var redis = builder.AddRedis("redis");
// 添加RabbitMQ消息队列，设置为持久容器（不会随应用停止而删除）
var rabbitMq = builder.AddRabbitMQ("eventbus")
    .WithLifetime(ContainerLifetime.Persistent);
// 添加PostgreSQL数据库，使用支持向量操作的pgvector镜像，设置为持久容器
var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

// 在PostgreSQL中创建4个不同用途的数据库
var catalogDb = postgres.AddDatabase("catalogdb");     // 商品目录数据库
var identityDb = postgres.AddDatabase("identitydb");   // 身份认证数据库
var orderDb = postgres.AddDatabase("orderingdb");      // 订单数据库
var webhooksDb = postgres.AddDatabase("webhooksdb");   // Webhook数据库

// 根据环境决定使用HTTP还是HTTPS
var launchProfileName = ShouldUseHttpForEndpoints() ? "http" : "https";

// ===== 服务配置 =====
// 配置身份认证API
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api", launchProfileName)
    .WithExternalHttpEndpoints()  // 允许外部访问HTTP端点
    .WithReference(identityDb);   // 关联身份数据库

var identityEndpoint = identityApi.GetEndpoint(launchProfileName);

// 配置购物篮API
var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)         // 关联Redis（用于购物篮数据缓存）
    .WithReference(rabbitMq).WaitFor(rabbitMq)  // 关联消息队列并等待其就绪
    .WithEnvironment("Identity__Url", identityEndpoint);  // 设置身份服务URL
redis.WithParentRelationship(basketApi);  // 设置Redis与购物篮API的父子关系

// 配置商品目录API
var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(rabbitMq).WaitFor(rabbitMq)  // 关联消息队列并等待其就绪
    .WithReference(catalogDb);                  // 关联商品目录数据库

// 配置订单API
var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq).WaitFor(rabbitMq)        // 关联消息队列并等待其就绪
    .WithReference(orderDb).WaitFor(orderDb)          // 关联订单数据库并等待其就绪
    .WithHttpHealthCheck("/health")                   // 设置健康检查端点
    .WithEnvironment("Identity__Url", identityEndpoint);  // 设置身份服务URL

// 配置订单处理器服务
builder.AddProject<Projects.OrderProcessor>("order-processor")
    .WithReference(rabbitMq).WaitFor(rabbitMq)  // 关联消息队列并等待其就绪
    .WithReference(orderDb)                     // 关联订单数据库
    .WaitFor(orderingApi);                      // 等待订单API就绪（因为订单API包含EF迁移）

// 配置支付处理器服务
builder.AddProject<Projects.PaymentProcessor>("payment-processor")
    .WithReference(rabbitMq).WaitFor(rabbitMq);  // 关联消息队列并等待其就绪

// 配置WebhooksAPI
var webHooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(rabbitMq).WaitFor(rabbitMq)        // 关联消息队列并等待其就绪
    .WithReference(webhooksDb)                        // 关联Webhook数据库
    .WithEnvironment("Identity__Url", identityEndpoint);  // 设置身份服务URL

// ===== 反向代理配置 =====
// 配置移动后端转发（BFF模式 - Backend For Frontend）
builder.AddProject<Projects.Mobile_Bff_Shopping>("mobile-bff")
    .WithReference(catalogApi)   // 关联商品目录API
    .WithReference(orderingApi)  // 关联订单API
    .WithReference(basketApi)    // 关联购物篮API
    .WithReference(identityApi); // 关联身份API

// ===== 应用配置 =====
// 配置Webhook客户端
var webhooksClient = builder.AddProject<Projects.WebhookClient>("webhooksclient", launchProfileName)
    .WithReference(webHooksApi)                      // 关联WebhooksAPI
    .WithEnvironment("IdentityUrl", identityEndpoint);  // 设置身份服务URL

// 配置Web应用（主要Web UI）
var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName)
    .WithExternalHttpEndpoints()                     // 允许外部访问HTTP端点
    .WithReference(basketApi)                        // 关联购物篮API
    .WithReference(catalogApi)                       // 关联商品目录API
    .WithReference(orderingApi)                      // 关联订单API
    .WithReference(rabbitMq).WaitFor(rabbitMq)       // 关联消息队列并等待其就绪
    .WithEnvironment("IdentityUrl", identityEndpoint);  // 设置身份服务URL

// ===== AI集成配置 =====
// 设置为true以启用OpenAI集成
bool useOpenAI = false;
if (useOpenAI)
{
    builder.AddOpenAI(catalogApi, webApp);  // 将OpenAI集成到目录API和Web应用
}

// 设置为true以启用Ollama（开源AI模型）集成
bool useOllama = false;
if (useOllama)
{
    builder.AddOllama(catalogApi, webApp);  // 将Ollama集成到目录API和Web应用
}

// ===== 回调URL配置 =====
// 配置应用的自引用回调URL
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint(launchProfileName));
webhooksClient.WithEnvironment("CallBackUrl", webhooksClient.GetEndpoint(launchProfileName));

// 为身份API配置所有相关服务的客户端URL（这是一个循环引用）
identityApi.WithEnvironment("BasketApiClient", basketApi.GetEndpoint("http"))
           .WithEnvironment("OrderingApiClient", orderingApi.GetEndpoint("http"))
           .WithEnvironment("WebhooksApiClient", webHooksApi.GetEndpoint("http"))
           .WithEnvironment("WebhooksWebClient", webhooksClient.GetEndpoint(launchProfileName))
           .WithEnvironment("WebAppClient", webApp.GetEndpoint(launchProfileName));

// 构建并运行应用
builder.Build().Run();

// 仅用于测试
// 检查环境变量以决定是否对所有端点强制使用HTTP
// 主要用于在CI环境中更容易运行Playwright测试
static bool ShouldUseHttpForEndpoints()
{
    const string EnvVarName = "ESHOP_USE_HTTP_ENDPOINTS";
    var envValue = Environment.GetEnvironmentVariable(EnvVarName);

    // 尝试解析环境变量值；如果值恰好为"1"，则返回true
    return int.TryParse(envValue, out int result) && result == 1;
}
