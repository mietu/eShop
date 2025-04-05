namespace eShop.Ordering.API.Application.Validations;

/// <summary>
/// 取消订单命令的验证器
/// </summary>
/// <remarks>
/// 此验证器负责验证取消订单命令中的数据是否有效，
/// 确保命令中包含必要的订单编号信息
/// </remarks>
public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    /// <summary>
    /// 初始化取消订单命令验证器的新实例
    /// </summary>
    /// <param name="logger">用于记录验证过程中的日志信息</param>
    public CancelOrderCommandValidator(ILogger<CancelOrderCommandValidator> logger)
    {
        // 验证订单编号不能为空
        RuleFor(order => order.OrderNumber).NotEmpty().WithMessage("未找到订单 ID");

        // 在跟踪级别记录实例创建信息
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("实例已创建 - {ClassName}", GetType().Name);
        }
    }
}
