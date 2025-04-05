namespace eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;

/// <summary>
/// 创建订单草稿的命令
/// 该命令用于根据用户购物篮中的商品创建订单草稿
/// 实现了IRequest接口，返回OrderDraftDTO类型的响应
/// </summary>
/// <param name="BuyerId">购买者的唯一标识符</param>
/// <param name="Items">购物篮中的商品集合</param>
public record CreateOrderDraftCommand(string BuyerId, IEnumerable<BasketItem> Items)
    : IRequest<OrderDraftDTO>;
