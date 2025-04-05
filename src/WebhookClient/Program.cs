// 创建Web应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认值，这通常包括健康检查、日志记录和遥测等标准服务
builder.AddServiceDefaults();

// 添加Razor组件服务，并启用交互式服务器组件功能，用于Blazor应用程序
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// 添加应用程序特定的服务（自定义服务注册）
builder.AddApplicationServices();

// 构建应用程序实例
var app = builder.Build();

// 映射默认端点，如健康检查端点
app.MapDefaultEndpoints();

// 配置HTTP请求管道
if (!app.Environment.IsDevelopment())
{
    // 在非开发环境中使用异常处理中间件
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // 启用HTTP严格传输安全(HSTS)，提高安全性
    // 默认HSTS值为30天。对于生产场景，您可能需要更改此设置，请参阅 https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

// 启用防跨站请求伪造(CSRF/XSRF)保护
app.UseAntiforgery();

// 启用静态文件服务，如CSS、JavaScript和图像文件
app.UseStaticFiles();

// 映射Razor组件，并添加交互式服务器渲染模式，支持Blazor交互性
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// 映射身份验证相关的端点
app.MapAuthenticationEndpoints();

// 映射Webhook相关的端点
app.MapWebhookEndpoints();

// 运行应用程序
app.Run();
