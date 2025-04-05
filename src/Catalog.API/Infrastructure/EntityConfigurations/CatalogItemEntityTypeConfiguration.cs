namespace eShop.Catalog.API.Infrastructure.EntityConfigurations;

/// <summary>
/// Entity Framework Core 的 CatalogItem 实体类型配置
/// 用于定义 CatalogItem 实体在数据库中的映射规则
/// </summary>
class CatalogItemEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogItem>
{
    /// <summary>
    /// 配置 CatalogItem 实体的数据库映射
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        // 将实体映射到名为 "Catalog" 的数据库表
        builder.ToTable("Catalog");

        // 配置 Name 属性的最大长度为 50 个字符
        builder.Property(ci => ci.Name)
            .HasMaxLength(50);

        // 配置 Embedding 属性的列类型为 "vector(384)"
        // 这是为了支持向量搜索功能，使用特定的数据库列类型
        builder.Property(ci => ci.Embedding)
            .HasColumnType("vector(384)");

        // 配置 CatalogItem 与 CatalogBrand 之间的一对多关系
        // 一个 CatalogBrand 可以关联多个 CatalogItem
        builder.HasOne(ci => ci.CatalogBrand)
            .WithMany();

        // 配置 CatalogItem 与 CatalogType 之间的一对多关系
        // 一个 CatalogType 可以关联多个 CatalogItem
        builder.HasOne(ci => ci.CatalogType)
            .WithMany();

        // 在 Name 属性上创建索引以提高查询性能
        builder.HasIndex(ci => ci.Name);
    }
}
