using System.Text.Json.Serialization;
using eShop.Basket.API.IntegrationEvents.EventHandling;
using eShop.Basket.API.IntegrationEvents.EventHandling.Events;
using eShop.Basket.API.Repositories;

namespace eShop.Basket.API.Extensions;

/// <summary>
/// 提供应用程序服务配置的扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 向应用程序添加所需的服务和依赖项
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 添加默认的身份验证服务
        builder.AddDefaultAuthentication();

        // 添加Redis客户端，使用配置名称"redis"
        builder.AddRedisClient("redis");

        // 注册购物篮仓储服务的实现
        builder.Services.AddSingleton<IBasketRepository, RedisBasketRepository>();

        // 配置RabbitMQ事件总线
        // 1. 添加事件总线，使用配置名称"eventbus"
        // 2. 注册订单开始集成事件的订阅处理程序
        // 3. 配置JSON序列化选项，添加集成事件上下文用于类型解析
        builder.AddRabbitMqEventBus("eventbus")
               .AddSubscription<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>()
               .ConfigureJsonOptions(options => options.TypeInfoResolverChain.Add(IntegrationEventContext.Default));
    }
}

/// <summary>
/// 用于JSON序列化的集成事件上下文
/// 通过JsonSerializable特性指定可序列化的类型
/// </summary>
[JsonSerializable(typeof(OrderStartedIntegrationEvent))]
partial class IntegrationEventContext : JsonSerializerContext
{
    // 空实现，通过JsonSerializerContext提供的功能和特性标记完成工作
}
