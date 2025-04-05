namespace Webhooks.API.Infrastructure;

/// <summary>
/// Webhook 数据上下文，负责管理 Webhook 订阅数据的持久化
/// </summary>
/// <remarks>
/// 在 'Webhooks.API' 项目目录中使用以下命令添加迁移：
///
/// dotnet ef migrations add [migration-name]
/// </remarks>
public class WebhooksContext(DbContextOptions<WebhooksContext> options) : DbContext(options)
{
    /// <summary>
    /// 获取或设置 Webhook 订阅集合
    /// </summary>
    public DbSet<WebhookSubscription> Subscriptions { get; set; }

    /// <summary>
    /// 配置实体模型
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebhookSubscription>(eb =>
        {
            // 为 UserId 字段创建索引以提高查询性能
            eb.HasIndex(s => s.UserId);
            // 为 Type 字段创建索引以提高按类型查询的性能
            eb.HasIndex(s => s.Type);
        });
    }
}
