namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示发送验证码视图模型，用于双因素认证过程中的提供者选择和相关信息传递
    /// </summary>
    public record SendCodeViewModel
    {
        /// <summary>
        /// 获取或设置用户选择的验证提供者
        /// </summary>
        public string SelectedProvider { get; init; }

        /// <summary>
        /// 获取或设置可用的验证提供者列表
        /// </summary>
        public ICollection<SelectListItem> Providers { get; init; }

        /// <summary>
        /// 获取或设置认证成功后的返回URL
        /// </summary>
        public string ReturnUrl { get; init; }

        /// <summary>
        /// 获取或设置是否记住当前用户
        /// </summary>
        public bool RememberMe { get; init; }
    }
}
