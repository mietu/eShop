namespace eShop.IntegrationEventLogEF;

/// <summary>
/// 提供用于配置集成事件日志实体的扩展方法
/// </summary>
public static class IntegrationLogExtensions
{
    /// <summary>
    /// 配置实体框架模型构建器，将IntegrationEventLogEntry映射到数据库表
    /// </summary>
    /// <param name="builder">实体框架模型构建器</param>
    /// <remarks>
    /// 此方法将IntegrationEventLogEntry实体映射到名为"IntegrationEventLog"的数据库表，
    /// 并配置EventId属性作为主键。通常在DbContext的OnModelCreating方法中调用。
    /// </remarks>
    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLogEntry>(builder =>
        {
            // 配置表名为"IntegrationEventLog"
            builder.ToTable("IntegrationEventLog");

            // 将EventId属性设置为主键
            builder.HasKey(e => e.EventId);
        });
    }
}
