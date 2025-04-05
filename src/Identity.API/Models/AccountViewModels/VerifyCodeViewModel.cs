namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示验证双因素认证代码的视图模型
    /// </summary>
    public record VerifyCodeViewModel
    {
        /// <summary>
        /// 获取或初始化认证提供程序的名称
        /// </summary>
        [Required]
        public string Provider { get; init; }

        /// <summary>
        /// 获取或初始化用户提供的验证码
        /// </summary>
        [Required]
        public string Code { get; init; }

        /// <summary>
        /// 获取或初始化验证成功后的返回URL
        /// </summary>
        public string ReturnUrl { get; init; }

        /// <summary>
        /// 获取或初始化一个值，指示是否在当前浏览器中保存验证信息
        /// </summary>
        [Display(Name = "记住浏览器?")]
        public bool RememberBrowser { get; init; }

        /// <summary>
        /// 获取或初始化一个值，指示是否记住当前用户
        /// </summary>
        [Display(Name = "记住我?")]
        public bool RememberMe { get; init; }
    }
}
