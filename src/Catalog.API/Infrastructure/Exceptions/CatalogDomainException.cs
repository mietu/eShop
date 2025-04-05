namespace eShop.Catalog.API.Infrastructure.Exceptions;

/// <summary>
/// 目录领域异常类，用于表示电子商店目录模块中的业务规则违反和领域逻辑错误。
/// 此异常应当在目录聚合根或领域服务中抛出，以表示业务规则的违反。
/// </summary>
public class CatalogDomainException : Exception
{
    /// <summary>
    /// 初始化 <see cref="CatalogDomainException"/> 类的新实例。
    /// </summary>
    public CatalogDomainException()
    { }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="CatalogDomainException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    public CatalogDomainException(string message)
        : base(message)
    { }

    /// <summary>
    /// 使用指定的错误消息和对作为此异常原因的内部异常的引用来初始化 <see cref="CatalogDomainException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的异常。</param>
    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
