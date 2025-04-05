// 创建Web应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认设置（如健康检查、遥测等）
builder.AddServiceDefaults();
// 添加应用程序特定服务（如数据库连接、应用服务等）
builder.AddApplicationServices();

// 配置API版本控制服务
var withApiVersioning = builder.Services.AddApiVersioning();

// 添加默认的OpenAPI/Swagger配置，传入API版本控制设置
builder.AddDefaultOpenApi(withApiVersioning);

// 构建Web应用程序实例
var app = builder.Build();

// 映射默认端点（如健康检查、状态等）
app.MapDefaultEndpoints();

// 创建一个新的版本化API组，命名为"Web Hooks"
var webHooks = app.NewVersionedApi("Web Hooks");

// 映射WebHooks API v1版本的所有端点
webHooks.MapWebHooksApiV1()
        // 要求对这些端点进行授权
        .RequireAuthorization();

// 启用默认的OpenAPI/Swagger中间件
app.UseDefaultOpenApi();
// 运行应用程序
app.Run();
