using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace eShop.WebhookClient.Endpoints;

/// <summary>
/// 提供 Webhook 相关端点的映射功能
/// </summary>
public static class WebhookEndpoints
{
    /// <summary>
    /// 将 Webhook 相关的端点映射到应用程序路由中
    /// </summary>
    /// <param name="app">应用程序端点路由构建器</param>
    /// <returns>配置后的端点路由构建器</returns>
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // 用于 Webhook 令牌验证的 HTTP 头名称
        const string webhookCheckHeader = "X-eshop-whtoken";

        // 从配置中获取验证设置
        var configuration = app.ServiceProvider.GetRequiredService<IConfiguration>();
        bool.TryParse(configuration["ValidateToken"], out var validateToken);
        var tokenToValidate = configuration["WebhookClientOptions:Token"];

        // 映射 OPTIONS 请求处理程序，用于验证 Webhook 令牌
        app.MapMethods("/check", [HttpMethods.Options], Results<Ok, BadRequest<string>> ([FromHeader(Name = webhookCheckHeader)] string value, HttpResponse response) =>
        {
            // 如果不需要验证令牌或令牌匹配，则返回 OK
            if (!validateToken || value == tokenToValidate)
            {
                // 如果提供了有效的令牌，将其添加到响应头中
                if (!string.IsNullOrWhiteSpace(value))
                {
                    response.Headers.Append(webhookCheckHeader, value);
                }

                return TypedResults.Ok();
            }

            // 令牌验证失败，返回错误消息
            return TypedResults.BadRequest("无效令牌");
        });

        // 映射 POST 请求处理程序，用于接收和处理 Webhook 数据
        app.MapPost("/webhook-received", async (WebhookData hook, HttpRequest request, ILogger<Program> logger, HooksRepository hooksRepository) =>
        {
            // 从请求头中获取令牌
            var token = request.Headers[webhookCheckHeader];

            // 记录接收到的 Webhook 信息
            logger.LogInformation("收到带有令牌 {Token} 的hook。 我的令牌为 {MyToken}. 令牌验证设置为 {ValidateToken}", token, tokenToValidate, validateToken);

            // 验证令牌是否有效
            if (!validateToken || tokenToValidate == token)
            {
                logger.LogInformation("已接收的 hook 将被处理");

                // 创建新的 WebHookReceived 对象保存接收到的数据
                var newHook = new WebHookReceived()
                {
                    Data = hook.Payload,
                    When = hook.When,
                    Token = token
                };

                // 将新的 Webhook 数据保存到仓库中
                await hooksRepository.AddNew(newHook);
                logger.LogInformation("已处理收到的 hook。");
                return Results.Ok(newHook);
            }

            // 令牌验证失败，记录日志并返回错误
            logger.LogInformation("未处理收到的hook - 返回了错误的请求。");
            return Results.BadRequest();
        });

        return app;
    }
}
