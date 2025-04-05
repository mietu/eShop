using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Pgvector;

namespace eShop.Catalog.API.Model;

/// <summary>
/// 目录商品类 - 表示电子商城中的一个商品项
/// </summary>
public class CatalogItem
{
    /// <summary>
    /// 商品唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 商品名称 - 必填项
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 商品价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 商品图片文件名
    /// </summary>
    public string PictureFileName { get; set; }

    /// <summary>
    /// 商品类型ID - 外键
    /// </summary>
    public int CatalogTypeId { get; set; }

    /// <summary>
    /// 关联的商品类型
    /// </summary>
    public CatalogType CatalogType { get; set; }

    /// <summary>
    /// 商品品牌ID - 外键
    /// </summary>
    public int CatalogBrandId { get; set; }

    /// <summary>
    /// 关联的商品品牌
    /// </summary>
    public CatalogBrand CatalogBrand { get; set; }

    /// <summary>
    /// 当前库存数量
    /// </summary>
    public int AvailableStock { get; set; }

    /// <summary>
    /// 补货阈值 - 当库存低于此值时应当补货
    /// </summary>
    public int RestockThreshold { get; set; }

    /// <summary>
    /// 最大库存阈值 - 仓库能够容纳的此商品的最大数量(受物理/物流限制)
    /// </summary>
    public int MaxStockThreshold { get; set; }

    /// <summary>
    /// 商品描述的可选向量嵌入表示，用于语义搜索
    /// </summary>
    [JsonIgnore]
    public Vector Embedding { get; set; }

    /// <summary>
    /// 标记商品是否处于补货状态
    /// </summary>
    public bool OnReorder { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CatalogItem() { }

    /// <summary>
    /// 从库存中减少指定商品的数量并检查是否达到补货阈值
    /// 
    /// 如果库存充足，返回值应等于请求的数量(quantityDesired)
    /// 如果库存不足，方法将移除所有可用库存并返回实际移除的数量
    /// 客户端需要负责判断返回数量是否等于请求数量
    /// 不允许传入负数
    /// </summary>
    /// <param name="quantityDesired">希望从库存中移除的商品数量</param>
    /// <returns>实际从库存中移除的商品数量</returns>
    /// <exception cref="CatalogDomainException">当库存为空或请求数量不合法时抛出异常</exception>
    public int RemoveStock(int quantityDesired)
    {
        if (AvailableStock == 0)
        {
            throw new CatalogDomainException($"库存为空，商品 {Name} 已售罄");
        }

        if (quantityDesired <= 0)
        {
            throw new CatalogDomainException($"请求的商品数量必须大于零");
        }

        int removed = Math.Min(quantityDesired, this.AvailableStock);

        this.AvailableStock -= removed;

        return removed;
    }

    /// <summary>
    /// 增加指定商品的库存数量
    /// </summary>
    /// <param name="quantity">要添加的商品数量</param>
    /// <returns>实际添加到库存的商品数量</returns>
    public int AddStock(int quantity)
    {
        int original = this.AvailableStock;

        // 客户端尝试添加的库存数量超过了仓库能够容纳的物理限制
        if ((this.AvailableStock + quantity) > this.MaxStockThreshold)
        {
            // 目前，此方法只会添加库存至最大阈值
            // 在应用的扩展版本中，我们可以跟踪剩余单位并在其他地方存储过量库存信息
            this.AvailableStock += (this.MaxStockThreshold - this.AvailableStock);
        }
        else
        {
            this.AvailableStock += quantity;
        }

        this.OnReorder = false;

        return this.AvailableStock - original;
    }
}
