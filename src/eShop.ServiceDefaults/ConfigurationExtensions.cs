namespace Microsoft.Extensions.Configuration;

/// <summary>
/// 提供用于配置操作的扩展方法
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// 从配置中获取必需的值。如果值不存在，则抛出异常。
    /// </summary>
    /// <param name="configuration">配置对象</param>
    /// <param name="name">配置键名</param>
    /// <returns>配置值</returns>
    /// <exception cref="InvalidOperationException">当指定的配置键不存在或值为null时抛出</exception>
    public static string GetRequiredValue(this IConfiguration configuration, string name)
    {
        return configuration[name]
            ?? throw new InvalidOperationException($"配置缺少值: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");

    }
}
