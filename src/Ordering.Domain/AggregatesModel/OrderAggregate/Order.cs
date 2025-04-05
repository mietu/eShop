using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单聚合根类，实现了领域驱动设计(DDD)中的聚合根模式
/// 封装了订单相关的业务规则和行为，管理订单项集合及订单状态流转
/// </summary>
public class Order
    : Entity, IAggregateRoot
{
    /// <summary>
    /// 获取或设置订单创建日期
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// 订单配送地址，采用值对象模式，作为EF Core 2.0的owned entity持久化
    /// </summary>
    [Required]
    public Address Address { get; private set; }

    /// <summary>
    /// 买家ID，可为空表示匿名订单
    /// </summary>
    public int? BuyerId { get; private set; }

    /// <summary>
    /// 买家实体引用
    /// </summary>
    public Buyer Buyer { get; }

    /// <summary>
    /// 订单当前状态
    /// </summary>
    public OrderStatus OrderStatus { get; private set; }

    /// <summary>
    /// 订单状态描述，提供人类可读的状态说明
    /// </summary>
    public string Description { get; private set; }

    // Draft orders have this set to true. Currently we don't check anywhere the draft status of an Order, but we could do it if needed
#pragma warning disable CS0414 // The field 'Order._isDraft' is assigned but its value is never used
    /// <summary>
    /// 标记订单是否为草稿状态
    /// </summary>
    private bool _isDraft;
#pragma warning restore CS0414

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method OrderAggregateRoot.AddOrderItem() which includes behavior.
    /// <summary>
    /// 订单项私有集合，遵循DDD封装原则，防止外部直接访问集合
    /// </summary>
    private readonly List<OrderItem> _orderItems;

    /// <summary>
    /// 提供订单项的只读访问，保护集合不被外部修改
    /// </summary>
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    /// 支付方式ID，可为空
    /// </summary>
    public int? PaymentId { get; private set; }

    /// <summary>
    /// 创建一个新的草稿订单
    /// </summary>
    /// <returns>新创建的草稿状态订单</returns>
    public static Order NewDraft()
    {
        var order = new Order
        {
            _isDraft = true
        };
        return order;
    }

    /// <summary>
    /// 受保护的默认构造函数，初始化订单基本属性
    /// </summary>
    protected Order()
    {
        _orderItems = new List<OrderItem>();
        _isDraft = false;
    }

    /// <summary>
    /// 创建新订单的构造函数
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="address">配送地址</param>
    /// <param name="cardTypeId">支付卡类型ID</param>
    /// <param name="cardNumber">支付卡号</param>
    /// <param name="cardSecurityNumber">支付卡安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="cardExpiration">卡过期日期</param>
    /// <param name="buyerId">买家ID，可选</param>
    /// <param name="paymentMethodId">支付方式ID，可选</param>
    public Order(string userId, string userName, Address address, int cardTypeId, string cardNumber, string cardSecurityNumber,
            string cardHolderName, DateTime cardExpiration, int? buyerId = null, int? paymentMethodId = null) : this()
    {
        BuyerId = buyerId;
        PaymentId = paymentMethodId;
        OrderStatus = OrderStatus.Submitted;
        OrderDate = DateTime.UtcNow;
        Address = address;

        // Add the OrderStarterDomainEvent to the domain events collection 
        // to be raised/dispatched when committing changes into the Database [ After DbContext.SaveChanges() ]
        AddOrderStartedDomainEvent(userId, userName, cardTypeId, cardNumber,
                                    cardSecurityNumber, cardHolderName, cardExpiration);
    }

    // DDD Patterns comment
    // This Order AggregateRoot's method "AddOrderItem()" should be the only way to add Items to the Order,
    // so any behavior (discounts, etc.) and validations are controlled by the AggregateRoot 
    // in order to maintain consistency between the whole Aggregate. 
    /// <summary>
    /// 添加订单项到订单，是向订单添加商品的唯一方法
    /// 确保所有行为（折扣等）和验证由聚合根控制，维护整个聚合的一致性
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="productName">商品名称</param>
    /// <param name="unitPrice">单价</param>
    /// <param name="discount">折扣</param>
    /// <param name="pictureUrl">商品图片URL</param>
    /// <param name="units">数量，默认为1</param>
    public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        var existingOrderForProduct = _orderItems.SingleOrDefault(o => o.ProductId == productId);

        if (existingOrderForProduct != null)
        {
            //if previous line exist modify it with higher discount  and units..
            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.SetNewDiscount(discount);
            }

            existingOrderForProduct.AddUnits(units);
        }
        else
        {
            //add validated new order item
            var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
            _orderItems.Add(orderItem);
        }
    }

    /// <summary>
    /// 设置已验证的支付方式
    /// </summary>
    /// <param name="buyerId">买家ID</param>
    /// <param name="paymentId">支付方式ID</param>
    public void SetPaymentMethodVerified(int buyerId, int paymentId)
    {
        BuyerId = buyerId;
        PaymentId = paymentId;
    }

    /// <summary>
    /// 将订单状态设置为"等待验证"
    /// 仅当订单状态为"已提交"时才可执行此操作
    /// </summary>
    public void SetAwaitingValidationStatus()
    {
        if (OrderStatus == OrderStatus.Submitted)
        {
            AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, _orderItems));
            OrderStatus = OrderStatus.AwaitingValidation;
        }
    }

    /// <summary>
    /// 将订单状态设置为"库存已确认"
    /// 仅当订单状态为"等待验证"时才可执行此操作
    /// </summary>
    public void SetStockConfirmedStatus()
    {
        if (OrderStatus == OrderStatus.AwaitingValidation)
        {
            AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));

            OrderStatus = OrderStatus.StockConfirmed;
            Description = "所有物品均已确认有可用库存。";
        }
    }

    /// <summary>
    /// 将订单状态设置为"已支付"
    /// 仅当订单状态为"库存已确认"时才可执行此操作
    /// </summary>
    public void SetPaidStatus()
    {
        if (OrderStatus == OrderStatus.StockConfirmed)
        {
            AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, OrderItems));

            OrderStatus = OrderStatus.Paid;
            Description = "付款是在模拟的“美国银行支票银行帐户结束于XX35071”进行的。";
        }
    }

    /// <summary>
    /// 将订单状态设置为"已发货"
    /// 仅当订单状态为"已支付"时才可执行此操作，否则抛出异常
    /// </summary>
    public void SetShippedStatus()
    {
        if (OrderStatus != OrderStatus.Paid)
        {
            StatusChangeException(OrderStatus.Shipped);
        }

        OrderStatus = OrderStatus.Shipped;
        Description = "T他的订单已发货。";
        AddDomainEvent(new OrderShippedDomainEvent(this));
    }

    /// <summary>
    /// 将订单状态设置为"已取消"
    /// 当订单已支付或已发货时不允许取消，将抛出异常
    /// </summary>
    public void SetCancelledStatus()
    {
        if (OrderStatus == OrderStatus.Paid ||
            OrderStatus == OrderStatus.Shipped)
        {
            StatusChangeException(OrderStatus.Cancelled);
        }

        OrderStatus = OrderStatus.Cancelled;
        Description = "订单已取消。";
        AddDomainEvent(new OrderCancelledDomainEvent(this));
    }

    /// <summary>
    /// 当商品库存不足时取消订单
    /// 仅当订单状态为"等待验证"时才可执行此操作
    /// </summary>
    /// <param name="orderStockRejectedItems">库存不足的商品ID集合</param>
    public void SetCancelledStatusWhenStockIsRejected(IEnumerable<int> orderStockRejectedItems)
    {
        if (OrderStatus == OrderStatus.AwaitingValidation)
        {
            OrderStatus = OrderStatus.Cancelled;

            var itemsStockRejectedProductNames = OrderItems
                .Where(c => orderStockRejectedItems.Contains(c.ProductId))
                .Select(c => c.ProductName);

            var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
            Description = $"产品没有库存： ({itemsStockRejectedDescription}).";
        }
    }

    /// <summary>
    /// 添加订单启动领域事件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="cardTypeId">支付卡类型ID</param>
    /// <param name="cardNumber">支付卡号</param>
    /// <param name="cardSecurityNumber">支付卡安全码</param>
    /// <param name="cardHolderName">持卡人姓名</param>
    /// <param name="cardExpiration">卡过期日期</param>
    private void AddOrderStartedDomainEvent(string userId, string userName, int cardTypeId, string cardNumber,
            string cardSecurityNumber, string cardHolderName, DateTime cardExpiration)
    {
        var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId, userName, cardTypeId,
                                                                    cardNumber, cardSecurityNumber,
                                                                    cardHolderName, cardExpiration);

        this.AddDomainEvent(orderStartedDomainEvent);
    }

    /// <summary>
    /// 抛出订单状态变更异常
    /// </summary>
    /// <param name="orderStatusToChange">尝试变更的目标状态</param>
    /// <exception cref="OrderingDomainException">当状态转换不允许时抛出</exception>
    private void StatusChangeException(OrderStatus orderStatusToChange)
    {
        throw new OrderingDomainException($"无法将订单状态从 {OrderStatus} 切换至 {orderStatusToChange}.");
    }

    /// <summary>
    /// 计算订单总金额
    /// </summary>
    /// <returns>所有订单项的总价值</returns>
    public decimal GetTotal() => _orderItems.Sum(o => o.Units * o.UnitPrice);
}
