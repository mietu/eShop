namespace Webhooks.API.Extensions;

/// <summary>
/// 提供用于扩展 RouteHandlerBuilder 的扩展方法
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// 为路由处理程序添加 Webhook 订阅请求验证过滤器
    /// </summary>
    /// <param name="routeHandlerBuilder">要扩展的路由处理程序构建器</param>
    /// <returns>扩展后的路由处理程序构建器以支持方法链</returns>
    /// <remarks>
    /// 此过滤器将自动验证请求中的 WebhookSubscriptionRequest 对象，
    /// 如果验证失败，将返回适当的错误响应，而不会执行端点处理程序。
    /// </remarks>
    public static RouteHandlerBuilder ValidateWebhookSubscriptionRequest(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder.AddEndpointFilter(async (context, next) =>
        {
            // 从请求参数中提取 WebhookSubscriptionRequest 对象
            var webhookSubscriptionRequest = context.Arguments.OfType<WebhookSubscriptionRequest>().SingleOrDefault();

            // 检查请求中是否存在必要的 WebhookSubscriptionRequest 对象
            if (webhookSubscriptionRequest == null)
            {
                return TypedResults.BadRequest("没有找到 WebhookSubscriptionRequest。");
            }

            // 执行数据注解验证
            var validationResults = webhookSubscriptionRequest.Validate(new ValidationContext(webhookSubscriptionRequest));

            // 如果存在验证错误，返回验证问题结果
            if (validationResults.Any())
            {
                return TypedResults.ValidationProblem(validationResults.ToErrors());
            }

            // 验证通过，继续执行下一个过滤器或端点处理程序
            return await next(context);
        });
    }

    /// <summary>
    /// 将验证结果集合转换为标准错误字典格式
    /// </summary>
    /// <param name="validationResults">验证结果集合</param>
    /// <returns>错误字典，键为属性名，值为该属性的错误消息数组</returns>
    /// <remarks>
    /// 如果验证结果没有指定成员名称，将使用空字符串作为键。
    /// 对于同一属性的多个错误，将合并到同一数组中。
    /// </remarks>
    private static Dictionary<string, string[]> ToErrors(this IEnumerable<ValidationResult> validationResults)
    {
        Dictionary<string, string[]> errors = [];

        foreach (var validationResult in validationResults)
        {
            // 如果没有指定成员名称，则使用空字符串作为属性名
            var propertyNames = validationResult.MemberNames.Any() ? validationResult.MemberNames : [string.Empty];

            foreach (string propertyName in propertyNames)
            {
                // 如果该属性已经有错误，则追加新的错误消息
                if (errors.TryGetValue(propertyName, out var value))
                {
                    errors[propertyName] = [.. value, validationResult.ErrorMessage];
                }
                else
                {
                    // 否则，为该属性添加新的错误条目
                    errors.Add(propertyName, [validationResult.ErrorMessage]);
                }
            }
        }
        return errors;
    }
}
