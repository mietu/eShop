using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace eShop.ServiceDefaults;

public static partial class Extensions
{
    /// <summary>
    /// 添加服务默认设置，包括基础服务、服务发现和HTTP客户端配置
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        // 添加基础服务配置
        builder.AddBasicServiceDefaults();

        // 添加服务发现功能
        builder.Services.AddServiceDiscovery();

        // 配置HTTP客户端默认设置
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // 默认启用弹性处理
            http.AddStandardResilienceHandler();

            // 默认启用服务发现
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// 添加基础服务默认设置，不包括对外HTTP调用相关的服务
    /// </summary>
    /// <remarks>
    /// 这样设计允许在不使用时将Polly等库从应用程序中裁剪掉
    /// </remarks>
    public static IHostApplicationBuilder AddBasicServiceDefaults(this IHostApplicationBuilder builder)
    {
        // 添加默认健康检查，包括事件总线和自身健康检查
        builder.AddDefaultHealthChecks();

        // 配置OpenTelemetry
        builder.ConfigureOpenTelemetry();

        return builder;
    }

    /// <summary>
    /// 配置OpenTelemetry，用于可观测性和监控
    /// </summary>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        // 配置OpenTelemetry日志记录
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true; // 包含格式化后的消息
            logging.IncludeScopes = true; // 包含作用域
        });

        // 添加OpenTelemetry服务
        builder.Services.AddOpenTelemetry()
            // 配置指标收集
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation() // 添加ASP.NET Core指标
                    .AddHttpClientInstrumentation() // 添加HTTP客户端指标
                    .AddRuntimeInstrumentation() // 添加运行时指标
                    .AddMeter("Experimental.Microsoft.Extensions.AI"); // 添加AI相关指标
            })
            // 配置链路追踪
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // 在开发环境中，我们希望查看所有跟踪
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation() // 添加ASP.NET Core跟踪
                    .AddGrpcClientInstrumentation() // 添加gRPC客户端跟踪
                    .AddHttpClientInstrumentation() // 添加HTTP客户端跟踪
                    .AddSource("Experimental.Microsoft.Extensions.AI"); // 添加AI相关跟踪源
            });

        // 添加OpenTelemetry导出器
        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// 添加OpenTelemetry导出器，用于将遥测数据发送到外部系统
    /// </summary>
    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        // 检查是否配置了OTLP端点
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            // 配置OTLP导出器，将遥测数据发送到指定端点
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        return builder;
    }

    /// <summary>
    /// 添加默认健康检查，确保应用程序正常运行
    /// </summary>
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // 添加默认活跃性检查，确保应用响应正常
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// 映射默认端点，包括健康检查和监控端点
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // 取消注释以下行启用Prometheus端点（需要安装OpenTelemetry.Exporter.Prometheus.AspNetCore包）
        // app.MapPrometheusScrapingEndpoint();

        // 在非开发环境中添加健康检查端点存在安全隐患。
        // 在非开发环境中启用这些端点前，请参阅 https://aka.ms/dotnet/aspire/healthchecks 了解详情。
        if (app.Environment.IsDevelopment())
        {
            // 所有健康检查必须通过，应用才被视为准备好接受流量
            app.MapHealthChecks("/health");

            // 只有标记为"live"的健康检查必须通过，应用才被视为存活
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
