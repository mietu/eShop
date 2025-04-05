using System.Diagnostics;
using OpenTelemetry.Context.Propagation;

namespace eShop.EventBusRabbitMQ;

/// <summary>
/// 提供 RabbitMQ 事件总线的遥测功能。
/// 用于创建、跟踪和传播分布式追踪上下文。
/// </summary>
public class RabbitMQTelemetry
{
    /// <summary>
    /// 定义用于 RabbitMQ 事件总线遥测的活动源名称。
    /// 该名称用于在分布式追踪中标识来自 RabbitMQ 事件总线的遥测数据。
    /// </summary>
    public static string ActivitySourceName = "EventBusRabbitMQ";

    /// <summary>
    /// 提供用于创建和跟踪活动的活动源。
    /// 活动用于测量操作的持续时间和收集诊断信息。
    /// </summary>
    public ActivitySource ActivitySource { get; } = new(ActivitySourceName);

    /// <summary>
    /// 提供用于在服务之间传播上下文的文本映射传播器。
    /// 用于确保分布式追踪上下文能够跨越消息队列边界。
    /// </summary>
    public TextMapPropagator Propagator { get; } = Propagators.DefaultTextMapPropagator;
}
