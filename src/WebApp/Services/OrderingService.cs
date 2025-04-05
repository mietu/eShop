namespace eShop.WebApp.Services;

/// <summary>
/// 订单服务类，负责处理与订单API的通信
/// 提供获取订单列表和创建新订单的功能
/// </summary>
public class OrderingService(HttpClient httpClient)
{
    /// <summary>
    /// 订单API的基础URL
    /// </summary>
    private readonly string remoteServiceBaseUrl = "/api/Orders/";

    /// <summary>
    /// 获取用户的所有订单
    /// </summary>
    /// <returns>包含订单记录的数组的任务</returns>
    public Task<OrderRecord[]> GetOrders()
    {
        return httpClient.GetFromJsonAsync<OrderRecord[]>(remoteServiceBaseUrl)!;
    }

    /// <summary>
    /// 创建新订单
    /// </summary>
    /// <param name="request">包含创建订单所需信息的请求对象</param>
    /// <param name="requestId">请求的唯一标识符，用于幂等性处理</param>
    /// <returns>表示异步操作的任务</returns>
    public Task CreateOrder(CreateOrderRequest request, Guid requestId)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, remoteServiceBaseUrl);
        requestMessage.Headers.Add("x-requestid", requestId.ToString());
        requestMessage.Content = JsonContent.Create(request);
        return httpClient.SendAsync(requestMessage);
    }
}

/// <summary>
/// 订单记录类，表示订单的摘要信息
/// </summary>
/// <param name="OrderNumber">订单编号</param>
/// <param name="Date">订单创建日期</param>
/// <param name="Status">订单当前状态</param>
/// <param name="Total">订单总金额</param>
public record OrderRecord(
    int OrderNumber,
    DateTime Date,
    string Status,
    decimal Total);
