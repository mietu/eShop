namespace eShop.IntegrationEventLogEF;

/// <summary>
/// 表示集成事件的发布状态
/// </summary>
public enum EventStateEnum
{
    /// <summary>
    /// 事件尚未发布
    /// </summary>
    NotPublished = 0,

    /// <summary>
    /// 事件发布正在进行中
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 事件已成功发布
    /// </summary>
    Published = 2,

    /// <summary>
    /// 事件发布失败
    /// </summary>
    PublishedFailed = 3
}

