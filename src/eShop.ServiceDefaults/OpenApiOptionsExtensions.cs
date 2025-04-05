using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace eShop.ServiceDefaults;

/// <summary>
/// 提供用于配置Swagger/OpenAPI文档的扩展方法集合
/// 这些方法增强API文档，添加版本信息、安全设置、授权检查等
/// </summary>
internal static class OpenApiOptionsExtensions
{
    /// <summary>
    /// 应用API版本信息到OpenAPI文档
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <param name="title">API文档标题</param>
    /// <param name="description">API文档描述</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplyApiVersionInfo(this OpenApiOptions options, string title, string description)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            // 获取API版本描述提供程序
            var versionedDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            // 查找与当前文档名称匹配的API版本描述
            var apiDescription = versionedDescriptionProvider?.ApiVersionDescriptions
                .SingleOrDefault(description => description.GroupName == context.DocumentName);
            if (apiDescription is null)
            {
                return Task.CompletedTask;
            }
            // 设置文档信息
            document.Info.Version = apiDescription.ApiVersion.ToString();
            document.Info.Title = title;
            document.Info.Description = BuildDescription(apiDescription, description);
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 构建API描述文本，包括版本弃用信息和日落策略
    /// </summary>
    /// <param name="api">API版本描述</param>
    /// <param name="description">基础描述文本</param>
    /// <returns>完整的描述文本</returns>
    private static string BuildDescription(ApiVersionDescription api, string description)
    {
        var text = new StringBuilder(description);

        // 添加版本弃用信息
        if (api.IsDeprecated)
        {
            if (text.Length > 0)
            {
                // 确保前面的文本以句号结束
                if (text[^1] != '.')
                {
                    text.Append('.');
                }

                text.Append(' ');
            }

            text.Append("此 API 版本已弃用。");
        }

        // 添加日落策略信息
        if (api.SunsetPolicy is { } policy)
        {
            // 包含日落日期
            if (policy.Date is { } when)
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }

                text.Append("API 将于停用")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            // 添加相关链接
            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                // 只包含HTML类型的链接
                foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
                {
                    if (!rendered)
                    {
                        text.Append("<h4>Links</h4><ul>");
                        rendered = true;
                    }

                    // 构建链接HTML
                    text.Append("<li><a href=\"");
                    text.Append(link.LinkTarget.OriginalString);
                    text.Append("\">");
                    text.Append(
                        StringSegment.IsNullOrEmpty(link.Title)
                        ? link.LinkTarget.OriginalString
                        : link.Title.ToString());
                    text.Append("</a></li>");
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        return text.ToString();
    }

    /// <summary>
    /// 应用安全方案定义到OpenAPI文档
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplySecuritySchemeDefinitions(this OpenApiOptions options)
    {
        options.AddDocumentTransformer<SecuritySchemeDefinitionsTransformer>();
        return options;
    }

    /// <summary>
    /// 应用授权检查到OpenAPI操作
    /// 为需要授权的操作添加安全要求、401和403响应
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <param name="scopes">OAuth2需要的作用域数组</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplyAuthorizationChecks(this OpenApiOptions options, string[] scopes)
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            // 获取端点元数据
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            // 如果操作不需要授权，不进行任何更改
            if (!metadata.OfType<IAuthorizeData>().Any())
            {
                return Task.CompletedTask;
            }

            // 添加常见的授权相关响应
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            // 设置OAuth2安全方案
            var oAuthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };

            // 为操作添加所需的安全要求
            operation.Security =
            [
              new()
              {
                  [oAuthScheme] = scopes
              }
            ];

            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 应用操作弃用状态到OpenAPI操作
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplyOperationDeprecatedStatus(this OpenApiOptions options)
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var apiDescription = context.Description;
            // 使用位运算符保留原有的弃用状态，同时考虑API描述中的弃用标志
            operation.Deprecated |= apiDescription.IsDeprecated();
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 应用API版本描述到OpenAPI操作
    /// 为api-version参数添加描述和示例值
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplyApiVersionDescription(this OpenApiOptions options)
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            // 查找名为"api-version"的参数
            var apiVersionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "api-version");
            if (apiVersionParameter is not null)
            {
                // 添加描述文本
                apiVersionParameter.Description = "The API version, in the format 'major.minor'.";
                // 根据文档名称设置不同的示例值
                switch (context.DocumentName)
                {
                    case "v1":
                        apiVersionParameter.Schema.Example = new OpenApiString("1.0");
                        break;
                    case "v2":
                        apiVersionParameter.Schema.Example = new OpenApiString("2.0");
                        break;
                }
            }
            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 为所有可选属性设置nullable为false的架构转换器
    /// 这有助于生成更准确的客户端代码
    /// </summary>
    /// <param name="options">OpenAPI选项</param>
    /// <returns>配置后的OpenAPI选项</returns>
    public static OpenApiOptions ApplySchemaNullableFalse(this OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            if (schema.Properties is not null)
            {
                foreach (var property in schema.Properties)
                {
                    // 如果属性不是必需的，将其Nullable属性设置为false
                    if (schema.Required?.Contains(property.Key) != true)
                    {
                        property.Value.Nullable = false;
                    }
                }
            }

            return Task.CompletedTask;
        });
        return options;
    }

    /// <summary>
    /// 负责从配置中读取并添加OAuth2安全方案定义的文档转换器
    /// </summary>
    private class SecuritySchemeDefinitionsTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            // 获取身份验证配置节
            var identitySection = configuration.GetSection("Identity");
            if (!identitySection.Exists())
            {
                return Task.CompletedTask;
            }

            // 读取身份服务URL和作用域
            var identityUrlExternal = identitySection.GetRequiredValue("Url");
            var scopes = identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);

            // 创建OAuth2安全方案
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows()
                {
                    // TODO: Change this to use Authorization Code flow with PKCE
                    Implicit = new OpenApiOAuthFlow()
                    {
                        AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
                        TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
                        Scopes = scopes,
                    }
                }
            };

            // 确保Components对象存在并添加安全方案
            document.Components ??= new();
            document.Components.SecuritySchemes.Add("oauth2", securityScheme);
            return Task.CompletedTask;
        }
    }
}
