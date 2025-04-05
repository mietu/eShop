namespace eShop.Ordering.API.Application.Validations;

/// <summary>
/// 验证 ShipOrderCommand 的验证器
/// 使用 FluentValidation 库确保命令符合业务规则
/// </summary>
public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    /// <summary>
    /// 初始化 ShipOrderCommand 命令的验证器
    /// </summary>
    /// <param name="logger">用于记录验证器实例创建的日志器</param>
    public ShipOrderCommandValidator(ILogger<ShipOrderCommandValidator> logger)
    {
        // 验证订单号不能为空
        RuleFor(order => order.OrderNumber).NotEmpty().WithMessage("未找到订单 ID");

        // 记录验证器实例创建的跟踪日志
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("实例已创建 - {ClassName}", GetType().Name);
        }
    }
}
