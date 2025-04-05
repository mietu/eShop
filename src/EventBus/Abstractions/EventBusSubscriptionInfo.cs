using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace eShop.EventBus.Abstractions;

/// <summary>
/// 提供事件总线订阅信息，包括事件类型映射和JSON序列化选项
/// </summary>
public class EventBusSubscriptionInfo
{
    /// <summary>
    /// 获取事件名称到事件类型的映射字典
    /// </summary>
    public Dictionary<string, Type> EventTypes { get; } = [];

    /// <summary>
    /// 获取用于事件序列化和反序列化的JSON序列化选项
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);

    /// <summary>
    /// 默认JSON序列化选项，配置了类型解析器
    /// </summary>
    internal static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
        ? CreateDefaultTypeResolver()
        : JsonTypeInfoResolver.Combine()
    };

#pragma warning disable IL2026 // 禁用可能需要动态代码执行的成员的警告
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
    /// <summary>
    /// 创建默认JSON类型解析器
    /// 注意：此方法在AOT编译时可能不完全支持
    /// </summary>
    /// <returns>默认JSON类型信息解析器</returns>
    private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
        => new DefaultJsonTypeInfoResolver();
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026
}
