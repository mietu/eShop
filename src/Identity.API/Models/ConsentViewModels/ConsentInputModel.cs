namespace eShop.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示用户对授权请求的同意输入模型
    /// </summary>
    public class ConsentInputModel
    {
        /// <summary>
        /// 获取或设置用户点击的按钮类型（如"允许"或"拒绝"）
        /// </summary>
        public string Button { get; set; }

        /// <summary>
        /// 获取或设置用户同意的作用域集合
        /// </summary>
        public IEnumerable<string> ScopesConsented { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否记住用户的同意选择
        /// </summary>
        public bool RememberConsent { get; set; }

        /// <summary>
        /// 获取或设置授权完成后的返回URL
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 获取或设置请求的描述信息
        /// </summary>
        public string Description { get; set; }
    }
}
