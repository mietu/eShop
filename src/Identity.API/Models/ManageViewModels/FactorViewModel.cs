namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 表示身份验证因素的视图模型。
    /// 用于在用户管理界面中显示或收集与身份验证因素相关的信息，
    /// 例如双因素身份验证中的认证器应用、手机号码等。
    /// </summary>
    public record FactorViewModel
    {
        /// <summary>
        /// 获取或初始化此身份验证因素的用途或目的描述。
        /// 例如："谷歌验证器"、"短信认证"等。
        /// </summary>
        public string Purpose { get; init; }
    }
}
