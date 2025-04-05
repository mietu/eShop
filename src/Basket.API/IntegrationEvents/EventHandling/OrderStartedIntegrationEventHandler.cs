using eShop.Basket.API.IntegrationEvents.EventHandling.Events;
using eShop.Basket.API.Repositories;

namespace eShop.Basket.API.IntegrationEvents.EventHandling;

/// <summary>
/// 处理订单开始集成事件的处理器
/// 当订单流程开始时，会触发删除用户购物篮的操作
/// </summary>
public class OrderStartedIntegrationEventHandler(
    IBasketRepository repository,  // 购物篮仓储接口，用于操作购物篮数据
    ILogger<OrderStartedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    /// <summary>
    /// 处理订单开始事件
    /// </summary>
    /// <param name="event">订单开始集成事件，包含用户ID信息</param>
    /// <returns>异步任务</returns>
    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        // 记录处理集成事件的日志信息
        logger.LogInformation("处理集成事件：{集成事件 ID} - （{@Integration 事件}）", @event.Id, @event);

        // 当订单开始后，删除用户的购物篮，因为商品已转移到订单中
        await repository.DeleteBasketAsync(@event.UserId);
    }
}
