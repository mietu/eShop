namespace eShop.PaymentProcessor.IntegrationEvents.Events;

/// <summary>
/// 表示订单状态已更改为"库存已确认"的集成事件
/// </summary>
/// <param name="OrderId">相关订单的唯一标识符</param>
public record OrderStatusChangedToStockConfirmedIntegrationEvent(int OrderId) : IntegrationEvent;
