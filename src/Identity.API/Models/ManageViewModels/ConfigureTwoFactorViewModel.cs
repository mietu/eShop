namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 用于配置双因素认证的视图模型
    /// </summary>
    public record ConfigureTwoFactorViewModel
    {
        /// <summary>
        /// 获取或设置用户选择的双因素认证提供程序
        /// </summary>
        public string SelectedProvider { get; init; }

        /// <summary>
        /// 获取或设置可用的双因素认证提供程序列表
        /// </summary>
        public ICollection<SelectListItem> Providers { get; init; }
    }
}
