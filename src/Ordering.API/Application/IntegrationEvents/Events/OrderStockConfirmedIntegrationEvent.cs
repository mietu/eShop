namespace eShop.Ordering.API.Application.IntegrationEvents.Events;

/// <summary>
/// 表示订单库存已确认的集成事件
/// </summary>
public record OrderStockConfirmedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 初始化订单库存确认集成事件的新实例
    /// </summary>
    /// <param name="orderId">已确认库存的订单ID</param>
    public OrderStockConfirmedIntegrationEvent(int orderId) => OrderId = orderId;
}
