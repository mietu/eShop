namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// Entity Framework Core 的实体类型配置类，用于配置 CatalogBrand 实体的数据库映射
/// </summary>
class CatalogBrandEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogBrand>
{
    /// <summary>
    /// 配置 CatalogBrand 实体的数据库映射
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<CatalogBrand> builder)
    {
        // 指定实体映射到的数据库表名
        builder.ToTable("CatalogBrand");

        // 配置 Brand 属性的最大长度为 100 个字符
        builder.Property(cb => cb.Brand)
            .HasMaxLength(100);
    }
}
