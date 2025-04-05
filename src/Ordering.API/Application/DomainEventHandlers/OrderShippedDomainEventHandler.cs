namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 处理订单发货领域事件的处理程序
/// 当订单状态变为"已发货"时，此处理程序会被触发
/// 负责记录状态变更日志并发布集成事件通知其他服务
/// </summary>
public class OrderShippedDomainEventHandler
                : INotificationHandler<OrderShippedDomainEvent>
{
    /// <summary>
    /// 订单仓储接口，用于获取订单数据
    /// </summary>
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// 买家仓储接口，用于获取买家信息
    /// </summary>
    private readonly IBuyerRepository _buyerRepository;

    /// <summary>
    /// 订单集成事件服务，用于发布跨服务的集成事件
    /// </summary>
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化订单发货领域事件处理程序
    /// </summary>
    /// <param name="orderRepository">订单仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="buyerRepository">买家仓储</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务</param>
    /// <exception cref="ArgumentNullException">任何必要参数为空时抛出</exception>
    public OrderShippedDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderShippedDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    /// <summary>
    /// 处理订单发货领域事件
    /// </summary>
    /// <param name="domainEvent">订单发货领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task Handle(OrderShippedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 记录订单状态更新日志
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.Order.Id, OrderStatus.Shipped);

        // 获取完整的订单信息
        var order = await _orderRepository.GetAsync(domainEvent.Order.Id);

        // 获取关联的买家信息
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        // 创建订单状态变更为已发货的集成事件
        var integrationEvent = new OrderStatusChangedToShippedIntegrationEvent(order.Id, order.OrderStatus, buyer.Name, buyer.IdentityGuid);

        // 保存并发布集成事件
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
