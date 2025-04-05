using System.Diagnostics;

namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// 提供用于数据库迁移和种子数据初始化的扩展方法
/// </summary>
internal static class MigrateDbContextExtensions
{
    // 用于OpenTelemetry跟踪的活动源名称
    private static readonly string ActivitySourceName = "DbMigrations";
    // 创建用于跟踪数据库迁移操作的活动源
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    /// <summary>
    /// 添加数据库迁移服务，不执行任何种子数据初始化
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddMigration<TContext>((_, _) => Task.CompletedTask);

    /// <summary>
    /// 添加数据库迁移服务，并使用指定的种子数据初始化函数
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="seeder">种子数据初始化函数</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services, Func<TContext, IServiceProvider, Task> seeder)
        where TContext : DbContext
    {
        // 启用迁移操作的OpenTelemetry跟踪
        services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        // 注册执行数据库迁移的后台服务
        return services.AddHostedService(sp => new MigrationHostedService<TContext>(sp, seeder));
    }

    /// <summary>
    /// 添加数据库迁移服务，并使用实现了IDbSeeder接口的类来初始化种子数据
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    /// <typeparam name="TDbSeeder">种子数据初始化器的类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        // 注册种子数据初始化器
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
        // 使用注册的初始化器执行种子数据初始化
        return services.AddMigration<TContext>((context, sp) => sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context));
    }

    /// <summary>
    /// 执行数据库迁移和种子数据初始化
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    /// <param name="services">服务提供者</param>
    /// <param name="seeder">种子数据初始化函数</param>
    /// <returns>表示异步操作的任务</returns>
    private static async Task MigrateDbContextAsync<TContext>(this IServiceProvider services, Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
    {
        // 创建新的作用域
        using var scope = services.CreateScope();
        var scopeServices = scope.ServiceProvider;
        var logger = scopeServices.GetRequiredService<ILogger<TContext>>();
        var context = scopeServices.GetService<TContext>();

        // 创建迁移操作的活动跟踪
        using var activity = ActivitySource.StartActivity($"迁移操作{typeof(TContext).Name}");

        try
        {
            logger.LogInformation("迁移与{DbContextName}关联的数据库", typeof(TContext).Name);

            // 创建执行策略，处理暂时性错误
            var strategy = context.Database.CreateExecutionStrategy();

            // 使用执行策略执行迁移和种子数据初始化
            await strategy.ExecuteAsync(() => InvokeSeeder(seeder, context, scopeServices));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "迁移上下文{Db Context Name}上使用的数据库时出错", typeof(TContext).Name);

            // 在活动跟踪中记录异常信息
            activity.SetExceptionTags(ex);

            throw;
        }
    }

    /// <summary>
    /// 执行数据库迁移和种子数据初始化
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    /// <param name="seeder">种子数据初始化函数</param>
    /// <param name="context">数据库上下文</param>
    /// <param name="services">服务提供者</param>
    /// <returns>表示异步操作的任务</returns>
    private static async Task InvokeSeeder<TContext>(Func<TContext, IServiceProvider, Task> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
    {
        // 创建迁移特定上下文的活动跟踪
        using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            // 应用待处理的迁移
            await context.Database.MigrateAsync();
            // 执行种子数据初始化
            await seeder(context, services);
        }
        catch (Exception ex)
        {
            // 在活动跟踪中记录异常信息
            activity.SetExceptionTags(ex);

            throw;
        }
    }

    /// <summary>
    /// 执行数据库迁移的后台服务
    /// </summary>
    /// <typeparam name="TContext">要迁移的数据库上下文类型</typeparam>
    private class MigrationHostedService<TContext>(IServiceProvider serviceProvider, Func<TContext, IServiceProvider, Task> seeder)
        : BackgroundService where TContext : DbContext
    {
        /// <summary>
        /// 服务启动时执行数据库迁移
        /// </summary>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return serviceProvider.MigrateDbContextAsync(seeder);
        }

        /// <summary>
        /// 执行后台服务主体逻辑 - 在这里不需要执行任何操作，因为迁移在StartAsync中已完成
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
/// <summary>
/// 定义数据库种子数据初始化器的接口
/// </summary>
/// <typeparam name="TContext">数据库上下文类型</typeparam>
public interface IDbSeeder<in TContext> where TContext : DbContext
{
    /// <summary>
    /// 执行数据库种子数据初始化
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <returns>表示异步操作的任务</returns>
    Task SeedAsync(TContext context);
}
