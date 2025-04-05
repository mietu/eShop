namespace eShop.Identity.API.Models.ManageViewModels
{
    /// <summary>
    /// 用于添加电话号码的视图模型
    /// </summary>
    public record AddPhoneNumberViewModel
    {
        /// <summary>
        /// 用户的电话号码
        /// </summary>
        [Required]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; init; }
    }
}
