namespace eShop.Ordering.API.Extensions;

/// <summary>
/// 提供订单API的结构化跟踪日志功能。
/// 这个类使用.NET的高性能结构化日志记录（使用LoggerMessage attribute）来记录关键的订单处理事件。
/// </summary>
internal static partial class OrderingApiTrace
{
    /// <summary>
    /// 记录订单状态更新事件。
    /// </summary>
    /// <param name="logger">用于记录日志的ILogger实例</param>
    /// <param name="orderId">被更新的订单ID</param>
    /// <param name="status">订单的新状态</param>
    [LoggerMessage(EventId = 1, EventName = "OrderStatusUpdated", Level = LogLevel.Trace, Message = "Order with Id: {OrderId} has been successfully updated to status {Status}")]
    public static partial void LogOrderStatusUpdated(ILogger logger, int orderId, OrderStatus status);

    /// <summary>
    /// 记录订单支付方式更新事件。
    /// </summary>
    /// <param name="logger">用于记录日志的ILogger实例</param>
    /// <param name="orderId">被更新的订单ID</param>
    /// <param name="paymentMethod">更新后的支付方式名称</param>
    /// <param name="id">支付方式的ID</param>
    [LoggerMessage(EventId = 2, EventName = "PaymentMethodUpdated", Level = LogLevel.Trace, Message = "Order with Id: {OrderId} has been successfully updated with a payment method {PaymentMethod} ({Id})")]
    public static partial void LogOrderPaymentMethodUpdated(ILogger logger, int orderId, string paymentMethod, int id);

    /// <summary>
    /// 记录买家和支付方式验证或更新事件。
    /// </summary>
    /// <param name="logger">用于记录日志的ILogger实例</param>
    /// <param name="buyerId">买家ID</param>
    /// <param name="orderId">相关的订单ID</param>
    [LoggerMessage(EventId = 3, EventName = "BuyerAndPaymentValidatedOrUpdated", Level = LogLevel.Trace, Message = "Buyer {BuyerId} and related payment method were validated or updated for order Id: {OrderId}.")]
    public static partial void LogOrderBuyerAndPaymentValidatedOrUpdated(ILogger logger, int buyerId, int orderId);
}
