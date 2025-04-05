using System.Diagnostics.CodeAnalysis;
using eShop.Basket.API.Model;
using eShop.Basket.API.Repositories;

namespace eShop.Basket.API.Grpc;

/// <summary>
/// gRPC服务，提供购物篮相关操作的实现
/// 继承自自动生成的Basket.BasketBase基类
/// </summary>
public class BasketService(
    IBasketRepository repository,
    ILogger<BasketService> logger) : Basket.BasketBase
{
    /// <summary>
    /// 获取用户购物篮
    /// 允许匿名访问此接口
    /// </summary>
    /// <param name="request">获取购物篮的请求</param>
    /// <param name="context">服务器调用上下文</param>
    /// <returns>用户的购物篮信息</returns>
    [AllowAnonymous]
    public override async Task<CustomerBasketResponse> GetBasket(GetBasketRequest request, ServerCallContext context)
    {
        // 从上下文中获取用户标识
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            // 用户未认证，返回空购物篮
            return new();
        }

        // 记录调试日志
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Begin GetBasketById call from method {Method} for basket id {Id}", context.Method, userId);
        }

        // 从仓储中获取用户购物篮
        var data = await repository.GetBasketAsync(userId);

        if (data is not null)
        {
            // 将领域模型转换为响应模型
            return MapToCustomerBasketResponse(data);
        }

        // 用户没有购物篮，返回空对象
        return new();
    }

    /// <summary>
    /// 更新用户购物篮
    /// </summary>
    /// <param name="request">更新购物篮的请求，包含购物篮项目</param>
    /// <param name="context">服务器调用上下文</param>
    /// <returns>更新后的购物篮信息</returns>
    public override async Task<CustomerBasketResponse> UpdateBasket(UpdateBasketRequest request, ServerCallContext context)
    {
        // 从上下文中获取用户标识
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            // 用户未认证，抛出未认证异常
            ThrowNotAuthenticated();
        }

        // 记录调试日志
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Begin UpdateBasket call from method {Method} for basket id {Id}", context.Method, userId);
        }

        // 将请求模型转换为领域模型
        var customerBasket = MapToCustomerBasket(userId, request);

        // 更新仓储中的购物篮
        var response = await repository.UpdateBasketAsync(customerBasket);
        if (response is null)
        {
            // 更新失败，购物篮不存在
            ThrowBasketDoesNotExist(userId);
        }

        // 将更新后的领域模型转换为响应模型
        return MapToCustomerBasketResponse(response);
    }

    /// <summary>
    /// 删除用户购物篮
    /// </summary>
    /// <param name="request">删除购物篮的请求</param>
    /// <param name="context">服务器调用上下文</param>
    /// <returns>删除结果响应</returns>
    public override async Task<DeleteBasketResponse> DeleteBasket(DeleteBasketRequest request, ServerCallContext context)
    {
        // 从上下文中获取用户标识
        var userId = context.GetUserIdentity();
        if (string.IsNullOrEmpty(userId))
        {
            // 用户未认证，抛出未认证异常
            ThrowNotAuthenticated();
        }

        // 从仓储中删除用户购物篮
        await repository.DeleteBasketAsync(userId);

        // 返回空响应表示成功
        return new();
    }

    /// <summary>
    /// 抛出未认证异常的辅助方法
    /// </summary>
    [DoesNotReturn]
    private static void ThrowNotAuthenticated() => throw new RpcException(new Status(StatusCode.Unauthenticated, "The caller is not authenticated."));

    /// <summary>
    /// 抛出购物篮不存在异常的辅助方法
    /// </summary>
    /// <param name="userId">用户ID</param>
    [DoesNotReturn]
    private static void ThrowBasketDoesNotExist(string userId) => throw new RpcException(new Status(StatusCode.NotFound, $"Basket with buyer id {userId} does not exist"));

    /// <summary>
    /// 将领域模型转换为gRPC响应模型
    /// </summary>
    /// <param name="customerBasket">购物篮领域模型</param>
    /// <returns>gRPC响应模型</returns>
    private static CustomerBasketResponse MapToCustomerBasketResponse(CustomerBasket customerBasket)
    {
        var response = new CustomerBasketResponse();

        foreach (var item in customerBasket.Items)
        {
            response.Items.Add(new BasketItem()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }

    /// <summary>
    /// 将gRPC请求模型转换为领域模型
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="customerBasketRequest">购物篮请求模型</param>
    /// <returns>购物篮领域模型</returns>
    private static CustomerBasket MapToCustomerBasket(string userId, UpdateBasketRequest customerBasketRequest)
    {
        var response = new CustomerBasket
        {
            BuyerId = userId
        };

        foreach (var item in customerBasketRequest.Items)
        {
            response.Items.Add(new()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            });
        }

        return response;
    }
}
