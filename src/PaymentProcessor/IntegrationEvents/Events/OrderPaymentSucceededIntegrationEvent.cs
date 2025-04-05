namespace eShop.PaymentProcessor.IntegrationEvents.Events;

/// <summary>
/// 表示订单支付成功的集成事件
/// </summary>
/// <param name="OrderId">已成功支付的订单ID</param>
public record OrderPaymentSucceededIntegrationEvent(int OrderId) : IntegrationEvent;
