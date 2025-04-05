using System.ComponentModel.DataAnnotations;

namespace eShop.WebApp.Services;

/// <summary>
/// 表示用户结账时需要提供的信息，包括配送地址和支付详情
/// </summary>
public class BasketCheckoutInfo
{
    /// <summary>
    /// 配送地址的街道名称
    /// </summary>
    [Required]
    public string? Street { get; set; }

    /// <summary>
    /// 配送地址的城市
    /// </summary>
    [Required]
    public string? City { get; set; }

    /// <summary>
    /// 配送地址的州/省
    /// </summary>
    [Required]
    public string? State { get; set; }

    /// <summary>
    /// 配送地址的国家
    /// </summary>
    [Required]
    public string? Country { get; set; }

    /// <summary>
    /// 配送地址的邮政编码
    /// </summary>
    [Required]
    public string? ZipCode { get; set; }

    /// <summary>
    /// 支付卡号
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    /// 持卡人姓名
    /// </summary>
    public string? CardHolderName { get; set; }

    /// <summary>
    /// 卡片安全码（CVV）
    /// </summary>
    public string? CardSecurityNumber { get; set; }

    /// <summary>
    /// 卡片过期日期
    /// </summary>
    public DateTime? CardExpiration { get; set; }

    /// <summary>
    /// 卡片类型ID（如Visa、Mastercard等）
    /// </summary>
    public int CardTypeId { get; set; }

    /// <summary>
    /// 购买者信息
    /// </summary>
    public string? Buyer { get; set; }

    /// <summary>
    /// 结账请求的唯一标识符
    /// </summary>
    public Guid RequestId { get; set; }
}
