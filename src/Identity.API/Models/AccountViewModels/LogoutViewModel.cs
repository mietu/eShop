namespace eShop.Identity.API.Models.AccountViewModels
{
    /// <summary>
    /// 表示注销视图的模型。
    /// </summary>
    /// <remarks>
    /// 该模型用于在用户注销过程中传递必要的信息。
    /// 它使用C# 9.0+的record类型，提供了值语义和不可变性。
    /// </remarks>
    public record LogoutViewModel
    {
        /// <summary>
        /// 获取或设置注销操作的唯一标识符。
        /// </summary>
        /// <remarks>
        /// 此ID通常由身份验证系统生成，用于跟踪特定的注销请求，
        /// 并可能用于防止跨站点请求伪造(CSRF)攻击。
        /// </remarks>
        public string LogoutId { get; set; }
    }
}
