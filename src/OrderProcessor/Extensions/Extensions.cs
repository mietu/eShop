using System.Text.Json.Serialization;
using eShop.OrderProcessor.Events;

namespace eShop.OrderProcessor.Extensions;

/// <summary>
/// 提供应用程序服务扩展方法的静态类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 向应用程序添加所需的服务和配置
    /// </summary>
    /// <param name="builder">宿主应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 配置RabbitMQ事件总线并添加JSON序列化选项
        // 使用"eventbus"作为连接字符串名称
        builder.AddRabbitMqEventBus("eventbus")
               .ConfigureJsonOptions(options => options.TypeInfoResolverChain.Add(IntegrationEventContext.Default));

        // 添加PostgreSQL数据源，使用"orderingdb"作为连接字符串名称
        builder.AddNpgsqlDataSource("orderingdb");

        // 注册BackgroundTaskOptions选项并从配置中绑定值
        builder.Services.AddOptions<BackgroundTaskOptions>()
            .BindConfiguration(nameof(BackgroundTaskOptions));

        // 注册GracePeriodManagerService作为后台服务
        // 该服务负责管理订单的宽限期
        builder.Services.AddHostedService<GracePeriodManagerService>();
    }
}

/// <summary>
/// 为集成事件提供JSON序列化上下文
/// 通过AOT(Ahead-of-Time)编译优化序列化性能
/// </summary>
[JsonSerializable(typeof(GracePeriodConfirmedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{

}
