namespace eShop.Ordering.API.Application.Validations;
/// <summary>
/// CreateOrderCommand 命令的验证器
/// 使用 FluentValidation 框架验证订单创建命令的所有必要字段
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// 初始化 CreateOrderCommandValidator 的新实例
    /// </summary>
    /// <param name="logger">用于记录验证过程中的日志</param>
    public CreateOrderCommandValidator(ILogger<CreateOrderCommandValidator> logger)
    {
        // 地址信息验证
        RuleFor(command => command.City).NotEmpty(); // 城市不能为空
        RuleFor(command => command.Street).NotEmpty(); // 街道不能为空
        RuleFor(command => command.State).NotEmpty(); // 州/省不能为空
        RuleFor(command => command.Country).NotEmpty(); // 国家不能为空
        RuleFor(command => command.ZipCode).NotEmpty(); // 邮政编码不能为空

        // 信用卡信息验证
        RuleFor(command => command.CardNumber).NotEmpty().Length(12, 19); // 卡号不能为空且长度必须在12到19之间
        RuleFor(command => command.CardHolderName).NotEmpty(); // 持卡人姓名不能为空
        RuleFor(command => command.CardExpiration).NotEmpty().Must(BeValidExpirationDate).WithMessage("请指定有效的卡片到期日期"); // 到期日期必须有效
        RuleFor(command => command.CardSecurityNumber).NotEmpty().Length(3); // 安全码不能为空且长度必须为3
        RuleFor(command => command.CardTypeId).NotEmpty(); // 卡类型ID不能为空

        // 订单项验证
        RuleFor(command => command.OrderItems).Must(ContainOrderItems).WithMessage("未找到订单项"); // 必须包含至少一个订单项

        // 记录追踪日志
        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("实例已创建 - {ClassName}", GetType().Name);
        }
    }

    /// <summary>
    /// 验证信用卡到期日期是否有效
    /// </summary>
    /// <param name="dateTime">要验证的到期日期</param>
    /// <returns>如果日期大于或等于当前UTC时间则返回true，否则返回false</returns>
    private bool BeValidExpirationDate(DateTime dateTime)
    {
        return dateTime >= DateTime.UtcNow;
    }

    /// <summary>
    /// 验证订单是否包含至少一个订单项
    /// </summary>
    /// <param name="orderItems">订单项集合</param>
    /// <returns>如果集合中包含至少一个项则返回true，否则返回false</returns>
    private bool ContainOrderItems(IEnumerable<OrderItemDTO> orderItems)
    {
        return orderItems.Any();
    }
}
