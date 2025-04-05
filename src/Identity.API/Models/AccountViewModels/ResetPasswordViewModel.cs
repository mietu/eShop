namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 视图模型，用于重置密码页面，包含用户输入的电子邮件、新密码和验证码信息
    /// </summary>
    public record ResetPasswordViewModel
    {
        /// <summary>
        /// 用户的电子邮件地址，用于识别要重置密码的账户
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; init; }

        /// <summary>
        /// 用户设置的新密码，必须符合长度和复杂度要求
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; init; }

        /// <summary>
        /// 确认密码，必须与新密码完全匹配
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }

        /// <summary>
        /// 密码重置验证码，通常通过电子邮件发送给用户
        /// </summary>
        public string Code { get; init; }
    }
}
