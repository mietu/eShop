namespace eShop.Identity.API.Models.ConsentViewModels
{
    /// <summary>
    /// 表示处理用户授权同意结果的模型。
    /// 用于在身份验证过程中，处理用户对客户端请求的权限授权结果。
    /// </summary>
    public class ProcessConsentResult
    {
        /// <summary>
        /// 指示是否应该重定向用户。
        /// 当 RedirectUri 不为 null 时，表示需要重定向。
        /// </summary>
        public bool IsRedirect => RedirectUri != null;

        /// <summary>
        /// 如果需要重定向，则表示重定向的目标 URI。
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// 请求授权的客户端信息。
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// 指示是否应向用户显示同意视图。
        /// 当 ViewModel 不为 null 时，表示需要显示同意页面。
        /// </summary>
        public bool ShowView => ViewModel != null;

        /// <summary>
        /// 包含授权同意页面所需的所有数据的视图模型。
        /// </summary>
        public ConsentViewModel ViewModel { get; set; }

        /// <summary>
        /// 指示授权过程中是否发生验证错误。
        /// 当 ValidationError 不为 null 时，表示存在验证错误。
        /// </summary>
        public bool HasValidationError => ValidationError != null;

        /// <summary>
        /// 如果授权过程中发生验证错误，则包含错误信息。
        /// </summary>
        public string ValidationError { get; set; }
    }
}
