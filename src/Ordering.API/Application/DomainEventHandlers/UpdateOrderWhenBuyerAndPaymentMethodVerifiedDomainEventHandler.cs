namespace eShop.Ordering.API.Application.DomainEventHandlers;

/// <summary>
/// 处理买家和支付方式验证后的订单更新领域事件处理程序
/// 实现MediatR的INotificationHandler接口来处理BuyerAndPaymentMethodVerifiedDomainEvent事件
/// </summary>
public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler : INotificationHandler<BuyerAndPaymentMethodVerifiedDomainEvent>
{
    private readonly IOrderRepository _orderRepository; // 订单仓储接口，用于获取和更新订单
    private readonly ILogger _logger; // 日志记录器，用于记录操作日志

    /// <summary>
    /// 构造函数，通过依赖注入获取所需依赖
    /// </summary>
    /// <param name="orderRepository">订单仓储接口</param>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException">如果任何参数为null则抛出异常</exception>
    public UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(
        IOrderRepository orderRepository,
        ILogger<UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Domain Logic comment:
    // When the Buyer and Buyer's payment method have been created or verified that they existed, 
    // then we can update the original Order with the BuyerId and PaymentId (foreign keys)

    /// <summary>
    /// 处理买家和支付方式验证领域事件
    /// </summary>
    /// <param name="domainEvent">包含已验证的买家和支付方式信息的领域事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    /// <remarks>
    /// 此方法执行以下操作：
    /// 1. 从仓储中获取需要更新的订单
    /// 2. 使用验证过的买家ID和支付方式ID更新订单
    /// 3. 记录订单支付方式更新的日志
    /// </remarks>
    public async Task Handle(BuyerAndPaymentMethodVerifiedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 从仓储中获取需要更新的订单
        var orderToUpdate = await _orderRepository.GetAsync(domainEvent.OrderId);

        // 设置订单的买家ID和支付方式ID
        orderToUpdate.SetPaymentMethodVerified(domainEvent.Buyer.Id, domainEvent.Payment.Id);

        // 记录订单支付方式更新的日志
        OrderingApiTrace.LogOrderPaymentMethodUpdated(_logger, domainEvent.OrderId, nameof(domainEvent.Payment), domainEvent.Payment.Id);
    }
}
