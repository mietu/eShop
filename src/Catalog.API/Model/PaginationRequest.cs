using System.ComponentModel;

namespace eShop.Catalog.API.Model;

/// <summary>
/// 表示分页请求的参数，用于控制数据分页获取
/// </summary>
/// <param name="PageSize">单页返回的项目数量，默认为10</param>
/// <param name="PageIndex">要返回的页码索引，从0开始计数，默认为0</param>
public record PaginationRequest(
    [property: Description("在单个结果页中返回的项目数")]
    [property: DefaultValue(10)]
    int PageSize = 10,

    [property: Description("要返回的结果页的索引")]
    [property: DefaultValue(0)]
    int PageIndex = 0
);
