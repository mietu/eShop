namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 当订单启动域事件触发时，负责验证或添加买家聚合根的处理程序
/// 该处理程序实现了MediatR的INotificationHandler接口，用于处理OrderStartedDomainEvent事件
/// </summary>
public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler
                    : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ILogger _logger;
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

    /// <summary>
    /// 构造函数，通过依赖注入获取所需服务
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="buyerRepository">买家仓储接口</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务</param>
    /// <exception cref="ArgumentNullException">当任何参数为null时抛出</exception>
    public ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(
        ILogger<ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler> logger,
        IBuyerRepository buyerRepository,
        IOrderingIntegrationEventService orderingIntegrationEventService)
    {
        _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理OrderStartedDomainEvent事件
    /// 该方法负责验证或创建买家，添加支付方式，并发布集成事件
    /// </summary>
    /// <param name="domainEvent">订单启动域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task Handle(OrderStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 确保卡类型ID至少为1
        var cardTypeId = domainEvent.CardTypeId != 0 ? domainEvent.CardTypeId : 1;

        // 根据用户ID查找买家
        var buyer = await _buyerRepository.FindAsync(domainEvent.UserId);
        var buyerExisted = buyer is not null;

        // 如果买家不存在则创建新买家
        if (!buyerExisted)
        {
            buyer = new Buyer(domainEvent.UserId, domainEvent.UserName);
        }

        // REVIEW: The event this creates needs to be sent after SaveChanges has propagated the buyer Id. It currently only
        // works by coincidence. If we remove HiLo or if anything decides to yield earlier, it will break.

        // 验证或添加支付方式到买家实体
        buyer.VerifyOrAddPaymentMethod(cardTypeId,
                                        $"Payment Method on {DateTime.UtcNow}",
                                        domainEvent.CardNumber,
                                        domainEvent.CardSecurityNumber,
                                        domainEvent.CardHolderName,
                                        domainEvent.CardExpiration,
                                        domainEvent.Order.Id);

        // 如果是新买家，将其添加到仓储
        if (!buyerExisted)
        {
            _buyerRepository.Add(buyer);
        }

        // 保存实体更改到数据库
        await _buyerRepository.UnitOfWork
            .SaveEntitiesAsync(cancellationToken);

        // 创建并保存订单状态变更为已提交的集成事件
        var integrationEvent = new OrderStatusChangedToSubmittedIntegrationEvent(domainEvent.Order.Id, domainEvent.Order.OrderStatus, buyer.Name, buyer.IdentityGuid);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(integrationEvent);

        // 记录买家和支付方式验证或更新的日志
        OrderingApiTrace.LogOrderBuyerAndPaymentValidatedOrUpdated(_logger, buyer.Id, domainEvent.Order.Id);
    }
}
