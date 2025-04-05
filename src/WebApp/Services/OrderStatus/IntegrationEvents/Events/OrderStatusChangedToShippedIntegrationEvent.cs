using eShop.EventBus.Events;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 表示订单状态变更为已发货的集成事件
/// </summary>
public record OrderStatusChangedToShippedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单ID
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单状态
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 获取买家姓名
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 获取买家身份标识符
    /// </summary>
    public string BuyerIdentityGuid { get; }

    /// <summary>
    /// 初始化订单状态变更为已发货的集成事件实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家姓名</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    public OrderStatusChangedToShippedIntegrationEvent(
        int orderId, string orderStatus, string buyerName, string buyerIdentityGuid)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}

