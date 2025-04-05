namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 处理订单状态变为"已支付"的领域事件处理器
/// 当订单支付完成后，该处理器负责创建并发布集成事件，以通知其他微服务（如库存服务）
/// </summary>
public class OrderStatusChangedToPaidDomainEventHandler : INotificationHandler<OrderStatusChangedToPaidDomainEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 初始化<see cref="OrderStatusChangedToPaidDomainEventHandler"/>的新实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口，用于获取订单信息</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="buyerRepository">买家仓储接口，用于获取买家信息</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务，用于发布集成事件</param>
    /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
    public OrderStatusChangedToPaidDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderStatusChangedToPaidDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
    }

    /// <summary>
    /// 处理订单状态变为已支付的领域事件
    /// </summary>
    /// <param name="domainEvent">包含订单ID和订单项信息的领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderStatusChangedToPaidDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 记录订单状态更新的日志
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.OrderId, OrderStatus.Paid);

        // 获取订单详细信息
        var order = await _orderRepository.GetAsync(domainEvent.OrderId);

        // 获取买家详细信息
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        // 转换订单项列表为库存项列表，用于通知库存服务
        var orderStockList = domainEvent.OrderItems
            .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.Units));

        // 创建订单状态变更为已支付的集成事件
        var integrationEvent = new OrderStatusChangedToPaidIntegrationEvent(
            domainEvent.OrderId,
            order.OrderStatus,
            buyer.Name,
            buyer.IdentityGuid,
            orderStockList);

        // 将集成事件添加到事件服务并保存，后续会被发布到事件总线
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
