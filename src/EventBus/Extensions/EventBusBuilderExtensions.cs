using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using eShop.EventBus.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 提供用于配置事件总线的扩展方法
/// </summary>
public static class EventBusBuilderExtensions
{
    /// <summary>
    /// 配置事件总线使用的JSON序列化选项
    /// </summary>
    /// <param name="eventBusBuilder">事件总线构建器</param>
    /// <param name="configure">配置JSON序列化选项的委托</param>
    /// <returns>事件总线构建器实例，用于链式调用</returns>
    public static IEventBusBuilder ConfigureJsonOptions(
        this IEventBusBuilder eventBusBuilder, Action<JsonSerializerOptions> configure)
    {
        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            configure(o.JsonSerializerOptions);
        });

        return eventBusBuilder;
    }

    /// <summary>
    /// 为指定的事件类型添加事件处理程序订阅
    /// </summary>
    /// <typeparam name="T">集成事件类型</typeparam>
    /// <typeparam name="TH">处理该事件的处理程序类型</typeparam>
    /// <param name="eventBusBuilder">事件总线构建器</param>
    /// <returns>事件总线构建器实例，用于链式调用</returns>
    /// <remarks>
    /// 使用键控服务注册机制，允许为同一事件类型注册多个处理程序
    /// </remarks>
    public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(this IEventBusBuilder eventBusBuilder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        // 使用键控服务注册多个同类型事件的处理程序
        // 消费者可以使用 IKeyedServiceProvider.GetKeyedService<IIntegrationEventHandler>(typeof(T)) 
        // 获取该事件类型的所有处理程序
        eventBusBuilder.Services.AddKeyedTransient<IIntegrationEventHandler, TH>(typeof(T));

        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            // 跟踪所有已注册的事件类型及其名称映射
            // 我们通过消息总线发送这些事件类型，并且不希望使用 Type.GetType，
            // 因此在这里维护名称到类型的映射关系

            // 此列表还将用于从底层消息代理实现订阅事件
            o.EventTypes[typeof(T).Name] = typeof(T);
        });

        return eventBusBuilder;
    }
}
