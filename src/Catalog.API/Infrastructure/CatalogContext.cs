namespace eShop.Catalog.API.Infrastructure;

/// <remarks>
/// Add migrations using the following command inside the 'Catalog.API' project directory:
///
/// dotnet ef migrations add --context CatalogContext [migration-name]
/// </remarks>
public class CatalogContext : DbContext
{
    // 构造函数接收DbContext配置选项和应用配置
    public CatalogContext(DbContextOptions<CatalogContext> options, IConfiguration configuration) : base(options)
    {
        // 将配置选项传递给基类DbContext
    }

    // 定义数据库实体集
    public DbSet<CatalogItem> CatalogItems { get; set; } // 商品项表
    public DbSet<CatalogBrand> CatalogBrands { get; set; } // 品牌表
    public DbSet<CatalogType> CatalogTypes { get; set; } // 商品类型表

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 启用PostgreSQL的vector扩展，用于支持向量数据类型（可能用于搜索或AI功能）
        builder.HasPostgresExtension("vector");

        // 应用各实体的配置，使用单独的配置类来管理每个实体的映射设置
        builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());

        // 添加集成事件日志表到上下文，用于事件溯源或事件驱动架构
        builder.UseIntegrationEventLogs();
    }
}
