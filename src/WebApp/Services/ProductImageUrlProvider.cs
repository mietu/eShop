using eShop.WebAppComponents.Services;

namespace eShop.WebApp.Services;

/// <summary>
/// 提供产品图片 URL 的服务实现
/// </summary>
public class ProductImageUrlProvider : IProductImageUrlProvider
{
    /// <summary>
    /// 根据产品 ID 获取产品图片 URL
    /// </summary>
    /// <param name="productId">产品 ID</param>
    /// <returns>产品图片 URL</returns>
    public string GetProductImageUrl(int productId)
        => $"product-images/{productId}?api-version=2.0";
}
