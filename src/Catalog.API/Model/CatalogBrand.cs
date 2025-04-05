using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Model;

/// <summary>
/// 表示电子商城目录中的品牌
/// </summary>
public class CatalogBrand
{
    /// <summary>
    /// 获取或设置品牌的唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置品牌名称
    /// </summary>
    [Required]
    public string Brand { get; set; }
}
