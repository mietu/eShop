namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 视图模型类，用于捕获和验证用户注册过程中的数据
    /// </summary>
    public record RegisterViewModel
    {
        /// <summary>
        /// 用户的电子邮件地址，作为唯一标识符和登录名
        /// 必填项，必须符合有效的电子邮件格式
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; }

        /// <summary>
        /// 用户的密码
        /// 必填项，长度必须在6到100个字符之间
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; }

        /// <summary>
        /// 用户密码的确认字段
        /// 必须与Password字段内容完全匹配
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }

        /// <summary>
        /// 包含用户完整个人信息的ApplicationUser对象
        /// 包括信用卡信息、地址和个人详细信息
        /// </summary>
        public ApplicationUser User { get; init; }
    }
}
