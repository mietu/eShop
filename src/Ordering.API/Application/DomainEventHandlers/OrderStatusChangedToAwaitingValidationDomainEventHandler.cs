namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 处理订单状态变更为"等待验证"的领域事件处理器
/// 当订单状态变更为等待验证时，此处理器将检索相关数据并发布集成事件
/// </summary>
public class OrderStatusChangedToAwaitingValidationDomainEventHandler
                : INotificationHandler<OrderStatusChangedToAwaitingValidationDomainEvent>
{
    private readonly IOrderRepository _orderRepository;          // 订单仓储，用于获取订单信息
    private readonly ILogger _logger;                            // 日志记录器
    private readonly IBuyerRepository _buyerRepository;          // 买家仓储，用于获取买家信息
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;  // 集成事件服务，用于发布跨微服务的事件

    /// <summary>
    /// 构造函数，通过依赖注入初始化处理器所需的服务
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="buyerRepository">买家仓储接口</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务</param>
    /// <exception cref="ArgumentNullException">当必要依赖为空时抛出</exception>
    public OrderStatusChangedToAwaitingValidationDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderStatusChangedToAwaitingValidationDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository;
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    /// <summary>
    /// 处理订单状态变更为"等待验证"的领域事件
    /// </summary>
    /// <param name="domainEvent">包含订单ID和订单项的领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task Handle(OrderStatusChangedToAwaitingValidationDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 记录订单状态更新的日志
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.OrderId, OrderStatus.AwaitingValidation);

        // 从仓储获取完整的订单信息
        var order = await _orderRepository.GetAsync(domainEvent.OrderId);

        // 获取买家信息
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        // 将领域事件中的订单项转换为库存项列表
        var orderStockList = domainEvent.OrderItems
            .Select(orderItem => new OrderStockItem(orderItem.ProductId, orderItem.Units));

        // 创建集成事件，包含订单和买家信息，以通知其他微服务（如库存服务）
        var integrationEvent = new OrderStatusChangedToAwaitingValidationIntegrationEvent(
            order.Id,
            order.OrderStatus,
            buyer.Name,
            buyer.IdentityGuid,
            orderStockList);

        // 保存集成事件，稍后将通过事件总线发布
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
