using eShop.Ordering.Domain.SeedWork;

namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 表示订单地址的值对象
/// 作为值对象，Address是不可变的，且通过其所有属性进行相等性比较
/// </summary>
public class Address : ValueObject
{
    /// <summary>
    /// 获取或设置街道地址
    /// </summary>
    public string Street { get; private set; }

    /// <summary>
    /// 获取或设置城市
    /// </summary>
    public string City { get; private set; }

    /// <summary>
    /// 获取或设置州/省
    /// </summary>
    public string State { get; private set; }

    /// <summary>
    /// 获取或设置国家
    /// </summary>
    public string Country { get; private set; }

    /// <summary>
    /// 获取或设置邮政编码
    /// </summary>
    public string ZipCode { get; private set; }

    /// <summary>
    /// 创建一个空的地址实例
    /// 主要用于ORM框架
    /// </summary>
    public Address() { }

    /// <summary>
    /// 创建一个完整的地址实例
    /// </summary>
    /// <param name="street">街道地址</param>
    /// <param name="city">城市</param>
    /// <param name="state">州/省</param>
    /// <param name="country">国家</param>
    /// <param name="zipcode">邮政编码</param>
    public Address(string street, string city, string state, string country, string zipcode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipcode;
    }

    /// <summary>
    /// 返回用于相等性比较的组件
    /// 在值对象中，如果所有属性相等，则两个对象相等
    /// </summary>
    /// <returns>用于相等性比较的属性集合</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        // 使用yield return语句逐个返回每个元素用于相等性比较
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }
}
