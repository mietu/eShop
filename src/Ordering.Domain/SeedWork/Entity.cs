namespace eShop.Ordering.Domain.Seedwork;

/// <summary>
/// 实体基类，为所有领域实体提供基本功能。
/// 实现了实体标识、相等性比较和领域事件处理功能。
/// </summary>
public abstract class Entity
{
    // 缓存的哈希码，用于优化GetHashCode方法
    int? _requestedHashCode;
    // 实体唯一标识符
    int _Id;

    /// <summary>
    /// 实体唯一标识符
    /// </summary>
    public virtual int Id
    {
        get
        {
            return _Id;
        }
        protected set
        {
            _Id = value;
        }
    }

    // 领域事件集合
    private List<INotification> _domainEvents;

    /// <summary>
    /// 获取实体相关的领域事件只读集合
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

    /// <summary>
    /// 添加领域事件到实体
    /// </summary>
    /// <param name="eventItem">要添加的领域事件</param>
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// 从实体移除指定的领域事件
    /// </summary>
    /// <param name="eventItem">要移除的领域事件</param>
    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    /// <summary>
    /// 清除实体的所有领域事件
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    /// <summary>
    /// 判断实体是否是临时的（未持久化的）
    /// 临时实体的Id为默认值
    /// </summary>
    /// <returns>如果实体是临时的则返回true，否则返回false</returns>
    public bool IsTransient()
    {
        return this.Id == default;
    }

    /// <summary>
    /// 重写Equals方法以基于实体ID进行相等性比较
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果对象相等则返回true，否则返回false</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Entity))
            return false;

        if (Object.ReferenceEquals(this, obj))
            return true;

        if (this.GetType() != obj.GetType())
            return false;

        Entity item = (Entity)obj;

        if (item.IsTransient() || this.IsTransient())
            return false;
        else
            return item.Id == this.Id;
    }

    /// <summary>
    /// 重写GetHashCode方法以基于实体ID生成哈希码
    /// 对于非临时实体，使用ID的哈希码与31进行异或操作以获得更均匀的分布
    /// </summary>
    /// <returns>实体的哈希码</returns>
    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

            return _requestedHashCode.Value;
        }
        else
            return base.GetHashCode();

    }

    /// <summary>
    /// 重载相等运算符（==）
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>如果两个实体相等则返回true，否则返回false</returns>
    public static bool operator ==(Entity left, Entity right)
    {
        if (Object.Equals(left, null))
            return (Object.Equals(right, null)) ? true : false;
        else
            return left.Equals(right);
    }

    /// <summary>
    /// 重载不等运算符（!=）
    /// </summary>
    /// <param name="left">左操作数</param>
    /// <param name="right">右操作数</param>
    /// <returns>如果两个实体不相等则返回true，否则返回false</returns>
    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
