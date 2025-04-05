namespace eShop.Ordering.API.Application.Commands;

/// <summary>
/// 为命令提供唯一标识的泛型包装器类。
/// 用于命令的幂等性处理，确保相同的命令不会被处理多次。
/// </summary>
/// <typeparam name="T">被包装的命令类型，必须实现 IRequest<R></typeparam>
/// <typeparam name="R">命令执行后的返回类型</typeparam>
public class IdentifiedCommand<T, R> : IRequest<R>
    where T : IRequest<R>
{
    /// <summary>
    /// 获取被封装的原始命令
    /// </summary>
    public T Command { get; }

    /// <summary>
    /// 获取命令的唯一标识符
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 创建一个带有唯一标识的命令
    /// </summary>
    /// <param name="command">要封装的原始命令</param>
    /// <param name="id">命令的唯一标识符</param>
    public IdentifiedCommand(T command, Guid id)
    {
        Command = command;
        Id = id;
    }
}
