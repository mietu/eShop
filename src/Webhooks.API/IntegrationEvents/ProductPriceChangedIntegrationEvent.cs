namespace Webhooks.API.IntegrationEvents;

/// <summary>
/// 表示产品价格变更的集成事件
/// </summary>
/// <param name="ProductId">变更价格的产品ID</param>
/// <param name="NewPrice">产品的新价格</param>
/// <param name="OldPrice">产品的原价格</param>
public record ProductPriceChangedIntegrationEvent(int ProductId, decimal NewPrice, decimal OldPrice) : IntegrationEvent;
