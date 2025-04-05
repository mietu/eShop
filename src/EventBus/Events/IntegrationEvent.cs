namespace eShop.EventBus.Events;

/// <summary>
/// 表示一个集成事件的基类，用于跨服务或组件的消息传递
/// </summary>
public record IntegrationEvent
{
    /// <summary>
    /// 初始化集成事件实例，自动生成Id和设置创建时间
    /// </summary>
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// 获取或设置事件的唯一标识符
    /// </summary>
    [JsonInclude]
    public Guid Id { get; set; }

    /// <summary>
    /// 获取或设置事件的创建时间（UTC）
    /// </summary>
    [JsonInclude]
    public DateTime CreationDate { get; set; }
}
