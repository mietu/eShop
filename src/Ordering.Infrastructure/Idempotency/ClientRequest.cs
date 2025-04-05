using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Infrastructure.Idempotency;

/// <summary>
/// 表示客户端请求的记录，用于实现幂等性处理
/// </summary>
/// <remarks>
/// 幂等性确保即使同一请求被多次提交，也只会处理一次，
/// 避免重复操作带来的副作用
/// </remarks>
public class ClientRequest
{
    /// <summary>
    /// 获取或设置请求的唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 获取或设置请求的名称或操作类型
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 获取或设置请求的时间戳
    /// </summary>
    public DateTime Time { get; set; }
}
