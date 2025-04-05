namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示用户成功登出后的视图模型
    /// </summary>
    public record LoggedOutViewModel
    {
        /// <summary>
        /// 登出后重定向的URI
        /// </summary>
        public string PostLogoutRedirectUri { get; init; }

        /// <summary>
        /// 客户端应用程序名称
        /// </summary>
        public string ClientName { get; init; }

        /// <summary>
        /// 用于单点登出的iframe URL
        /// </summary>
        public string SignOutIframeUrl { get; init; }
    }
}
