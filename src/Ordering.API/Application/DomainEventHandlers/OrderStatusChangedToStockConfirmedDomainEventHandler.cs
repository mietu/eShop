namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 处理订单状态变更为"库存已确认"的领域事件处理程序
/// 当订单库存确认后，此处理程序负责记录状态变更并发布相应的集成事件
/// </summary>
public class OrderStatusChangedToStockConfirmedDomainEventHandler
                : INotificationHandler<OrderStatusChangedToStockConfirmedDomainEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly ILogger _logger;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 初始化<see cref="OrderStatusChangedToStockConfirmedDomainEventHandler"/>的新实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口，用于获取订单数据</param>
    /// <param name="buyerRepository">买家仓储接口，用于获取买家数据</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务，用于发布集成事件</param>
    /// <exception cref="ArgumentNullException">当任何必需依赖为null时抛出</exception>
    public OrderStatusChangedToStockConfirmedDomainEventHandler(
        IOrderRepository orderRepository,
        IBuyerRepository buyerRepository,
        ILogger<OrderStatusChangedToStockConfirmedDomainEventHandler> logger,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    /// <summary>
    /// 处理订单状态变更为"库存已确认"的领域事件
    /// </summary>
    /// <param name="domainEvent">包含已确认库存订单信息的领域事件</param>
    /// <param name="cancellationToken">取消操作的令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToStockConfirmedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 记录订单状态更新的日志
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.OrderId, OrderStatus.StockConfirmed);

        // 获取相关的订单和买家信息
        var order = await _orderRepository.GetAsync(domainEvent.OrderId);
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        // 创建并保存集成事件，以通知其他微服务订单状态的变更
        var integrationEvent = new OrderStatusChangedToStockConfirmedIntegrationEvent(order.Id, order.OrderStatus, buyer.Name, buyer.IdentityGuid);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
