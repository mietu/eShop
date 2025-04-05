namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 订单取消领域事件的处理程序
/// 当订单被取消时，负责处理相关的后续操作
/// </summary>
public partial class OrderCancelledDomainEventHandler
                : INotificationHandler<OrderCancelledDomainEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBuyerRepository _buyerRepository;
    private readonly ILogger _logger;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 初始化订单取消领域事件处理程序的新实例
    /// </summary>
    /// <param name="orderRepository">订单仓储接口，用于访问订单数据</param>
    /// <param name="logger">日志记录器，用于记录处理过程中的事件</param>
    /// <param name="buyerRepository">买家仓储接口，用于访问买家数据</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务，用于发布集成事件</param>
    /// <exception cref="ArgumentNullException">当任何必需的依赖项为空时抛出</exception>
    public OrderCancelledDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<OrderCancelledDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService;
    }

    /// <summary>
    /// 处理订单取消领域事件
    /// </summary>
    /// <param name="domainEvent">包含已取消订单信息的领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 记录订单状态已更新为"已取消"的日志
        OrderingApiTrace.LogOrderStatusUpdated(_logger, domainEvent.Order.Id, OrderStatus.Cancelled);

        // 获取完整的订单信息
        var order = await _orderRepository.GetAsync(domainEvent.Order.Id);

        // 获取与订单关联的买家信息
        var buyer = await _buyerRepository.FindByIdAsync(order.BuyerId.Value);

        // 创建订单状态变更为已取消的集成事件
        var integrationEvent = new OrderStatusChangedToCancelledIntegrationEvent(order.Id, order.OrderStatus, buyer.Name, buyer.IdentityGuid);

        // 添加并保存集成事件，以便后续发布到事件总线
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
