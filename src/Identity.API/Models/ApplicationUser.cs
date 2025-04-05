namespace eShop.Identity.API.Models
{
    /// <summary>
    /// 应用程序用户类，扩展了IdentityUser以包含额外的用户信息，
    /// 如支付和地址信息
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 用户支付卡号
        /// </summary>
        [Required]
        public string CardNumber { get; set; }

        /// <summary>
        /// 卡片安全码/CVV码
        /// </summary>
        [Required]
        public string SecurityNumber { get; set; }

        /// <summary>
        /// 卡片有效期，格式为MM/YY
        /// </summary>
        [Required]
        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        public string Expiration { get; set; }

        /// <summary>
        /// 卡片持有人姓名
        /// </summary>
        [Required]
        public string CardHolderName { get; set; }

        /// <summary>
        /// 卡片类型(如Visa, MasterCard等)的枚举值
        /// </summary>
        public int CardType { get; set; }

        /// <summary>
        /// 用户街道地址
        /// </summary>
        [Required]
        public string Street { get; set; }

        /// <summary>
        /// 用户所在城市
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// 用户所在州/省
        /// </summary>
        [Required]
        public string State { get; set; }

        /// <summary>
        /// 用户所在国家
        /// </summary>
        [Required]
        public string Country { get; set; }

        /// <summary>
        /// 用户邮政编码
        /// </summary>
        [Required]
        public string ZipCode { get; set; }

        /// <summary>
        /// 用户名字
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 用户姓氏
        /// </summary>
        [Required]
        public string LastName { get; set; }
    }
}
