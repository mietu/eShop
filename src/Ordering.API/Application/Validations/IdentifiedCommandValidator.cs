namespace eShop.Ordering.API.Application.Validations;

/// <summary>
/// 用于验证 IdentifiedCommand<CreateOrderCommand, bool> 的验证器类
/// 确保命令包含有效的唯一标识符，用于幂等性处理
/// </summary>
public class IdentifiedCommandValidator : AbstractValidator<IdentifiedCommand<CreateOrderCommand, bool>>
{
    /// <summary>
    /// 初始化 IdentifiedCommandValidator 的新实例
    /// </summary>
    /// <param name="logger">用于记录验证过程中的日志信息</param>
    public IdentifiedCommandValidator(ILogger<IdentifiedCommandValidator> logger)
    {
        // 添加验证规则：命令的Id不能为空
        RuleFor(command => command.Id).NotEmpty();

        // 在跟踪级别记录实例创建信息
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("实例已创建 - {ClassName}", GetType().Name);
        }
    }
}
