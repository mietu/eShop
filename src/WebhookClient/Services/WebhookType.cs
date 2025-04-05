namespace eShop.WebhookClient.Services;

/// <summary>
/// 定义系统中支持的各种 Webhook 事件类型
/// </summary>
public enum WebhookType
{
    /// <summary>
    /// 目录商品价格变更事件
    /// </summary>
    CatalogItemPriceChange = 1,

    /// <summary>
    /// 订单已发货事件
    /// </summary>
    OrderShipped = 2,

    /// <summary>
    /// 订单已支付事件
    /// </summary>
    OrderPaid = 3
}
