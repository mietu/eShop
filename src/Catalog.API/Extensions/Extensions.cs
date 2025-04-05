using eShop.Catalog.API.Services;
using Microsoft.Extensions.AI;
using OpenAI;

/// <summary>
/// 提供应用程序服务注册的扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 向应用程序添加所需的服务
    /// </summary>
    /// <param name="builder">宿主应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 避免在构建时OpenAPI生成过程中加载完整的数据库配置和迁移
        if (builder.Environment.IsBuild())
        {
            builder.Services.AddDbContext<CatalogContext>();
            return;
        }

        // 添加PostgreSQL数据库上下文，并配置支持向量搜索
        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", configureDbContextOptions: dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(builder =>
            {
                builder.UseVector(); // 启用向量扩展，用于AI嵌入和相似性搜索
            });
        });

        // 注意：此为开发便利而添加，不应在生产环境中使用
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        // 添加使用DbContext的集成服务
        builder.Services.AddTransient<IIntegrationEventLogService, IntegrationEventLogService<CatalogContext>>();

        // 添加目录集成事件服务
        builder.Services.AddTransient<ICatalogIntegrationEventService, CatalogIntegrationEventService>();

        // 配置RabbitMQ事件总线及其订阅
        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>()
               .AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();

        // 绑定目录选项配置
        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

        // AI服务配置 - 根据配置选择使用Ollama或OpenAI
        if (builder.Configuration["OllamaEnabled"] is string ollamaEnabled
            && bool.Parse(ollamaEnabled))
        {
            // 使用Ollama作为AI嵌入生成器
            builder.AddOllamaApiClient("embedding")
                .AddEmbeddingGenerator();
        }
        else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("openai")))
        {
            // 使用OpenAI作为AI嵌入生成器
            builder.AddOpenAIClientFromConfiguration("openai");
            builder.Services
                .AddEmbeddingGenerator(sp => sp.GetRequiredService<OpenAIClient>()
                .AsEmbeddingGenerator(builder.Configuration["AI:OpenAI:EmbeddingModel"]!))
                .UseOpenTelemetry() // 启用OpenTelemetry监控
                .UseLogging();      // 启用日志记录
        }

        // 注册目录AI服务
        builder.Services.AddScoped<ICatalogAI, CatalogAI>();
    }
}
