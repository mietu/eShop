using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Webhooks.API.Extensions;

namespace Webhooks.API;

/// <summary>
/// Webhook API 端点定义类，包含所有 webhook 订阅相关的 RESTful API 端点。
/// 提供了订阅的创建、查询、删除等功能。
/// </summary>
public static class WebHooksApi
{
    /// <summary>
    /// 配置并映射 Webhook API 的 v1 版本路由
    /// </summary>
    /// <param name="app">应用程序的端点路由构建器</param>
    /// <returns>配置完成的路由组构建器</returns>
    public static RouteGroupBuilder MapWebHooksApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/webhooks").HasApiVersion(1.0);

        // 获取当前用户的所有 webhook 订阅
        api.MapGet("/", async (WebhooksContext context, ClaimsPrincipal user) =>
        {
            // 从用户声明中获取用户 ID
            var userId = user.GetUserId();
            // 查询该用户的所有订阅记录
            var data = await context.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
            return TypedResults.Ok(data);
        });

        // 根据 ID 获取特定的 webhook 订阅
        api.MapGet("/{id:int}", async Task<Results<Ok<WebhookSubscription>, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id) =>
        {
            // 从用户声明中获取用户 ID
            var userId = user.GetUserId();
            // 查询指定 ID 且属于当前用户的订阅
            var subscription = await context.Subscriptions
                .SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (subscription != null)
            {
                return TypedResults.Ok(subscription);
            }
            // 未找到订阅时返回 404
            return TypedResults.NotFound($"未找到订阅 {id}");
        });

        // 创建新的 webhook 订阅
        api.MapPost("/", async Task<Results<Created, BadRequest<string>>> (
            WebhookSubscriptionRequest request,
            IGrantUrlTesterService grantUrlTester,
            WebhooksContext context,
            ClaimsPrincipal user) =>
        {
            // 测试授权 URL 是否有效
            var grantOk = await grantUrlTester.TestGrantUrl(request.Url, request.GrantUrl, request.Token ?? string.Empty);

            if (grantOk)
            {
                // 创建新的订阅记录
                var subscription = new WebhookSubscription()
                {
                    Date = DateTime.UtcNow,          // 订阅创建时间
                    DestUrl = request.Url,           // 回调 URL
                    Token = request.Token,           // 安全令牌
                    Type = Enum.Parse<WebhookType>(request.Event, ignoreCase: true), // 事件类型
                    UserId = user.GetUserId()        // 用户 ID
                };

                // 保存到数据库
                context.Add(subscription);
                await context.SaveChangesAsync();

                // 返回创建成功状态和资源 URL
                return TypedResults.Created($"/api/webhooks/{subscription.Id}");
            }
            else
            {
                // 授权 URL 无效，返回错误信息
                return TypedResults.BadRequest($"无效授权URL: {request.GrantUrl}");
            }
        })
        .ValidateWebhookSubscriptionRequest(); // 添加模型验证

        // 删除指定 ID 的 webhook 订阅
        api.MapDelete("/{id:int}", async Task<Results<Accepted, NotFound<string>>> (
            WebhooksContext context,
            ClaimsPrincipal user,
            int id) =>
        {
            // 从用户声明中获取用户 ID
            var userId = user.GetUserId();
            // 查询指定 ID 且属于当前用户的订阅
            var subscription = await context.Subscriptions.SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (subscription != null)
            {
                // 从数据库中移除订阅
                context.Remove(subscription);
                await context.SaveChangesAsync();
                // 返回接受状态和资源 URL
                return TypedResults.Accepted($"/api/webhooks/{subscription.Id}");
            }

            // 未找到订阅时返回 404
            return TypedResults.NotFound($"未找到订阅 {id}");
        });

        return api;
    }
}
