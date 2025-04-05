// 创建Web应用程序构建器实例
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认值（如健康检查、遥测等通用服务配置）
builder.AddServiceDefaults();
// 添加应用程序特定的服务（如数据访问、业务逻辑等）
builder.AddApplicationServices();
// 添加ProblemDetails服务，用于标准化错误响应格式
builder.Services.AddProblemDetails();

// 配置API版本控制服务并获取配置对象以供后续使用
var withApiVersioning = builder.Services.AddApiVersioning();

// 添加默认的OpenAPI/Swagger配置，并传入API版本控制配置
builder.AddDefaultOpenApi(withApiVersioning);

// 构建应用程序实例
var app = builder.Build();

// 映射默认终结点（如健康检查等）
app.MapDefaultEndpoints();

// 使用状态码页中间件，为HTTP错误状态码提供友好的响应页面
app.UseStatusCodePages();

// 映射目录API相关的路由和终结点
app.MapCatalogApi();

// 启用默认的OpenAPI/Swagger UI
app.UseDefaultOpenApi();
// 运行应用程序
app.Run();
