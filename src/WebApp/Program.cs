// 导入必要的命名空间
// 创建Web应用程序构建器实例
var builder = WebApplication.CreateBuilder(args);

//添加HttpContextAccessor服务
//允许在应用程序中访问当前的HTTP上下文
//这对于在服务中获取请求信息或用户信息非常有用
builder.Services.AddHttpContextAccessor();

// 添加服务默认设置（如健康检查、遥测等）
builder.AddServiceDefaults();

// 添加Razor组件服务和交互式服务器组件功能
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// 添加应用程序特定的服务（自定义服务注册方法，定义在其他地方）
builder.AddApplicationServices();

// 构建应用程序实例
var app = builder.Build();

// 映射默认端点（如健康检查端点）
app.MapDefaultEndpoints();

// 配置HTTP请求管道
if (!app.Environment.IsDevelopment())
{
    // 在非开发环境中使用异常处理中间件
    app.UseExceptionHandler("/Error");
    // 启用HSTS（HTTP严格传输安全），增强安全性
    // HSTS指示浏览器只能通过HTTPS访问该站点
    app.UseHsts();
}

// 启用防跨站请求伪造(CSRF)保护
app.UseAntiforgery();

// 将HTTP请求重定向到HTTPS
app.UseHttpsRedirection();

// 启用静态文件服务（如CSS、JavaScript、图片等）
app.UseStaticFiles();

// 将Razor组件映射到请求管道，并添加交互式服务器渲染模式
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// 配置请求转发：将对/product-images/{id}的请求转发到catalog-api服务
// 这允许从外部服务获取产品图片，同时对客户端保持统一的URL结构
app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/catalog/items/{id}/pic");

// 启动应用程序
app.Run();
