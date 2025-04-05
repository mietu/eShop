namespace eShop.Ordering.API.Application.Commands;

using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 创建订单命令处理器
/// 负责处理创建订单的业务逻辑，包括订单聚合根的创建、添加订单项以及持久化
/// 遵循CQRS模式的命令处理器实现
/// </summary>
public class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    /// <summary>
    /// 创建订单命令处理器的构造函数
    /// </summary>
    /// <param name="mediator">中介者，用于发布领域事件</param>
    /// <param name="orderingIntegrationEventService">订单集成事件服务，用于发布跨服务的集成事件</param>
    /// <param name="orderRepository">订单仓储接口，用于持久化订单数据</param>
    /// <param name="identityService">身份服务，用于用户身份相关操作</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException">当任何依赖项为空时抛出</exception>
    public CreateOrderCommandHandler(IMediator mediator,
        IOrderingIntegrationEventService orderingIntegrationEventService,
        IOrderRepository orderRepository,
        IIdentityService identityService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 处理创建订单命令
    /// </summary>
    /// <param name="message">创建订单命令对象，包含订单所有必要信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>订单创建是否成功</returns>
    public async Task<bool> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        // 添加集成事件以清空购物篮
        // 这是跨服务边界的操作，通过集成事件实现服务间通信
        var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(message.UserId);
        await _orderingIntegrationEventService.AddAndSaveEventAsync(orderStartedIntegrationEvent);

        // 创建地址值对象
        var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);

        // 创建订单聚合根实例
        // DDD模式说明：通过聚合根的方法和构造函数添加子实体和值对象
        // 以确保验证规则、不变量和业务逻辑在整个聚合中保持一致性
        var order = new Order(message.UserId, message.UserName, address, message.CardTypeId, message.CardNumber, message.CardSecurityNumber, message.CardHolderName, message.CardExpiration);

        // 添加订单项到订单聚合根
        foreach (var item in message.OrderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        // 记录创建订单的信息
        _logger.LogInformation("正在创建订单 - Order: {@Order}", order);

        // 将订单添加到仓储
        _orderRepository.Add(order);

        // 保存订单实体并发布领域事件
        // 返回保存操作的结果
        return await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}


/// <summary>
/// 带有幂等性处理的创建订单命令处理器
/// 用于确保相同的订单请求不会被重复处理
/// </summary>
public class CreateOrderIdentifiedCommandHandler : IdentifiedCommandHandler<CreateOrderCommand, bool>
{
    /// <summary>
    /// 带有幂等性处理的创建订单命令处理器构造函数
    /// </summary>
    /// <param name="mediator">中介者，用于转发实际的命令处理</param>
    /// <param name="requestManager">请求管理器，用于跟踪请求以实现幂等性</param>
    /// <param name="logger">日志记录器</param>
    public CreateOrderIdentifiedCommandHandler(
        IMediator mediator,
        IRequestManager requestManager,
        ILogger<IdentifiedCommandHandler<CreateOrderCommand, bool>> logger)
        : base(mediator, requestManager, logger)
    {
    }

    /// <summary>
    /// 为重复请求创建结果
    /// 当检测到重复的创建订单请求时，返回的结果
    /// </summary>
    /// <returns>对于创建订单的重复请求，返回true表示操作成功</returns>
    protected override bool CreateResultForDuplicateRequest()
    {
        return true; // 忽略创建订单的重复请求，返回成功
    }
}
