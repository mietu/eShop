// 创建 Web 应用程序构建器实例
var builder = WebApplication.CreateBuilder(args);

// 添加基本服务默认配置，包括日志、遥测、健康检查等
builder.AddBasicServiceDefaults();
// 添加应用程序特定的服务（在别处定义的扩展方法）
builder.AddApplicationServices();

// 构建应用程序实例
var app = builder.Build();

// 映射默认端点，如健康检查、指标等
app.MapDefaultEndpoints();

// 异步启动应用程序并等待其完成
await app.RunAsync();
