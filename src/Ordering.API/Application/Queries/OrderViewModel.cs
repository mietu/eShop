namespace eShop.Ordering.API.Application.Queries;

/// <summary>
/// 表示订单中的单个商品项
/// </summary>
public record Orderitem
{
    /// <summary>
    /// 获取或初始化商品名称
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// 获取或初始化商品数量
    /// </summary>
    public int Units { get; init; }

    /// <summary>
    /// 获取或初始化商品单价
    /// </summary>
    public double UnitPrice { get; init; }

    /// <summary>
    /// 获取或初始化商品图片URL
    /// </summary>
    public string PictureUrl { get; init; }
}

/// <summary>
/// 表示完整的订单信息，包含订单详情和配送地址
/// </summary>
public record Order
{
    /// <summary>
    /// 获取或初始化订单编号
    /// </summary>
    public int OrderNumber { get; init; }

    /// <summary>
    /// 获取或初始化订单日期
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// 获取或初始化订单状态
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    /// 获取或初始化订单描述
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// 获取或初始化配送地址的街道信息
    /// </summary>
    public string Street { get; init; }

    /// <summary>
    /// 获取或初始化配送地址的城市
    /// </summary>
    public string City { get; init; }

    /// <summary>
    /// 获取或初始化配送地址的州/省
    /// </summary>
    public string State { get; init; }

    /// <summary>
    /// 获取或初始化配送地址的邮政编码
    /// </summary>
    public string Zipcode { get; init; }

    /// <summary>
    /// 获取或初始化配送地址的国家
    /// </summary>
    public string Country { get; init; }

    /// <summary>
    /// 获取或设置订单中的商品列表
    /// </summary>
    public List<Orderitem> OrderItems { get; set; }

    /// <summary>
    /// 获取或设置订单总金额
    /// </summary>
    public decimal Total { get; set; }
}

/// <summary>
/// 表示订单的摘要信息，用于订单列表显示
/// </summary>
public record OrderSummary
{
    /// <summary>
    /// 获取或初始化订单编号
    /// </summary>
    public int OrderNumber { get; init; }

    /// <summary>
    /// 获取或初始化订单日期
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// 获取或初始化订单状态
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    /// 获取或初始化订单总金额
    /// </summary>
    public double Total { get; init; }
}

/// <summary>
/// 表示支付卡类型信息
/// </summary>
public record CardType
{
    /// <summary>
    /// 获取或初始化卡类型ID
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 获取或初始化卡类型名称
    /// </summary>
    public string Name { get; init; }
}
