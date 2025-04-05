using System.Text.Json;
using eShop.Catalog.API.Services;
using Pgvector;

namespace eShop.Catalog.API.Infrastructure;

/// <summary>
/// 目录上下文的种子数据初始化器，用于向数据库填充初始数据
/// 实现了IDbSeeder接口，定义数据种子功能
/// </summary>
public partial class CatalogContextSeed(
    IWebHostEnvironment env,               // 提供网站环境信息，包括文件路径
    IOptions<CatalogOptions> settings,     // 提供目录相关配置选项
    ICatalogAI catalogAI,                  // AI服务，用于生成嵌入向量
    ILogger<CatalogContextSeed> logger) : IDbSeeder<CatalogContext>
{
    /// <summary>
    /// 执行数据库种子数据初始化
    /// </summary>
    /// <param name="context">目录数据库上下文</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SeedAsync(CatalogContext context)
    {
        var useCustomizationData = settings.Value.UseCustomizationData;
        var contentRootPath = env.ContentRootPath;  // 获取应用程序内容根目录
        var picturePath = env.WebRootPath;          // 获取Web根目录

        // 解决Npgsql类型加载问题的临时解决方案
        // Workaround from https://github.com/npgsql/efcore.pg/issues/292#issuecomment-388608426
        context.Database.OpenConnection();
        ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypes();

        // 仅在目录项为空时执行种子初始化
        if (!context.CatalogItems.Any())
        {
            // 从JSON文件加载种子数据
            var sourcePath = Path.Combine(contentRootPath, "Setup", "catalog.json");
            var sourceJson = File.ReadAllText(sourcePath);
            var sourceItems = JsonSerializer.Deserialize<CatalogSourceEntry[]>(sourceJson);

            // 清除并重新填充品牌数据
            context.CatalogBrands.RemoveRange(context.CatalogBrands);
            await context.CatalogBrands.AddRangeAsync(sourceItems.Select(x => x.Brand).Distinct()
                .Select(brandName => new CatalogBrand { Brand = brandName }));
            logger.LogInformation("Seeded catalog with {NumBrands} brands", context.CatalogBrands.Count());

            // 清除并重新填充类型数据
            context.CatalogTypes.RemoveRange(context.CatalogTypes);
            await context.CatalogTypes.AddRangeAsync(sourceItems.Select(x => x.Type).Distinct()
                .Select(typeName => new CatalogType { Type = typeName }));
            logger.LogInformation("Seeded catalog with {NumTypes} types", context.CatalogTypes.Count());

            // 保存品牌和类型数据以获取它们的ID
            await context.SaveChangesAsync();

            // 创建品牌名称到ID和类型名称到ID的映射字典
            var brandIdsByName = await context.CatalogBrands.ToDictionaryAsync(x => x.Brand, x => x.Id);
            var typeIdsByName = await context.CatalogTypes.ToDictionaryAsync(x => x.Type, x => x.Id);

            // 将JSON源数据转换为CatalogItem对象数组
            var catalogItems = sourceItems.Select(source => new CatalogItem
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                Price = source.Price,
                CatalogBrandId = brandIdsByName[source.Brand],    // 使用字典查找品牌ID
                CatalogTypeId = typeIdsByName[source.Type],       // 使用字典查找类型ID
                AvailableStock = 100,                             // 初始库存设为100
                MaxStockThreshold = 200,                          // 最大库存阈值设为200
                RestockThreshold = 10,                            // 补货阈值设为10
                PictureFileName = $"{source.Id}.webp",            // 图片文件名基于ID生成
            }).ToArray();

            // 如果AI服务可用，为每个目录项生成嵌入向量
            if (catalogAI.IsEnabled)
            {
                logger.LogInformation("Generating {NumItems} embeddings", catalogItems.Length);
                IReadOnlyList<Vector> embeddings = await catalogAI.GetEmbeddingsAsync(catalogItems);
                for (int i = 0; i < catalogItems.Length; i++)
                {
                    catalogItems[i].Embedding = embeddings[i];
                }
            }

            // 添加所有目录项并保存更改
            await context.CatalogItems.AddRangeAsync(catalogItems);
            logger.LogInformation("Seeded catalog with {NumItems} items", context.CatalogItems.Count());
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 用于从JSON反序列化的内部类，表示目录项的源数据结构
    /// </summary>
    private class CatalogSourceEntry
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
