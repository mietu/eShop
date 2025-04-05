namespace eShop.WebApp.Services;

/// <summary>
/// 提供订单状态通知服务，允许客户端订阅特定买家的订单状态变更通知
/// </summary>
public class OrderStatusNotificationService
{
    // 手动锁定是因为我们需要每个键有多个值，且只需要非常短暂的锁定
    private readonly object _subscriptionsLock = new();
    /// <summary>
    /// 按买家ID存储订阅信息的字典
    /// </summary>
    private readonly Dictionary<string, HashSet<Subscription>> _subscriptionsByBuyerId = new();

    /// <summary>
    /// 订阅指定买家的订单状态变更通知
    /// </summary>
    /// <param name="buyerId">买家ID</param>
    /// <param name="callback">订单状态变更时要执行的回调函数</param>
    /// <returns>可用于取消订阅的IDisposable对象</returns>
    public IDisposable SubscribeToOrderStatusNotifications(string buyerId, Func<Task> callback)
    {
        var subscription = new Subscription(this, buyerId, callback);

        lock (_subscriptionsLock)
        {
            if (!_subscriptionsByBuyerId.TryGetValue(buyerId, out var subscriptions))
            {
                subscriptions = [];
                _subscriptionsByBuyerId.Add(buyerId, subscriptions);
            }

            subscriptions.Add(subscription);
        }

        return subscription;
    }

    /// <summary>
    /// 通知指定买家的订单状态已发生变更
    /// </summary>
    /// <param name="buyerId">买家ID</param>
    /// <returns>通知所有订阅者的任务</returns>
    public Task NotifyOrderStatusChangedAsync(string buyerId)
    {
        lock (_subscriptionsLock)
        {
            return _subscriptionsByBuyerId.TryGetValue(buyerId, out var subscriptions)
                ? Task.WhenAll(subscriptions.Select(s => s.NotifyAsync()))
                : Task.CompletedTask;
        }
    }

    /// <summary>
    /// 取消特定买家的指定订阅
    /// </summary>
    /// <param name="buyerId">买家ID</param>
    /// <param name="subscription">要取消的订阅</param>
    private void Unsubscribe(string buyerId, Subscription subscription)
    {
        lock (_subscriptionsLock)
        {
            if (_subscriptionsByBuyerId.TryGetValue(buyerId, out var subscriptions))
            {
                subscriptions.Remove(subscription);
                if (subscriptions.Count == 0)
                {
                    _subscriptionsByBuyerId.Remove(buyerId);
                }
            }
        }
    }

    /// <summary>
    /// 表示订单状态变更的订阅
    /// 实现IDisposable接口以支持在不再需要时取消订阅
    /// </summary>
    private class Subscription(OrderStatusNotificationService owner, string buyerId, Func<Task> callback) : IDisposable
    {
        /// <summary>
        /// 触发订阅的回调函数
        /// </summary>
        /// <returns>回调执行的任务</returns>
        public Task NotifyAsync()
        {
            return callback();
        }

        /// <summary>
        /// 取消订阅并释放资源
        /// </summary>
        public void Dispose()
            => owner.Unsubscribe(buyerId, this);
    }
}
