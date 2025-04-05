// 创建一个 ASP.NET Core Web 应用程序的构建器实例
var builder = WebApplication.CreateBuilder(args);

// 添加基本服务默认设置，通常包括日志记录、配置、健康检查等基础服务
builder.AddBasicServiceDefaults();
// 添加应用程序特定的服务，例如仓储、业务逻辑等自定义服务
builder.AddApplicationServices();

// 将 gRPC 服务框架添加到依赖注入容器
// gRPC 是一种高性能、跨平台的远程过程调用框架
builder.Services.AddGrpc();

// 使用配置好的服务构建 Web 应用程序实例
var app = builder.Build();

// 映射默认端点，比如健康检查端点、监控端点等
app.MapDefaultEndpoints();

// 将 BasketService 类映射为 gRPC 服务
// 这允许客户端通过 gRPC 协议访问 BasketService 提供的功能
app.MapGrpcService<BasketService>();

// 启动应用程序并开始监听请求
app.Run();
