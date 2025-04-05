namespace eShop.OrderProcessor.Events
{
    using eShop.EventBus.Events;

    /// <summary>
    /// 表示订单宽限期已确认的集成事件
    /// 当订单的宽限期结束并确认后触发此事件
    /// </summary>
    public record GracePeriodConfirmedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 获取需要确认的订单ID
        /// </summary>
        public int OrderId { get; }

        /// <summary>
        /// 初始化订单宽限期确认事件的新实例
        /// </summary>
        /// <param name="orderId">需要确认的订单ID</param>
        public GracePeriodConfirmedIntegrationEvent(int orderId) =>
            OrderId = orderId;
    }
}
