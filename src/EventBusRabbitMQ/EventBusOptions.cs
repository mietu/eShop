namespace eShop.EventBusRabbitMQ;

/// <summary>
/// 配置事件总线的选项类
/// </summary>
public class EventBusOptions
{
    /// <summary>
    /// 获取或设置订阅客户端名称
    /// </summary>
    public string SubscriptionClientName { get; set; }

    /// <summary>
    /// 获取或设置重试次数
    /// 默认值为10次
    /// </summary>
    public int RetryCount { get; set; } = 10;
}
