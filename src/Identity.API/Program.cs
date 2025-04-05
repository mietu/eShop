// 创建Web应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认设置（日志、健康检查等通用服务）
builder.AddServiceDefaults();

// 添加MVC控制器和视图支持
builder.Services.AddControllersWithViews();

// 配置PostgreSQL数据库上下文
builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");

// 应用数据库迁移自动运行
// 注意：此方法不建议用于生产环境，生产环境应考虑从迁移生成SQL脚本
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();

// 配置ASP.NET Core Identity服务
// 使用ApplicationUser作为用户类型，IdentityRole作为角色类型
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>() // 使用EF Core存储用户数据
        .AddDefaultTokenProviders(); // 添加默认令牌提供程序（用于重置密码等）

// 配置IdentityServer身份认证服务
builder.Services.AddIdentityServer(options =>
{
    //options.IssuerUri = "null";
    // 设置身份认证Cookie生命周期为2小时
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

    // 启用各种事件通知，便于调试和监控
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // TODO: 生产环境中移除此行，应该启用密钥管理
    options.KeyManagement.Enabled = false;
})
// 添加内存中的身份资源配置
.AddInMemoryIdentityResources(Config.GetResources())
// 添加API作用域配置
.AddInMemoryApiScopes(Config.GetApiScopes())
// 添加API资源配置
.AddInMemoryApiResources(Config.GetApis())
// 添加客户端配置
.AddInMemoryClients(Config.GetClients(builder.Configuration))
// 集成ASP.NET Core Identity
.AddAspNetIdentity<ApplicationUser>()
// 添加开发者签名凭证（仅用于开发环境）
// TODO: 生产环境不推荐使用此方法 - 需要在安全位置存储密钥材料
.AddDeveloperSigningCredential();

// 注册自定义服务
// 用户资料服务
builder.Services.AddTransient<IProfileService, ProfileService>();
// 登录服务
builder.Services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
// 重定向服务
builder.Services.AddTransient<IRedirectService, RedirectService>();

// 构建应用程序
var app = builder.Build();

// 映射默认健康检查和其他基础端点
app.MapDefaultEndpoints();

// 启用静态文件服务
app.UseStaticFiles();

// 配置Cookie策略以解决Chrome 80+版本使用HTTP时的登录问题
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
// 配置路由中间件
app.UseRouting();
// 启用IdentityServer中间件
app.UseIdentityServer();
// 启用授权中间件
app.UseAuthorization();

// 映射默认控制器路由
app.MapDefaultControllerRoute();

// 运行应用程序
app.Run();
