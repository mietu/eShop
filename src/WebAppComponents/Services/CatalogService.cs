using System.Net.Http.Json;
using System.Web;
using eShop.WebAppComponents.Catalog;

namespace eShop.WebAppComponents.Services;

/// <summary>
/// 目录服务类 - 负责处理与商品目录相关的所有API请求
/// </summary>
public class CatalogService(HttpClient httpClient) : ICatalogService
{
    private readonly string remoteServiceBaseUrl = "api/catalog/";

    /// <summary>
    /// 根据ID获取特定商品的详细信息
    /// </summary>
    /// <param name="id">商品的唯一标识符</param>
    /// <returns>商品详细信息，如果不存在则返回null</returns>
    public Task<CatalogItem?> GetCatalogItem(int id)
    {
        var uri = $"{remoteServiceBaseUrl}items/{id}";
        return httpClient.GetFromJsonAsync<CatalogItem>(uri);
    }

    /// <summary>
    /// 获取分页的商品列表，支持按品牌和类型筛选
    /// </summary>
    /// <param name="pageIndex">页码，从0开始</param>
    /// <param name="pageSize">每页显示的商品数量</param>
    /// <param name="brand">品牌ID筛选条件，可选</param>
    /// <param name="type">类型ID筛选条件，可选</param>
    /// <returns>包含分页商品列表的结果对象</returns>
    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type)
    {
        var uri = GetAllCatalogItemsUri(remoteServiceBaseUrl, pageIndex, pageSize, brand, type);
        var result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);
        return result!;
    }

    /// <summary>
    /// 根据多个商品ID获取商品列表
    /// </summary>
    /// <param name="ids">要查询的商品ID集合</param>
    /// <returns>匹配ID的商品列表</returns>
    public async Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids)
    {
        var uri = $"{remoteServiceBaseUrl}items/by?ids={string.Join("&ids=", ids)}";
        var result = await httpClient.GetFromJsonAsync<List<CatalogItem>>(uri);
        return result!;
    }

    /// <summary>
    /// 根据文本进行语义相关性搜索，返回匹配的商品
    /// </summary>
    /// <param name="page">页码，从0开始</param>
    /// <param name="take">每页返回的商品数量</param>
    /// <param name="text">搜索文本</param>
    /// <returns>包含语义相关商品的结果对象</returns>
    public Task<CatalogResult> GetCatalogItemsWithSemanticRelevance(int page, int take, string text)
    {
        var url = $"{remoteServiceBaseUrl}items/withsemanticrelevance?text={HttpUtility.UrlEncode(text)}&pageIndex={page}&pageSize={take}";
        var result = httpClient.GetFromJsonAsync<CatalogResult>(url);
        return result!;
    }

    /// <summary>
    /// 获取所有可用的商品品牌列表
    /// </summary>
    /// <returns>品牌列表</returns>
    public async Task<IEnumerable<CatalogBrand>> GetBrands()
    {
        var uri = $"{remoteServiceBaseUrl}catalogBrands";
        var result = await httpClient.GetFromJsonAsync<CatalogBrand[]>(uri);
        return result!;
    }

    /// <summary>
    /// 获取所有可用的商品类型列表
    /// </summary>
    /// <returns>类型列表</returns>
    public async Task<IEnumerable<CatalogItemType>> GetTypes()
    {
        var uri = $"{remoteServiceBaseUrl}catalogTypes";
        var result = await httpClient.GetFromJsonAsync<CatalogItemType[]>(uri);
        return result!;
    }

    /// <summary>
    /// 构建获取商品列表的URI，支持分页和筛选
    /// </summary>
    /// <param name="baseUri">基础URI</param>
    /// <param name="pageIndex">页码，从0开始</param>
    /// <param name="pageSize">每页商品数量</param>
    /// <param name="brand">品牌ID筛选条件，可选</param>
    /// <param name="type">类型ID筛选条件，可选</param>
    /// <returns>完整的请求URI</returns>
    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type)
    {
        string filterQs = string.Empty;

        if (type.HasValue)
        {
            filterQs += $"type={type.Value}&";
        }
        if (brand.HasValue)
        {
            filterQs += $"brand={brand.Value}&";
        }

        return $"{baseUri}items?{filterQs}pageIndex={pageIndex}&pageSize={pageSize}";
    }
}
