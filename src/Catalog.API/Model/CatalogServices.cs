using eShop.Catalog.API.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 提供目录服务所需的依赖项集合，作为服务容器使用
/// </summary>
/// <remarks>
/// 该类封装了目录相关操作所需的所有核心服务，
/// 便于在应用中统一注入和管理目录服务依赖
/// </remarks>
public class CatalogServices(
    CatalogContext context,
    [FromServices] ICatalogAI catalogAI,
    IOptions<CatalogOptions> options,
    ILogger<CatalogServices> logger,
    [FromServices] ICatalogIntegrationEventService eventService)
{
    /// <summary>
    /// 获取目录数据上下文，用于数据库操作
    /// </summary>
    public CatalogContext Context { get; } = context;

    /// <summary>
    /// 获取目录AI服务，用于商品嵌入和智能推荐
    /// </summary>
    public ICatalogAI CatalogAI { get; } = catalogAI;

    /// <summary>
    /// 获取目录配置选项，包含图片URL和自定义数据设置
    /// </summary>
    public IOptions<CatalogOptions> Options { get; } = options;

    /// <summary>
    /// 获取日志记录器，用于记录目录服务的操作日志
    /// </summary>
    public ILogger<CatalogServices> Logger { get; } = logger;

    /// <summary>
    /// 获取目录集成事件服务，用于发布和处理目录相关事件
    /// </summary>
    public ICatalogIntegrationEventService EventService { get; } = eventService;
};
