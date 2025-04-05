namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 修改密码视图模型
    /// </summary>
    public record ChangePasswordViewModel
    {
        /// <summary>
        /// 当前密码
        /// 必填项，使用密码数据类型
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; init; }

        /// <summary>
        /// 新密码
        /// 必填项，长度限制在6-100个字符之间
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; init; }

        /// <summary>
        /// 确认新密码
        /// 必须与新密码匹配
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
    }
}
