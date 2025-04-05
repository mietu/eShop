namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// Entity Framework Core 的实体类型配置类，用于配置 CatalogType 实体在数据库中的映射关系
/// </summary>
class CatalogTypeEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogType>
{
    /// <summary>
    /// 配置 CatalogType 实体的数据库映射
    /// </summary>
    /// <param name="builder">用于配置实体类型的构建器</param>
    public void Configure(EntityTypeBuilder<CatalogType> builder)
    {
        // 将实体映射到数据库中名为 "CatalogType" 的表
        builder.ToTable("CatalogType");

        // 配置 Type 属性的最大长度为 100 个字符
        builder.Property(cb => cb.Type)
            .HasMaxLength(100);

        // 注：此配置未设置主键，因为 EF Core 默认会将名为 Id 的属性识别为主键
    }
}
