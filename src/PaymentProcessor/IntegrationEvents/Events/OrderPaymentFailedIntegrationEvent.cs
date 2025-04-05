namespace eShop.PaymentProcessor.IntegrationEvents.Events;

/// <summary>
/// 表示订单支付失败的集成事件
/// </summary>
/// <param name="OrderId">支付失败的订单ID</param>
public record OrderPaymentFailedIntegrationEvent(int OrderId) : IntegrationEvent;
