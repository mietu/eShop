namespace eShop.Ordering.API.Application.Queries;

/// <summary>
/// 提供订单相关的查询服务
/// </summary>
/// <remarks>
/// 该类负责从数据库中查询订单信息，并将领域模型转换为客户端可用的DTO
/// </remarks>
public class OrderQueries(OrderingContext context)
    : IOrderQueries
{
    /// <summary>
    /// 根据订单ID获取订单详情
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>包含订单详细信息的DTO</returns>
    /// <exception cref="KeyNotFoundException">当指定ID的订单不存在时抛出</exception>
    public async Task<Order> GetOrderAsync(int id)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            throw new KeyNotFoundException();

        return new Order
        {
            OrderNumber = order.Id,
            Date = order.OrderDate,
            Description = order.Description,
            City = order.Address.City,
            Country = order.Address.Country,
            State = order.Address.State,
            Street = order.Address.Street,
            Zipcode = order.Address.ZipCode,
            Status = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(oi => new Orderitem
            {
                ProductName = oi.ProductName,
                Units = oi.Units,
                UnitPrice = (double)oi.UnitPrice,
                PictureUrl = oi.PictureUrl
            }).ToList()
        };
    }

    /// <summary>
    /// 获取指定用户的所有订单摘要信息
    /// </summary>
    /// <param name="userId">用户标识ID</param>
    /// <returns>该用户的订单摘要集合</returns>
    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
    {
        return await context.Orders
            .Where(o => o.Buyer.IdentityGuid == userId)
            .Select(o => new OrderSummary
            {
                OrderNumber = o.Id,
                Date = o.OrderDate,
                Status = o.OrderStatus.ToString(),
                Total = (double)o.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
            .ToListAsync();
    }

    /// <summary>
    /// 获取所有支付卡类型
    /// </summary>
    /// <returns>支付卡类型集合</returns>
    public async Task<IEnumerable<CardType>> GetCardTypesAsync() =>
        await context.CardTypes.Select(c => new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
}
