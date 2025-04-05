namespace eShop.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示授权同意页面的视图模型，包含了客户端信息和请求的作用域。
    /// 用于在用户授权同意屏幕上显示应用程序请求的权限。
    /// </summary>
    public class ConsentViewModel : ConsentInputModel
    {
        /// <summary>
        /// 获取或设置客户端应用程序的名称。
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 获取或设置客户端应用程序的URL。
        /// </summary>
        public string ClientUrl { get; set; }

        /// <summary>
        /// 获取或设置客户端应用程序的Logo URL。
        /// </summary>
        public string ClientLogoUrl { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否允许用户记住他们的同意决定。
        /// </summary>
        public bool AllowRememberConsent { get; set; }

        /// <summary>
        /// 获取或设置身份相关的作用域集合，这些作用域与用户身份信息相关。
        /// </summary>
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

        /// <summary>
        /// 获取或设置API相关的作用域集合，这些作用域与API资源访问相关。
        /// </summary>
        public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
    }
}
