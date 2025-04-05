namespace eShop.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示同意视图中的权限范围的视图模型
    /// </summary>
    public class ScopeViewModel
    {
        /// <summary>
        /// 获取或设置权限范围的标识值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 获取或设置权限范围的显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 获取或设置权限范围的详细描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示此权限范围是否应该在UI中被强调显示
        /// </summary>
        public bool Emphasize { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示此权限范围是否为必需的
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 获取或设置一个值，指示此权限范围是否被用户选中
        /// </summary>
        public bool Checked { get; set; }
    }
}
