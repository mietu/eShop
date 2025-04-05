// 创建 ASP.NET Core 应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认设置（如健康检查、日志记录等）
builder.AddServiceDefaults();

// 添加应用程序特定的服务（如数据库连接、仓储、业务逻辑服务等）
builder.AddApplicationServices();

// 添加问题详情服务，用于标准化错误响应格式
builder.Services.AddProblemDetails();

// 配置 API 版本控制，允许同时支持多个 API 版本
var withApiVersioning = builder.Services.AddApiVersioning();

// 添加默认的 OpenAPI（Swagger）配置，并集成 API 版本控制
builder.AddDefaultOpenApi(withApiVersioning);

// 构建应用程序实例
var app = builder.Build();

// 映射默认端点（如健康检查端点等）
app.MapDefaultEndpoints();

// 创建新的版本化 API 组，用于订单相关接口
var orders = app.NewVersionedApi("Orders");

// 映射订单 API 的 V1 版本端点，并要求授权访问
orders.MapOrdersApiV1()
      .RequireAuthorization();

// 启用默认的 OpenAPI 中间件，提供 Swagger UI 和 JSON 文档
app.UseDefaultOpenApi();

// 启动应用程序并处理 HTTP 请求
app.Run();
