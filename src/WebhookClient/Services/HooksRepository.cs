using System.Collections.Concurrent;

namespace eShop.WebhookClient.Services;

/// <summary>
/// 存储和管理接收到的webhook的仓库
/// 提供添加、获取webhook以及变更通知订阅功能
/// </summary>
public class HooksRepository
{
    // 使用线程安全的队列存储webhook数据
    private readonly ConcurrentQueue<WebHookReceived> _data = new();

    // 使用线程安全的字典存储订阅关系，值不重要，仅用作标记
    private readonly ConcurrentDictionary<OnChangeSubscription, object?> _onChangeSubscriptions = new();

    /// <summary>
    /// 添加新的webhook并通知所有订阅者
    /// </summary>
    /// <param name="hook">接收到的webhook数据</param>
    /// <returns>表示异步操作的任务</returns>
    public Task AddNew(WebHookReceived hook)
    {
        // 将新webhook加入队列
        _data.Enqueue(hook);

        // 通知所有订阅者
        foreach (var subscription in _onChangeSubscriptions)
        {
            try
            {
                // 异步触发订阅者的回调但不等待其完成
                _ = subscription.Key.NotifyAsync();
            }
            catch (Exception)
            {
                // 忽略订阅者回调中的异常
                // 它是订阅者自身的责任处理其回调中的异常
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取所有存储的webhook
    /// </summary>
    /// <returns>包含所有webhook的集合</returns>
    public Task<IEnumerable<WebHookReceived>> GetAll()
    {
        return Task.FromResult(_data.AsEnumerable());
    }

    /// <summary>
    /// 订阅webhook变更通知
    /// </summary>
    /// <param name="callback">当有新webhook时要执行的回调</param>
    /// <returns>可用于取消订阅的IDisposable对象</returns>
    public IDisposable Subscribe(Func<Task> callback)
    {
        var subscription = new OnChangeSubscription(callback, this);
        _onChangeSubscriptions.TryAdd(subscription, null);
        return subscription;
    }

    /// <summary>
    /// 表示webhook变更的订阅
    /// 实现IDisposable接口以支持取消订阅
    /// </summary>
    private class OnChangeSubscription(Func<Task> callback, HooksRepository owner) : IDisposable
    {
        /// <summary>
        /// 触发订阅回调
        /// </summary>
        /// <returns>表示回调执行的任务</returns>
        public Task NotifyAsync() => callback();

        /// <summary>
        /// 取消订阅并释放资源
        /// </summary>
        public void Dispose() => owner._onChangeSubscriptions.Remove(this, out _);
    }
}
