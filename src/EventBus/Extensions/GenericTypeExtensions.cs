namespace eShop.EventBus.Extensions;

/// <summary>
/// 提供用于获取类型泛型名称的扩展方法。
/// </summary>
public static class GenericTypeExtensions
{
    /// <summary>
    /// 获取类型的泛型名称。
    /// </summary>
    /// <param name="type">要获取名称的类型。</param>
    /// <returns>
    /// 如果类型是泛型类型，则返回格式为"TypeName&lt;GenericArg1,GenericArg2,...&gt;"的字符串；
    /// 否则返回类型的名称。
    /// </returns>
    /// <example>
    /// 例如：对于类型 List&lt;string&gt;，返回 "List&lt;String&gt;"
    /// </example>
    public static string GetGenericTypeName(this Type type)
    {
        string typeName;

        if (type.IsGenericType)
        {
            var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
        }
        else
        {
            typeName = type.Name;
        }

        return typeName;
    }

    /// <summary>
    /// 获取对象实例的泛型类型名称。
    /// </summary>
    /// <param name="object">要获取类型名称的对象实例。</param>
    /// <returns>对象的泛型类型名称。</returns>
    /// <example>
    /// 例如：对于 new List&lt;int&gt;() 实例，返回 "List&lt;Int32&gt;"
    /// </example>
    public static string GetGenericTypeName(this object @object)
    {
        return @object.GetType().GetGenericTypeName();
    }
}
