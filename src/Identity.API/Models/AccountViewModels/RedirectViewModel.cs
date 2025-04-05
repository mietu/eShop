namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 视图模型，用于处理重定向操作的URL信息
    /// </summary>
    public class RedirectViewModel
    {
        /// <summary>
        /// 获取或设置重定向的目标URL
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
