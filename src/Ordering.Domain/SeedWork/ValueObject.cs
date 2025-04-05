namespace eShop.Ordering.Domain.SeedWork;

/// <summary>
/// 表示领域驱动设计(DDD)中的值对象基类。
/// 值对象是通过其属性值而非身份来定义的对象。两个值对象如果所有属性值相等，则认为它们相等。
/// 值对象应该是不可变的，一旦创建就不应该改变其状态。
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// 实现相等运算符(==)的逻辑
    /// </summary>
    /// <param name="left">左侧比较对象</param>
    /// <param name="right">右侧比较对象</param>
    /// <returns>如果两个对象相等则返回true，否则返回false</returns>
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }
        return ReferenceEquals(left, null) || left.Equals(right);
    }

    /// <summary>
    /// 实现不等运算符(!=)的逻辑
    /// </summary>
    /// <param name="left">左侧比较对象</param>
    /// <param name="right">右侧比较对象</param>
    /// <returns>如果两个对象不相等则返回true，否则返回false</returns>
    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    /// <summary>
    /// 获取用于相等性比较的组件集合。
    /// 子类必须实现此方法并返回用于确定对象相等性的所有属性。
    /// </summary>
    /// <returns>表示值对象相等性组件的对象集合</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// 重写Object.Equals方法，用于比较两个值对象是否相等。
    /// 当两个值对象的所有相等性组件都相等时，认为这两个值对象相等。
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象</param>
    /// <returns>如果指定的对象等于当前对象，则为true；否则为false</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// 重写Object.GetHashCode方法，确保相等的值对象具有相同的哈希码。
    /// 哈希码是通过对所有相等性组件的哈希码进行XOR运算生成的。
    /// </summary>
    /// <returns>当前对象的哈希码</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// 创建并返回当前值对象的浅表副本。
    /// 由于值对象应该是不可变的，浅拷贝通常足够满足需求。
    /// </summary>
    /// <returns>当前值对象的副本</returns>
    public ValueObject GetCopy()
    {
        return this.MemberwiseClone() as ValueObject;
    }
}
