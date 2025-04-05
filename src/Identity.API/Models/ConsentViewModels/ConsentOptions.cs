namespace eShop.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 提供用户同意页面的配置选项
    /// </summary>
    public class ConsentOptions
    {
        /// <summary>
        /// 指示是否启用离线访问功能
        /// </summary>
        public static bool EnableOfflineAccess = true;

        /// <summary>
        /// 离线访问功能的显示名称
        /// </summary>
        public static string OfflineAccessDisplayName = "Offline Access";

        /// <summary>
        /// 离线访问功能的描述文本
        /// </summary>
        public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

        /// <summary>
        /// 当用户未选择任何权限时显示的错误消息
        /// </summary>
        public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";

        /// <summary>
        /// 当用户选择无效时显示的错误消息
        /// </summary>
        public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
    }
}
