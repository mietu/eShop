using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Pgvector.EntityFrameworkCore;

namespace eShop.Catalog.API;

/// <summary>
/// 提供电子商城目录服务的API端点。包含商品查询、创建、更新和删除等功能。
/// 支持API版本控制，同时提供v1和v2版本的端点。
/// </summary>
public static class CatalogApi
{
    /// <summary>
    /// 向应用程序注册目录API的所有端点。
    /// </summary>
    /// <param name="app">应用程序路由构建器</param>
    /// <returns>配置后的应用程序路由构建器</returns>
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
    {
        // 创建版本化API分组
        var vApi = app.NewVersionedApi("Catalog");
        var api = vApi.MapGroup("api/catalog").HasApiVersion(1, 0).HasApiVersion(2, 0);
        var v1 = vApi.MapGroup("api/catalog").HasApiVersion(1, 0);
        var v2 = vApi.MapGroup("api/catalog").HasApiVersion(2, 0);

        // 查询商品的路由
        v1.MapGet("/items", GetAllItemsV1)
            .WithName("ListItems")
            .WithSummary("列出商品")
            .WithDescription("获取目录中商品的分页列表。")
            .WithTags("Items");
        v2.MapGet("/items", GetAllItems)
            .WithName("ListItems-V2")
            .WithSummary("列出商品")
            .WithDescription("获取目录中商品的分页列表。")
            .WithTags("Items");
        api.MapGet("/items/by", GetItemsByIds)
            .WithName("BatchGetItems")
            .WithSummary("批量获取商品")
            .WithDescription("从目录中获取多个商品")
            .WithTags("Items");
        api.MapGet("/items/{id:int}", GetItemById)
            .WithName("GetItem")
            .WithSummary("获取商品")
            .WithDescription("从目录中获取一个商品")
            .WithTags("Items");
        v1.MapGet("/items/by/{name:minlength(1)}", GetItemsByName)
            .WithName("GetItemsByName")
            .WithSummary("按名称获取商品")
            .WithDescription("获取具有指定名称的商品的分页列表。")
            .WithTags("Items");
        api.MapGet("/items/{id:int}/pic", GetItemPictureById)
            .WithName("GetItemPicture")
            .WithSummary("获取商品图片")
            .WithDescription("获取商品的图片")
            .WithTags("Items");

        // 使用AI解析商品的路由
        v1.MapGet("/items/withsemanticrelevance/{text:minlength(1)}", GetItemsBySemanticRelevanceV1)
            .WithName("GetRelevantItems")
            .WithSummary("搜索相关商品")
            .WithDescription("搜索与指定文本相关的商品")
            .WithTags("Search");

        // 使用AI解析商品的路由（V2版本）
        v2.MapGet("/items/withsemanticrelevance", GetItemsBySemanticRelevance)
            .WithName("GetRelevantItems-V2")
            .WithSummary("搜索相关商品")
            .WithDescription("搜索与指定文本相关的商品")
            .WithTags("Search");

        // 按类型和品牌查询商品的路由
        v1.MapGet("/items/type/{typeId}/brand/{brandId?}", GetItemsByBrandAndTypeId)
            .WithName("GetItemsByTypeAndBrand")
            .WithSummary("按类型和品牌获取商品")
            .WithDescription("获取指定类型和品牌的商品")
            .WithTags("Types");
        v1.MapGet("/items/type/all/brand/{brandId:int?}", GetItemsByBrandId)
            .WithName("GetItemsByBrand")
            .WithSummary("按品牌列出商品")
            .WithDescription("获取指定品牌的商品列表")
            .WithTags("Brands");
        api.MapGet("/catalogtypes",
            [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
        async (CatalogContext context) => await context.CatalogTypes.OrderBy(x => x.Type).ToListAsync())
            .WithName("ListItemTypes")
            .WithSummary("列出商品类型")
            .WithDescription("获取商品类型列表")
            .WithTags("Types");
        api.MapGet("/catalogbrands",
            [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
        async (CatalogContext context) => await context.CatalogBrands.OrderBy(x => x.Brand).ToListAsync())
            .WithName("ListItemBrands")
            .WithSummary("列出商品品牌")
            .WithDescription("获取商品品牌列表")
            .WithTags("Brands");

        // 修改商品的路由
        v1.MapPut("/items", UpdateItemV1)
            .WithName("UpdateItem")
            .WithSummary("创建或替换商品")
            .WithDescription("创建或替换商品")
            .WithTags("Items");
        v2.MapPut("/items/{id:int}", UpdateItem)
            .WithName("UpdateItem-V2")
            .WithSummary("创建或替换商品")
            .WithDescription("创建或替换商品")
            .WithTags("Items");
        api.MapPost("/items", CreateItem)
            .WithName("CreateItem")
            .WithSummary("创建商品")
            .WithDescription("在目录中创建新商品");
        api.MapDelete("/items/{id:int}", DeleteItemById)
            .WithName("DeleteItem")
            .WithSummary("删除商品")
            .WithDescription("删除指定的商品");

        return app;
    }

    /// <summary>
    /// 获取所有商品的分页列表（V1版本）
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <returns>商品分页列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetAllItemsV1(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services)
    {
        return await GetAllItems(paginationRequest, services, null, null, null);
    }

    /// <summary>
    /// 获取所有商品的分页列表（V2版本），支持按名称、类型和品牌筛选
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="name">商品名称筛选条件</param>
    /// <param name="type">商品类型筛选条件</param>
    /// <param name="brand">商品品牌筛选条件</param>
    /// <returns>商品分页列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetAllItems(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("要返回的商品名称")] string name,
        [Description("要返回的商品类型")] int? type,
        [Description("要返回的商品品牌")] int? brand)
    {
        var pageSize = paginationRequest.PageSize;
        var pageIndex = paginationRequest.PageIndex;

        var root = (IQueryable<CatalogItem>)services.Context.CatalogItems;

        if (name is not null)
        {
            root = root.Where(c => c.Name.StartsWith(name));
        }
        if (type is not null)
        {
            root = root.Where(c => c.CatalogTypeId == type);
        }
        if (brand is not null)
        {
            root = root.Where(c => c.CatalogBrandId == brand);
        }

        var totalItems = await root
            .LongCountAsync();

        var itemsOnPage = await root
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    /// <summary>
    /// 根据多个ID获取商品
    /// </summary>
    /// <param name="services">目录服务</param>
    /// <param name="ids">要返回的商品ID列表</param>
    /// <returns>商品列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<List<CatalogItem>>> GetItemsByIds(
        [AsParameters] CatalogServices services,
        [Description("要返回的商品ID列表")] int[] ids)
    {
        var items = await services.Context.CatalogItems.Where(item => ids.Contains(item.Id)).ToListAsync();
        return TypedResults.Ok(items);
    }

    /// <summary>
    /// 根据ID获取单个商品
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="services">目录服务</param>
    /// <param name="id">商品ID</param>
    /// <returns>商品详情或错误响应</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<Ok<CatalogItem>, NotFound, BadRequest<ProblemDetails>>> GetItemById(
        HttpContext httpContext,
        [AsParameters] CatalogServices services,
        [Description("商品ID")] int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest<ProblemDetails>(new()
            {
                Detail = "Id不合法"
            });
        }

        var item = await services.Context.CatalogItems.Include(ci => ci.CatalogBrand).SingleOrDefaultAsync(ci => ci.Id == id);

        if (item == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(item);
    }

    /// <summary>
    /// 根据名称获取商品
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="name">商品名称</param>
    /// <returns>商品分页列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByName(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("要返回的商品名称")] string name)
    {
        return await GetAllItems(paginationRequest, services, name, null, null);
    }

    /// <summary>
    /// 获取商品图片
    /// </summary>
    /// <param name="context">目录上下文</param>
    /// <param name="environment">Web主机环境</param>
    /// <param name="id">商品ID</param>
    /// <returns>图片文件或404响应</returns>
    [ProducesResponseType<byte[]>(StatusCodes.Status200OK, "application/octet-stream",
        [ "image/png", "image/gif", "image/jpeg", "image/bmp", "image/tiff",
                  "image/wmf", "image/jp2", "image/svg+xml", "image/webp" ])]
    public static async Task<Results<PhysicalFileHttpResult, NotFound>> GetItemPictureById(
        CatalogContext context,
        IWebHostEnvironment environment,
        [Description("商品ID")] int id)
    {
        var item = await context.CatalogItems.FindAsync(id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        var path = GetFullPath(environment.ContentRootPath, item.PictureFileName);

        string imageFileExtension = Path.GetExtension(item.PictureFileName);
        string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        DateTime lastModified = File.GetLastWriteTimeUtc(path);

        return TypedResults.PhysicalFile(path, mimetype, lastModified: lastModified);
    }

    /// <summary>
    /// 通过语义相关性搜索商品（V1版本）
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="text">用于搜索相关商品的文本</param>
    /// <returns>商品分页列表或重定向结果</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, RedirectToRouteHttpResult>> GetItemsBySemanticRelevanceV1(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("用于搜索目录中相关商品的文本字符串")] string text)

    {
        return await GetItemsBySemanticRelevance(paginationRequest, services, text);
    }

    /// <summary>
    /// 通过语义相关性搜索商品（V2版本）
    /// 使用向量搜索（余弦距离）查找与输入文本相关性最高的商品
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="text">用于搜索相关商品的文本</param>
    /// <returns>商品分页列表或重定向结果</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, RedirectToRouteHttpResult>> GetItemsBySemanticRelevance(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("用于搜索目录中相关商品的文本字符串"), Required, MinLength(1)] string text)
    {
        var pageSize = paginationRequest.PageSize;
        var pageIndex = paginationRequest.PageIndex;

        if (!services.CatalogAI.IsEnabled)
        {
            return await GetItemsByName(paginationRequest, services, text);
        }

        // 为输入搜索创建嵌入向量
        var vector = await services.CatalogAI.GetEmbeddingAsync(text);

        // 获取商品总数
        var totalItems = await services.Context.CatalogItems
            .LongCountAsync();

        // 获取下一页商品，按与输入搜索的相似度排序（最小距离）
        List<CatalogItem> itemsOnPage;
        if (services.Logger.IsEnabled(LogLevel.Debug))
        {
            var itemsWithDistance = await services.Context.CatalogItems
                .Select(c => new { Item = c, Distance = c.Embedding.CosineDistance(vector) })
                .OrderBy(c => c.Distance)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            services.Logger.LogDebug("从{text}中获取的结果{results}", text, string.Join(", ", itemsWithDistance.Select(i => $"{i.Item.Name} => {i.Distance}")));

            itemsOnPage = itemsWithDistance.Select(i => i.Item).ToList();
        }
        else
        {
            itemsOnPage = await services.Context.CatalogItems
                .OrderBy(c => c.Embedding.CosineDistance(vector))
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
        }

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    /// <summary>
    /// 根据品牌和类型ID获取商品
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="typeId">商品类型ID</param>
    /// <param name="brandId">商品品牌ID（可选）</param>
    /// <returns>商品分页列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandAndTypeId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("要返回的商品类型")] int typeId,
        [Description("要返回的商品品牌")] int? brandId)
    {
        return await GetAllItems(paginationRequest, services, null, typeId, brandId);
    }

    /// <summary>
    /// 根据品牌ID获取商品
    /// </summary>
    /// <param name="paginationRequest">分页请求参数</param>
    /// <param name="services">目录服务</param>
    /// <param name="brandId">商品品牌ID（可选）</param>
    /// <returns>商品分页列表</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [Description("要返回的商品品牌")] int? brandId)
    {
        return await GetAllItems(paginationRequest, services, null, null, brandId);
    }

    /// <summary>
    /// 更新商品（V1版本）
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="services">目录服务</param>
    /// <param name="productToUpdate">要更新的商品</param>
    /// <returns>创建结果或错误响应</returns>
    public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItemV1(
        HttpContext httpContext,
        [AsParameters] CatalogServices services,
        CatalogItem productToUpdate)
    {
        if (productToUpdate?.Id == null)
        {
            return TypedResults.BadRequest<ProblemDetails>(new()
            {
                Detail = "请求体中必须提供商品ID。"
            });
        }
        return await UpdateItem(httpContext, productToUpdate.Id, services, productToUpdate);
    }

    /// <summary>
    /// 更新商品（V2版本）
    /// 如果商品价格发生变化，将创建并发布价格变更集成事件
    /// </summary>
    /// <param name="httpContext">HTTP上下文</param>
    /// <param name="id">要更新的商品ID</param>
    /// <param name="services">目录服务</param>
    /// <param name="productToUpdate">要更新的商品数据</param>
    /// <returns>创建结果或错误响应</returns>
    public static async Task<Results<Created, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateItem(
        HttpContext httpContext,
        [Description("要删除的商品ID")] int id,
        [AsParameters] CatalogServices services,
        CatalogItem productToUpdate)
    {
        var catalogItem = await services.Context.CatalogItems.SingleOrDefaultAsync(i => i.Id == id);

        if (catalogItem == null)
        {
            return TypedResults.NotFound<ProblemDetails>(new()
            {
                Detail = $"未找到ID为{id}的商品。"
            });
        }

        // 更新当前商品
        var catalogEntry = services.Context.Entry(catalogItem);
        catalogEntry.CurrentValues.SetValues(productToUpdate);

        catalogItem.Embedding = await services.CatalogAI.GetEmbeddingAsync(catalogItem);

        var priceEntry = catalogEntry.Property(i => i.Price);

        if (priceEntry.IsModified) // 如果价格已更改，保存商品数据并通过事件总线发布集成事件
        {
            //创建要通过事件总线发布的集成事件
            var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, productToUpdate.Price, priceEntry.OriginalValue);

            // 通过本地事务在原始目录数据库操作和IntegrationEventLog之间实现原子性
            await services.EventService.SaveEventAndCatalogContextChangesAsync(priceChangedEvent);

            // 通过事件总线发布并将已保存的事件标记为已发布
            await services.EventService.PublishThroughEventBusAsync(priceChangedEvent);
        }
        else // 仅保存更新的商品，因为商品价格没有变化
        {
            await services.Context.SaveChangesAsync();
        }
        return TypedResults.Created($"/api/catalog/items/{id}");
    }

    /// <summary>
    /// 创建新商品
    /// </summary>
    /// <param name="services">目录服务</param>
    /// <param name="product">要创建的商品</param>
    /// <returns>创建结果</returns>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Created> CreateItem(
        [AsParameters] CatalogServices services,
        CatalogItem product)
    {
        var item = new CatalogItem
        {
            Id = product.Id,
            CatalogBrandId = product.CatalogBrandId,
            CatalogTypeId = product.CatalogTypeId,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price,
            AvailableStock = product.AvailableStock,
            RestockThreshold = product.RestockThreshold,
            MaxStockThreshold = product.MaxStockThreshold
        };
        item.Embedding = await services.CatalogAI.GetEmbeddingAsync(item);

        services.Context.CatalogItems.Add(item);
        await services.Context.SaveChangesAsync();

        return TypedResults.Created($"/api/catalog/items/{item.Id}");
    }

    /// <summary>
    /// 根据ID删除商品
    /// </summary>
    /// <param name="services">目录服务</param>
    /// <param name="id">要删除的商品ID</param>
    /// <returns>无内容结果或404响应</returns>
    public static async Task<Results<NoContent, NotFound>> DeleteItemById(
        [AsParameters] CatalogServices services,
        [Description("要删除的商品ID")] int id)
    {
        var item = services.Context.CatalogItems.SingleOrDefault(x => x.Id == id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        services.Context.CatalogItems.Remove(item);
        await services.Context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    /// <summary>
    /// 根据图片文件扩展名获取MIME类型
    /// </summary>
    /// <param name="extension">文件扩展名</param>
    /// <returns>对应的MIME类型</returns>
    private static string GetImageMimeTypeFromImageFileExtension(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };

    /// <summary>
    /// 获取图片文件的完整路径
    /// </summary>
    /// <param name="contentRootPath">内容根路径</param>
    /// <param name="pictureFileName">图片文件名</param>
    /// <returns>图片的完整路径</returns>
    public static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Pics", pictureFileName);
}
