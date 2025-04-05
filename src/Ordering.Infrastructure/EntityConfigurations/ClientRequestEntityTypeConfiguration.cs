namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 配置 ClientRequest 实体类型的映射关系
/// </summary>
class ClientRequestEntityTypeConfiguration
    : IEntityTypeConfiguration<ClientRequest>
{
    /// <summary>
    /// 配置 ClientRequest 实体映射到数据库表的方式
    /// </summary>
    /// <param name="requestConfiguration">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<ClientRequest> requestConfiguration)
    {
        requestConfiguration.ToTable("requests");

        // 配置主键
        requestConfiguration.HasKey(r => r.Id);

        // 配置属性
        requestConfiguration.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        requestConfiguration.Property(r => r.Time)
            .IsRequired();
    }
}
