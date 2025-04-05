using System.ComponentModel.DataAnnotations;

namespace eShop.IntegrationEventLogEF;

/// <summary>
/// 集成事件日志条目类，用于跟踪和持久化集成事件的状态和内容
/// </summary>
public class IntegrationEventLogEntry
{
    /// <summary>
    /// 用于序列化事件内容的JSON选项，启用缩进格式
    /// </summary>
    private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };

    /// <summary>
    /// 用于反序列化事件内容的JSON选项，不区分属性名称大小写
    /// </summary>
    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// 私有构造函数，用于EF Core的实体构建
    /// </summary>
    private IntegrationEventLogEntry() { }

    /// <summary>
    /// 从集成事件和事务ID创建日志条目
    /// </summary>
    /// <param name="event">要记录的集成事件</param>
    /// <param name="transactionId">关联的事务ID</param>
    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId)
    {
        EventId = @event.Id;
        CreationTime = @event.CreationDate;
        EventTypeName = @event.GetType().FullName;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), s_indentedOptions);
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId;
    }

    /// <summary>
    /// 获取或设置事件ID
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// 获取或设置事件类型的完整名称
    /// </summary>
    [Required]
    public string EventTypeName { get; private set; }

    /// <summary>
    /// 获取事件类型的短名称（不映射到数据库）
    /// </summary>
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.')?.Last();

    /// <summary>
    /// 获取或设置反序列化后的集成事件对象（不映射到数据库）
    /// </summary>
    [NotMapped]
    public IntegrationEvent IntegrationEvent { get; private set; }

    /// <summary>
    /// 获取或设置事件的发布状态
    /// </summary>
    public EventStateEnum State { get; set; }

    /// <summary>
    /// 获取或设置事件发送尝试次数
    /// </summary>
    public int TimesSent { get; set; }

    /// <summary>
    /// 获取或设置事件的创建时间
    /// </summary>
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// 获取或设置事件的序列化JSON内容
    /// </summary>
    [Required]
    public string Content { get; private set; }

    /// <summary>
    /// 获取或设置关联的事务ID
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// 将JSON内容反序列化为指定类型的集成事件
    /// </summary>
    /// <param name="type">要反序列化的集成事件类型</param>
    /// <returns>当前日志条目实例（支持链式调用）</returns>
    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        IntegrationEvent = JsonSerializer.Deserialize(Content, type, s_caseInsensitiveOptions) as IntegrationEvent;
        return this;
    }
}
