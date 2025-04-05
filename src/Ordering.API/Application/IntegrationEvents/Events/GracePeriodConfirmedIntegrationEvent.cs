namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单宽限期已确认的集成事件
/// </summary>
public record GracePeriodConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取相关订单的标识符
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化宽限期确认集成事件的新实例
    /// </summary>
    /// <param name="orderId">相关订单的标识符</param>
    public GracePeriodConfirmedIntegrationEvent(int orderId) =>
        OrderId = orderId;
}

