namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 表示用户管理首页的视图模型
    /// </summary>
    public record IndexViewModel
    {
        /// <summary>
        /// 获取或设置一个值，该值指示用户是否已设置密码
        /// </summary>
        public bool HasPassword { get; init; }

        /// <summary>
        /// 获取或设置用户已关联的外部登录信息列表
        /// </summary>
        public IList<UserLoginInfo> Logins { get; init; }

        /// <summary>
        /// 获取或设置用户的手机号码
        /// </summary>
        public string PhoneNumber { get; init; }

        /// <summary>
        /// 获取或设置一个值，该值指示用户是否已启用双因素身份验证
        /// </summary>
        public bool TwoFactor { get; init; }

        /// <summary>
        /// 获取或设置一个值，该值指示当前浏览器是否被记住用于身份验证
        /// </summary>
        public bool BrowserRemembered { get; init; }
    }
}
