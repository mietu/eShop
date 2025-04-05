namespace Webhooks.API.Model;

/// <summary>
/// 表示 Webhook 订阅请求的模型类
/// </summary>
public class WebhookSubscriptionRequest : IValidatableObject
{
    /// <summary>
    /// 获取或设置接收 Webhook 通知的 URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 获取或设置用于验证 Webhook 请求的安全令牌
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 获取或设置要订阅的事件类型名称
    /// </summary>
    public string Event { get; set; }

    /// <summary>
    /// 获取或设置授权回调 URL
    /// </summary>
    public string GrantUrl { get; set; }

    /// <summary>
    /// 验证 Webhook 订阅请求的属性
    /// </summary>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证结果的集合</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.IsWellFormedUriString(GrantUrl, UriKind.Absolute))
        {
            yield return new ValidationResult("GrantUrl is not valid", new[] { nameof(GrantUrl) });
        }

        if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
        {
            yield return new ValidationResult("Url is not valid", new[] { nameof(Url) });
        }

        var isOk = Enum.TryParse(Event, ignoreCase: true, result: out WebhookType whtype);
        if (!isOk)
        {
            yield return new ValidationResult($"{Event} is invalid event name", new[] { nameof(Event) });
        }
    }
}
