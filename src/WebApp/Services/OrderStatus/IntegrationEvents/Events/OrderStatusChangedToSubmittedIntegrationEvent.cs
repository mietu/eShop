using eShop.EventBus.Events;

namespace eShop.WebApp.Services.OrderStatus.IntegrationEvents;

/// <summary>
/// 表示订单状态变更为"已提交"的集成事件
/// 当订单从购物车转变为已提交状态时触发此事件
/// 此事件允许其他服务（如订单处理服务、通知服务等）响应订单状态的变化
/// </summary>
public record OrderStatusChangedToSubmittedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 获取订单的唯一标识符
    /// </summary>
    public int OrderId { get; }

    /// <summary>
    /// 获取订单的当前状态描述
    /// </summary>
    public string OrderStatus { get; }

    /// <summary>
    /// 获取下单用户的名称
    /// </summary>
    public string BuyerName { get; }

    /// <summary>
    /// 获取下单用户的身份标识符
    /// </summary>
    public string BuyerIdentityGuid { get; }

    /// <summary>
    /// 初始化订单状态变更为已提交的集成事件实例
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="orderStatus">订单状态</param>
    /// <param name="buyerName">买家名称</param>
    /// <param name="buyerIdentityGuid">买家身份标识符</param>
    public OrderStatusChangedToSubmittedIntegrationEvent(
        int orderId, string orderStatus, string buyerName, string buyerIdentityGuid)
    {
        OrderId = orderId;
        OrderStatus = orderStatus;
        BuyerName = buyerName;
        BuyerIdentityGuid = buyerIdentityGuid;
    }
}
