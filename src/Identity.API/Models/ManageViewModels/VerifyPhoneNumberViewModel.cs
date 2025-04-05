namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 视图模型，用于验证用户电话号码的过程。
    /// 当用户尝试验证其电话号码时，此视图模型用于捕获验证码和电话号码。
    /// </summary>
    public record VerifyPhoneNumberViewModel
    {
        /// <summary>
        /// 获取或初始化验证码。
        /// 这是发送到用户电话的验证码，用户需要输入以确认电话号码所有权。
        /// </summary>
        [Required(ErrorMessage = "验证码是必需的")]
        public string Code { get; init; }

        /// <summary>
        /// 获取或初始化电话号码。
        /// 用户需要验证的电话号码。
        /// </summary>
        [Required(ErrorMessage = "电话号码是必需的")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        [Display(Name = "电话号码")]
        public string PhoneNumber { get; init; }
    }
}
