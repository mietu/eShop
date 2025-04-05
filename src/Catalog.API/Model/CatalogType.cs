using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Model;

/// <summary>
/// 表示电子商城中的产品类别
/// </summary>
public class CatalogType
{
    /// <summary>
    /// 获取或设置类别的唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置类别的名称
    /// </summary>
    /// <remarks>此字段是必填的</remarks>
    [Required]
    public string Type { get; set; }
}
