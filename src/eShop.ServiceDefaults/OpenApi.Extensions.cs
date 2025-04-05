using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace eShop.ServiceDefaults;

/// <summary>
/// 提供用于配置和使用 OpenAPI 的扩展方法，简化微服务的 API 文档配置。
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// 为 WebApplication 配置并启用默认的 OpenAPI 功能。
    /// </summary>
    /// <param name="app">要配置的 WebApplication 实例。</param>
    /// <returns>配置后的 WebApplication 实例，用于方法链式调用。</returns>
    /// <remarks>
    /// 此方法会检查配置中是否存在 "OpenApi" 节点，如果存在则配置 OpenAPI。
    /// 在开发环境中，还会配置 Scalar UI 并将根路径重定向到 Scalar UI。
    /// </remarks>
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection("OpenApi");

        // 如果配置中不存在 OpenApi 节点，则不进行任何操作
        if (!openApiSection.Exists())
        {
            return app;
        }

        // 配置 OpenAPI 端点
        app.MapOpenApi();

        // 仅在开发环境中配置 Scalar UI 和根路径重定向
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options =>
            {
                // 禁用默认字体以避免下载不必要的字体
                options.DefaultFonts = false;
            });
            app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        }

        return app;
    }

    /// <summary>
    /// 向应用程序添加默认的 OpenAPI 配置和服务。
    /// </summary>
    /// <param name="builder">主机应用程序构建器。</param>
    /// <param name="apiVersioning">API 版本控制构建器，可选参数。</param>
    /// <returns>配置后的主机应用程序构建器，用于方法链式调用。</returns>
    /// <remarks>
    /// 此方法配置 OpenAPI 文档生成器，包括版本控制、安全方案、授权检查等。
    /// 如果配置中存在 Identity 节点，会从中读取授权范围（Scopes）。
    /// </remarks>
    public static IHostApplicationBuilder AddDefaultOpenApi(
        this IHostApplicationBuilder builder,
        IApiVersioningBuilder? apiVersioning = default)
    {
        var openApi = builder.Configuration.GetSection("OpenApi");
        var identitySection = builder.Configuration.GetSection("Identity");

        // 从配置中获取授权范围
        var scopes = identitySection.Exists()
            ? identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, string?>();

        // 如果配置中不存在 OpenApi 节点，则不进行任何操作
        if (!openApi.Exists())
        {
            return builder;
        }

        // 配置 API 版本控制和文档生成
        if (apiVersioning is not null)
        {
            // 设置版本格式为 'v'major[.minor][-status]，例如 v1.0
            var versioned = apiVersioning.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");

            // 定义支持的 API 版本
            string[] versions = ["v1", "v2"];

            // 为每个版本配置 OpenAPI 文档
            foreach (var description in versions)
            {
                builder.Services.AddOpenApi(description, options =>
                {
                    // 应用 API 版本信息到文档标题和描述
                    options.ApplyApiVersionInfo(openApi.GetRequiredValue("Document:Title"), openApi.GetRequiredValue("Document:Description"));

                    // 配置授权检查
                    options.ApplyAuthorizationChecks([.. scopes.Keys]);

                    // 配置安全方案定义
                    options.ApplySecuritySchemeDefinitions();

                    // 标记已废弃的操作
                    options.ApplyOperationDeprecatedStatus();

                    // 应用 API 版本描述
                    options.ApplyApiVersionDescription();

                    // 将所有模式的 nullable 设置为 false
                    options.ApplySchemaNullableFalse();

                    // 清除默认服务器配置，以便回退到 Aspire 为服务分配的端口
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Servers = [];
                        return Task.CompletedTask;
                    });
                });
            }
        }

        return builder;
    }
}
