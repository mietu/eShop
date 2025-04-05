namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 视图模型，用于用户设置新密码的表单
    /// 通常用于没有本地密码的外部登录用户首次设置密码
    /// </summary>
    public record SetPasswordViewModel
    {
        /// <summary>
        /// 用户的新密码
        /// </summary>
        /// <remarks>
        /// 密码要求：
        /// - 必填字段
        /// - 长度在6到100个字符之间
        /// - 使用密码类型的数据输入控件显示
        /// </remarks>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; init; }

        /// <summary>
        /// 确认新密码，用于验证用户输入的一致性
        /// </summary>
        /// <remarks>
        /// - 必须与NewPassword属性值匹配
        /// - 使用密码类型的数据输入控件显示
        /// </remarks>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
    }
}
