namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示忘记密码视图模型，用于用户请求密码重置时收集必要信息。
    /// </summary>
    public record ForgotPasswordViewModel
    {
        /// <summary>
        /// 获取或初始化用户的电子邮件地址。
        /// 该邮件地址将用于发送密码重置链接。
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; init; }
    }
}
