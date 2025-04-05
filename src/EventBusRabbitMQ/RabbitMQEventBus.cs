namespace eShop.EventBusRabbitMQ;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Polly.Retry;

/// <summary>
/// 基于RabbitMQ的事件总线实现，用于发布和订阅集成事件
/// </summary>
public sealed class RabbitMQEventBus(
    ILogger<RabbitMQEventBus> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    RabbitMQTelemetry rabbitMQTelemetry) : IEventBus, IDisposable, IHostedService
{
    /// <summary>
    /// RabbitMQ交换机名称
    /// </summary>
    private const string ExchangeName = "eshop_event_bus";

    /// <summary>
    /// 重试策略管道，用于处理消息发布时的错误
    /// </summary>
    private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value.RetryCount);

    /// <summary>
    /// 分布式跟踪上下文传播器
    /// </summary>
    private readonly TextMapPropagator _propagator = rabbitMQTelemetry.Propagator;

    /// <summary>
    /// 活动源，用于创建分布式跟踪活动
    /// </summary>
    private readonly ActivitySource _activitySource = rabbitMQTelemetry.ActivitySource;

    /// <summary>
    /// 消费者队列名称
    /// </summary>
    private readonly string _queueName = options.Value.SubscriptionClientName;

    /// <summary>
    /// 事件订阅信息，包含事件类型映射
    /// </summary>
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;

    /// <summary>
    /// RabbitMQ连接
    /// </summary>
    private IConnection _rabbitMQConnection;

    /// <summary>
    /// 消费者通道
    /// </summary>
    private IModel _consumerChannel;

    /// <summary>
    /// 发布集成事件到RabbitMQ
    /// </summary>
    /// <param name="event">要发布的集成事件</param>
    /// <returns>表示异步操作的任务</returns>
    public Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("创建RabbitMQ通道以发布事件: {EventId} ({EventName})", @event.Id, routingKey);
        }

        using var channel = _rabbitMQConnection?.CreateModel() ?? throw new InvalidOperationException("RabbitMQ连接未打开");

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("声明RabbitMQ交换机以发布事件: {EventId}", @event.Id);
        }

        channel.ExchangeDeclare(exchange: ExchangeName, type: "direct");

        var body = SerializeMessage(@event);

        // 使用遵循OpenTelemetry消息规范的名称启动活动
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
        var activityName = $"{routingKey} publish";

        return _pipeline.Execute(() =>
        {
            using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client);

            // 根据采样（以及是否注册了监听器），上面的活动可能不会被创建。
            // 如果创建了活动，则传播其上下文。如果未创建，则传播当前上下文（如果有）。
            ActivityContext contextToInject = default;

            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            var properties = channel.CreateBasicProperties();
            // 设置为持久化消息
            properties.DeliveryMode = 2;

            // 将跟踪上下文注入到消息属性中的本地函数
            static void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
            {
                props.Headers ??= new Dictionary<string, object>();
                props.Headers[key] = value;
            }

            _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), properties, InjectTraceContextIntoBasicProperties);

            SetActivityContext(activity, routingKey, "publish");

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("向RabbitMQ发布事件: {EventId}", @event.Id);
            }

            try
            {
                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                activity.SetExceptionTags(ex);

                throw;
            }
        });
    }

    /// <summary>
    /// 设置活动上下文的标签，遵循OpenTelemetry消息规范
    /// </summary>
    /// <param name="activity">要设置标签的活动</param>
    /// <param name="routingKey">路由键</param>
    /// <param name="operation">操作类型</param>
    private static void SetActivityContext(Activity activity, string routingKey, string operation)
    {
        if (activity is not null)
        {
            // 这些标签是根据OpenTelemetry消息规范添加的
            // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.destination_kind", "queue");
            activity.SetTag("messaging.operation", operation);
            activity.SetTag("messaging.destination.name", routingKey);
            activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="eventArgs">事件参数</param>
    /// <returns>表示异步操作的任务</returns>
    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        // 从消息头中提取跟踪上下文的本地函数
        static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return [Encoding.UTF8.GetString(bytes)];
            }
            return [];
        }

        // 从消息头中提取上游父级的传播上下文
        var parentContext = _propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // 使用遵循OpenTelemetry消息规范的名称启动活动
        var activityName = $"{eventArgs.RoutingKey} receive";

        using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

        SetActivityContext(activity, eventArgs.RoutingKey, "receive");

        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            activity?.SetTag("message", message);

            // 用于测试的假异常
            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"请求的假异常: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "处理消息时出错 \"{Message}\"", message);

            activity.SetExceptionTags(ex);
        }

        // 即使发生异常，我们也会将消息从队列中移除
        // 在实际应用中，这应该通过死信交换机(DLX)处理
        // 更多信息请参见: https://www.rabbitmq.com/dlx.html
        _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    /// <summary>
    /// 处理接收到的事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">事件消息内容</param>
    /// <returns>表示异步操作的任务</returns>
    private async Task ProcessEvent(string eventName, string message)
    {
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("处理RabbitMQ事件: {EventName}", eventName);
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("无法解析事件类型: {EventName}", eventName);
            return;
        }

        // 反序列化事件
        var integrationEvent = DeserializeMessage(message, eventType);

        // 注意: 这可以并行处理

        // 获取所有使用事件类型作为键的处理程序
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    /// <summary>
    /// 反序列化消息为集成事件
    /// </summary>
    /// <param name="message">JSON消息</param>
    /// <param name="eventType">事件类型</param>
    /// <returns>反序列化的集成事件</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }

    /// <summary>
    /// 序列化集成事件为字节数组
    /// </summary>
    /// <param name="event">要序列化的集成事件</param>
    /// <returns>序列化后的字节数组</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    /// <summary>
    /// 启动事件总线服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 消息处理是异步的，所以我们不需要等待它完成。此外，
        // 这些API是阻塞的，所以我们需要在后台线程上运行。
        _ = Task.Factory.StartNew(() =>
        {
            try
            {
                logger.LogInformation("在后台线程上启动RabbitMQ连接");

                _rabbitMQConnection = serviceProvider.GetRequiredService<IConnection>();
                if (!_rabbitMQConnection.IsOpen)
                {
                    return;
                }

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("创建RabbitMQ消费者通道");
                }

                _consumerChannel = _rabbitMQConnection.CreateModel();

                _consumerChannel.CallbackException += (sender, ea) =>
                {
                    logger.LogWarning(ea.Exception, "RabbitMQ消费者通道出错");
                };

                _consumerChannel.ExchangeDeclare(exchange: ExchangeName,
                                        type: "direct");

                _consumerChannel.QueueDeclare(queue: _queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.LogTrace("启动RabbitMQ基本消费");
                }

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += OnMessageReceived;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                // 为每种事件类型绑定队列和交换机
                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    _consumerChannel.QueueBind(
                        queue: _queueName,
                        exchange: ExchangeName,
                        routingKey: eventName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "启动RabbitMQ连接时出错");
            }
        },
        TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 停止事件总线服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建弹性管道，用于处理消息发布重试
    /// </summary>
    /// <param name="retryCount">最大重试次数</param>
    /// <returns>配置好的弹性管道</returns>
    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        // 参见 https://www.pollydocs.org/strategies/retry.html
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        // 根据尝试次数生成延迟时间（指数退避策略）
        static TimeSpan? GenerateDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}
