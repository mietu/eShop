using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// 提供用于向应用程序添加RabbitMQ事件总线功能的扩展方法。
/// </summary>
public static class RabbitMqDependencyInjectionExtensions
{
    // 配置示例：
    // {
    //   "EventBus": {
    //     "SubscriptionClientName": "...", // 订阅客户端名称
    //     "RetryCount": 10                // 重试次数
    //   }
    // }

    /// <summary>
    /// 事件总线配置在appsettings.json中的节点名称
    /// </summary>
    private const string SectionName = "EventBus";

    /// <summary>
    /// 向应用程序添加RabbitMQ事件总线及其依赖项。
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    /// <param name="connectionName">RabbitMQ连接名称，用于从配置中获取连接信息</param>
    /// <returns>事件总线构建器，允许进一步配置事件总线</returns>
    /// <exception cref="ArgumentNullException">当builder参数为null时抛出</exception>
    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // 添加RabbitMQ客户端，并配置工厂使用异步消费者分发
        builder.AddRabbitMQClient(connectionName, configureConnectionFactory: factory =>
        {
            ((ConnectionFactory)factory).DispatchConsumersAsync = true;
        });

        // RabbitMQ.Client没有内置对OpenTelemetry的支持，因此需要手动添加
        builder.Services.AddOpenTelemetry()
           .WithTracing(tracing =>
           {
               tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
           });

        // 从配置中加载事件总线选项
        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

        // 注册核心客户端API上的抽象
        builder.Services.AddSingleton<RabbitMQTelemetry>();
        builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
        // 将事件总线注册为托管服务，以便应用程序启动时开始消费消息
        builder.Services.AddSingleton<IHostedService>(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    /// <summary>
    /// 事件总线构建器的内部实现，用于提供服务集合访问，方便进一步配置
    /// </summary>
    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        /// <summary>
        /// 获取服务集合，用于注册额外的服务
        /// </summary>
        public IServiceCollection Services => services;
    }
}
