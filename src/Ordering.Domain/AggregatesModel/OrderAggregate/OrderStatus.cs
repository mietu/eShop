using System.Text.Json.Serialization;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 表示订单在处理流程中的各种状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    /// <summary>
    /// 订单已提交，等待进一步处理
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// 订单正在等待验证（如地址确认、商品可用性等）
    /// </summary>
    AwaitingValidation = 2,

    /// <summary>
    /// 库存已确认，商品可供发货
    /// </summary>
    StockConfirmed = 3,

    /// <summary>
    /// 订单已支付
    /// </summary>
    Paid = 4,

    /// <summary>
    /// 订单已发货
    /// </summary>
    Shipped = 5,

    /// <summary>
    /// 订单已取消
    /// </summary>
    Cancelled = 6
}
