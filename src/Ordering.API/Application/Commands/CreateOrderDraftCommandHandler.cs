namespace eShop.Ordering.API.Application.Commands;

using eShop.Ordering.API.Extensions;
using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 创建订单草稿的命令处理器
/// 负责将购物篮中的商品转换为订单草稿
/// </summary>
public class CreateOrderDraftCommandHandler
    : IRequestHandler<CreateOrderDraftCommand, OrderDraftDTO>
{
    /// <summary>
    /// 处理创建订单草稿的命令
    /// </summary>
    /// <param name="message">包含要转换为订单草稿的商品信息的命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示订单草稿的DTO</returns>
    public Task<OrderDraftDTO> Handle(CreateOrderDraftCommand message, CancellationToken cancellationToken)
    {
        // 创建一个新的订单草稿
        var order = Order.NewDraft();
        // 将命令中的商品转换为订单项DTO
        var orderItems = message.Items.Select(i => i.ToOrderItemDTO());
        // 遍历所有订单项并添加到订单中
        foreach (var item in orderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        // 将领域模型转换为DTO并返回
        return Task.FromResult(OrderDraftDTO.FromOrder(order));
    }
}

/// <summary>
/// 订单草稿的数据传输对象
/// 用于在API层和应用层之间传递订单草稿数据
/// </summary>
public record OrderDraftDTO
{
    /// <summary>
    /// 订单中的商品项集合
    /// </summary>
    public IEnumerable<OrderItemDTO> OrderItems { get; init; }

    /// <summary>
    /// 订单总金额
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// 从领域模型创建订单草稿DTO的工厂方法
    /// </summary>
    /// <param name="order">订单领域模型</param>
    /// <returns>订单草稿DTO</returns>
    public static OrderDraftDTO FromOrder(Order order)
    {
        return new OrderDraftDTO()
        {
            // 将领域模型中的订单项转换为DTO
            OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
            {
                Discount = oi.Discount,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                PictureUrl = oi.PictureUrl,
                Units = oi.Units,
                ProductName = oi.ProductName
            }),
            // 计算订单总金额
            Total = order.GetTotal()
        };
    }
}

/// <summary>
/// 订单项的数据传输对象
/// 表示订单中的单个商品
/// </summary>
public record OrderItemDTO
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// 商品单价
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// 商品折扣金额
    /// </summary>
    public decimal Discount { get; init; }

    /// <summary>
    /// 商品数量
    /// </summary>
    public int Units { get; init; }

    /// <summary>
    /// 商品图片URL
    /// </summary>
    public string PictureUrl { get; init; }
}
