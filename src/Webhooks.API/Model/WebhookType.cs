namespace Webhooks.API.Model;

/// <summary>
/// 定义可用的Webhook类型
/// </summary>
public enum WebhookType
{
    /// <summary>
    /// 当目录项目价格发生变化时触发
    /// </summary>
    CatalogItemPriceChange = 1,

    /// <summary>
    /// 当订单已发货时触发
    /// </summary>
    OrderShipped = 2,

    /// <summary>
    /// 当订单已支付时触发
    /// </summary>
    OrderPaid = 3,
    /// <summary>
    /// 当产品价格发生变化时触发
    /// </summary>
    ProductPriceChanged
}
