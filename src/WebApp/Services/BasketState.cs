using System.Security.Claims;
using eShop.WebAppComponents.Catalog;
using eShop.WebAppComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace eShop.WebApp.Services;

/// <summary>
/// 购物篮状态管理类，负责处理用户购物篮的所有操作
/// </summary>
public class BasketState(
    BasketService basketService,
    CatalogService catalogService,
    OrderingService orderingService,
    AuthenticationStateProvider authenticationStateProvider) : IBasketState
{
    /// <summary>
    /// 缓存的购物篮项目集合
    /// </summary>
    private Task<IReadOnlyCollection<BasketItem>>? _cachedBasket;

    /// <summary>
    /// 购物篮状态变更订阅集合
    /// </summary>
    private HashSet<BasketStateChangedSubscription> _changeSubscriptions = new();

    /// <summary>
    /// 删除当前用户的购物篮
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public Task DeleteBasketAsync()
        => basketService.DeleteBasketAsync();

    /// <summary>
    /// 获取当前用户的购物篮项目
    /// 如果用户未认证，返回空集合
    /// </summary>
    /// <returns>购物篮项目的只读集合</returns>
    public async Task<IReadOnlyCollection<BasketItem>> GetBasketItemsAsync()
        => (await GetUserAsync()).Identity?.IsAuthenticated == true
        ? await FetchBasketItemsAsync()
        : [];

    /// <summary>
    /// 订阅购物篮状态变更通知
    /// </summary>
    /// <param name="callback">状态变更时触发的回调</param>
    /// <returns>用于取消订阅的IDisposable对象</returns>
    public IDisposable NotifyOnChange(EventCallback callback)
    {
        var subscription = new BasketStateChangedSubscription(this, callback);
        _changeSubscriptions.Add(subscription);
        return subscription;
    }

    /// <summary>
    /// 向购物篮添加商品
    /// 如果商品已存在，则增加数量，否则添加新项目
    /// </summary>
    /// <param name="item">要添加的商品</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task AddAsync(CatalogItem item)
    {
        var items = (await FetchBasketItemsAsync()).Select(i => new BasketQuantity(i.ProductId, i.Quantity)).ToList();
        bool found = false;
        for (var i = 0; i < items.Count; i++)
        {
            var existing = items[i];
            if (existing.ProductId == item.Id)
            {
                items[i] = existing with { Quantity = existing.Quantity + 1 };
                found = true;
                break;
            }
        }

        if (!found)
        {
            items.Add(new BasketQuantity(item.Id, 1));
        }

        _cachedBasket = null;
        await basketService.UpdateBasketAsync(items);
        await NotifyChangeSubscribersAsync();
    }

    /// <summary>
    /// 设置购物篮中特定商品的数量
    /// 如果数量为零或负数，则从购物篮中移除该商品
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="quantity">新的数量</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SetQuantityAsync(int productId, int quantity)
    {
        var existingItems = (await FetchBasketItemsAsync()).ToList();
        if (existingItems.FirstOrDefault(row => row.ProductId == productId) is { } row)
        {
            if (quantity > 0)
            {
                row.Quantity = quantity;
            }
            else
            {
                existingItems.Remove(row);
            }

            _cachedBasket = null;
            await basketService.UpdateBasketAsync(existingItems.Select(i => new BasketQuantity(i.ProductId, i.Quantity)).ToList());
            await NotifyChangeSubscribersAsync();
        }
    }

    /// <summary>
    /// 结算购物篮并创建订单
    /// </summary>
    /// <param name="checkoutInfo">结算信息</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task CheckoutAsync(BasketCheckoutInfo checkoutInfo)
    {
        if (checkoutInfo.RequestId == default)
        {
            checkoutInfo.RequestId = Guid.NewGuid();
        }

        var buyerId = await authenticationStateProvider.GetBuyerIdAsync() ?? throw new InvalidOperationException("User does not have a buyer ID");
        var userName = await authenticationStateProvider.GetUserNameAsync() ?? throw new InvalidOperationException("User does not have a user name");

        // 获取购物篮中所有商品的详细信息
        var orderItems = await FetchBasketItemsAsync();

        // 调用Ordering.API创建订单
        var request = new CreateOrderRequest(
            UserId: buyerId,
            UserName: userName,
            City: checkoutInfo.City!,
            Street: checkoutInfo.Street!,
            State: checkoutInfo.State!,
            Country: checkoutInfo.Country!,
            ZipCode: checkoutInfo.ZipCode!,
            CardNumber: "1111222233334444",
            CardHolderName: "TESTUSER",
            CardExpiration: DateTime.UtcNow.AddYears(1),
            CardSecurityNumber: "111",
            CardTypeId: checkoutInfo.CardTypeId,
            Buyer: buyerId,
            Items: [.. orderItems]);
        await orderingService.CreateOrder(request, checkoutInfo.RequestId);
        await DeleteBasketAsync();
    }

    /// <summary>
    /// 通知所有订阅者购物篮状态已变更
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    private Task NotifyChangeSubscribersAsync()
        => Task.WhenAll(_changeSubscriptions.Select(s => s.NotifyAsync()));

    /// <summary>
    /// 获取当前用户的ClaimsPrincipal对象
    /// </summary>
    /// <returns>用户的ClaimsPrincipal对象</returns>
    private async Task<ClaimsPrincipal> GetUserAsync()
        => (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

    /// <summary>
    /// 获取购物篮项目，优先使用缓存
    /// </summary>
    /// <returns>购物篮项目的只读集合</returns>
    private Task<IReadOnlyCollection<BasketItem>> FetchBasketItemsAsync()
    {
        return _cachedBasket ??= FetchCoreAsync();

        async Task<IReadOnlyCollection<BasketItem>> FetchCoreAsync()
        {
            var quantities = await basketService.GetBasketAsync();
            if (quantities.Count == 0)
            {
                return [];
            }

            // 获取购物篮中商品的详细信息
            var basketItems = new List<BasketItem>();
            var productIds = quantities.Select(row => row.ProductId);
            var catalogItems = (await catalogService.GetCatalogItems(productIds)).ToDictionary(k => k.Id, v => v);
            foreach (var item in quantities)
            {
                var catalogItem = catalogItems[item.ProductId];
                var orderItem = new BasketItem
                {
                    Id = Guid.NewGuid().ToString(), // TODO: 此值无实际意义，应使用ProductId替代
                    ProductId = catalogItem.Id,
                    ProductName = catalogItem.Name,
                    UnitPrice = catalogItem.Price,
                    Quantity = item.Quantity,
                };
                basketItems.Add(orderItem);
            }

            return basketItems;
        }
    }

    /// <summary>
    /// 购物篮状态变更订阅类，用于处理状态变更通知
    /// </summary>
    private class BasketStateChangedSubscription(BasketState Owner, EventCallback Callback) : IDisposable
    {
        /// <summary>
        /// 通知订阅者状态已变更
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public Task NotifyAsync() => Callback.InvokeAsync();

        /// <summary>
        /// 取消订阅并从集合中移除
        /// </summary>
        public void Dispose() => Owner._changeSubscriptions.Remove(this);
    }
}

/// <summary>
/// 创建订单请求记录，包含订单的所有必要信息
/// </summary>
public record CreateOrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);
