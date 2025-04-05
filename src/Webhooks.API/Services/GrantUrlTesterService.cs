namespace Webhooks.API.Services;

/// <summary>
/// 服务实现，用于测试webhook授权URL是否有效并正确响应
/// </summary>
/// <remarks>
/// 该服务验证webhook URL和授权URL是否来自相同源，
/// 并通过发送OPTIONS请求测试授权URL是否正确响应指定的token
/// </remarks>
class GrantUrlTesterService(IHttpClientFactory factory, ILogger<IGrantUrlTesterService> logger) : IGrantUrlTesterService
{
    /// <summary>
    /// 测试授权URL是否有效并正确响应
    /// </summary>
    /// <param name="urlHook">webhook的URL</param>
    /// <param name="url">需要测试的授权URL</param>
    /// <param name="token">验证token</param>
    /// <returns>如果URL有效且正确响应token，则返回true；否则返回false</returns>
    public async Task<bool> TestGrantUrl(string urlHook, string url, string token)
    {
        // 验证webhook URL和授权URL是否来自相同源
        if (!CheckSameOrigin(urlHook, url))
        {
            logger.LogWarning("hook的URL（{UrlHook} 和授权URL（{Url} 不属于同一来源）", urlHook, url);
            return false;
        }

        // 创建HTTP客户端并准备OPTIONS请求
        var client = factory.CreateClient();
        var msg = new HttpRequestMessage(HttpMethod.Options, url);
        msg.Headers.Add("X-eshop-whtoken", token);

        logger.LogInformation("将带有令牌 \"{Token}\" 的 OPTIONS 消息发送到 {Url}", token ?? string.Empty, url);

        try
        {
            // 发送请求并处理响应
            var response = await client.SendAsync(msg);
            var tokenReceived = response.Headers.TryGetValues("X-eshop-whtoken", out var tokenValues) ? tokenValues.FirstOrDefault() : null;
            var tokenExpected = string.IsNullOrWhiteSpace(token) ? null : token;

            logger.LogInformation("URL {Url} 的响应码为 {StatusCode} 以及头部的令牌为 {TokenReceived} (预期令牌为 {TokenExpected})", url, response.StatusCode, tokenReceived, tokenExpected);

            // 验证响应是否成功，且返回的token是否与预期一致
            return response.IsSuccessStatusCode && tokenReceived == tokenExpected;
        }
        catch (Exception ex)
        {
            // 捕获并记录任何请求异常
            logger.LogWarning("发送 OPTIONS 请求时出现异常 {TypeName}。无法授予 URL。", ex.GetType().Name);

            return false;
        }
    }

    /// <summary>
    /// 检查两个URL是否来自相同源（相同的协议、主机和端口）
    /// </summary>
    /// <param name="urlHook">webhook的URL</param>
    /// <param name="url">需要检查的URL</param>
    /// <returns>如果两个URL来自相同源，则返回true；否则返回false</returns>
    private static bool CheckSameOrigin(string urlHook, string url)
    {
        var firstUrl = new Uri(urlHook, UriKind.Absolute);
        var secondUrl = new Uri(url, UriKind.Absolute);

        return firstUrl.Scheme == secondUrl.Scheme &&
            firstUrl.Port == secondUrl.Port &&
            firstUrl.Host == secondUrl.Host;
    }
}
