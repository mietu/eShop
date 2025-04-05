
namespace eShop.Ordering.Domain.Events;

/// <summary>
/// 订单创建时发布的领域事件
/// 包含订单创建所需的所有相关信息，包括订单实体、用户信息和支付详情
/// 作为通知事件被发布，供相关处理程序订阅处理
/// </summary>
/// <param name="Order">新创建的订单实体</param>
/// <param name="UserId">创建订单的用户ID</param>
/// <param name="UserName">创建订单的用户名</param>
/// <param name="CardTypeId">支付卡类型ID</param>
/// <param name="CardNumber">支付卡号</param>
/// <param name="CardSecurityNumber">支付卡安全码</param>
/// <param name="CardHolderName">持卡人姓名</param>
/// <param name="CardExpiration">卡过期日期</param>
public record class OrderStartedDomainEvent(
    Order Order,
    string UserId,
    string UserName,
    int CardTypeId,
    string CardNumber,
    string CardSecurityNumber,
    string CardHolderName,
    DateTime CardExpiration) : INotification;
