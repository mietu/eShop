namespace eShop.Ordering.API.Application.Commands;

// DDD and CQRS patterns comment: Note that it is recommended to implement immutable Commands
// In this case, its immutability is achieved by having all the setters as private
// plus only being able to update the data just once, when creating the object through its constructor.
// References on Immutable Commands:  
// http://cqrs.nu/Faq
// https://docs.spine3.org/motivation/immutability.html 
// http://blog.gauffin.org/2012/06/griffin-container-introducing-command-support/
// https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-a-lightweight-class-with-auto-implemented-properties

using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Extensions;

/// <summary>
/// 创建订单的命令类，实现IRequest接口用于MediatR处理
/// 遵循CQRS模式的不可变命令设计
/// </summary>
[DataContract]
public class CreateOrderCommand
    : IRequest<bool>
{
    /// <summary>
    /// 订单项列表
    /// </summary>
    [DataMember]
    private readonly List<OrderItemDTO> _orderItems;

    /// <summary>
    /// 用户ID
    /// </summary>
    [DataMember]
    public string UserId { get; private set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [DataMember]
    public string UserName { get; private set; }

    /// <summary>
    /// 城市
    /// </summary>
    [DataMember]
    public string City { get; private set; }

    /// <summary>
    /// 街道
    /// </summary>
    [DataMember]
    public string Street { get; private set; }

    /// <summary>
    /// 州/省
    /// </summary>
    [DataMember]
    public string State { get; private set; }

    /// <summary>
    /// 国家
    /// </summary>
    [DataMember]
    public string Country { get; private set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [DataMember]
    public string ZipCode { get; private set; }

    /// <summary>
    /// 信用卡卡号
    /// </summary>
    [DataMember]
    public string CardNumber { get; private set; }

    /// <summary>
    /// 信用卡持卡人姓名
    /// </summary>
    [DataMember]
    public string CardHolderName { get; private set; }

    /// <summary>
    /// 信用卡到期日期
    /// </summary>
    [DataMember]
    public DateTime CardExpiration { get; private set; }

    /// <summary>
    /// 信用卡安全码
    /// </summary>
    [DataMember]
    public string CardSecurityNumber { get; private set; }

    /// <summary>
    /// 信用卡类型ID
    /// </summary>
    [DataMember]
    public int CardTypeId { get; private set; }

    /// <summary>
    /// 获取订单项集合
    /// </summary>
    [DataMember]
    public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

    /// <summary>
    /// 默认构造函数，初始化一个空的订单项列表
    /// </summary>
    public CreateOrderCommand()
    {
        _orderItems = new List<OrderItemDTO>();
    }

    /// <summary>
    /// 参数化构造函数，根据提供的购物篮项和用户信息创建订单
    /// </summary>
    /// <param name="basketItems">购物篮中的商品项列表</param>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="city">城市</param>
    /// <param name="street">街道</param>
    /// <param name="state">州/省</param>
    /// <param name="country">国家</param>
    /// <param name="zipcode">邮政编码</param>
    /// <param name="cardNumber">信用卡卡号</param>
    /// <param name="cardHolderName">信用卡持卡人姓名</param>
    /// <param name="cardExpiration">信用卡到期日期</param>
    /// <param name="cardSecurityNumber">信用卡安全码</param>
    /// <param name="cardTypeId">信用卡类型ID</param>
    public CreateOrderCommand(List<BasketItem> basketItems, string userId, string userName, string city, string street, string state, string country, string zipcode,
        string cardNumber, string cardHolderName, DateTime cardExpiration,
        string cardSecurityNumber, int cardTypeId)
    {
        _orderItems = basketItems.ToOrderItemsDTO().ToList();
        UserId = userId;
        UserName = userName;
        City = city;
        Street = street;
        State = state;
        Country = country;
        ZipCode = zipcode;
        CardNumber = cardNumber;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
        CardSecurityNumber = cardSecurityNumber;
        CardTypeId = cardTypeId;
    }
}

