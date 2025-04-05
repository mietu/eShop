namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示用户登录页面的视图模型
    /// </summary>
    public record LoginViewModel
    {
        /// <summary>
        /// 获取或设置用户的电子邮件地址，用作登录标识符
        /// </summary>
        [Required(ErrorMessage = "电子邮件是必填项")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮件地址")]
        public string Email { get; set; }

        /// <summary>
        /// 获取或设置用户的密码
        /// </summary>
        [Required(ErrorMessage = "密码是必填项")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置表示用户是否希望保持登录状态的值
        /// </summary>
        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// 获取或设置成功登录后重定向的URL
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
