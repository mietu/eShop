using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eShop.ServiceDefaults;

/// <summary>
/// 提供HTTP客户端的扩展方法，用于在请求中添加身份验证令牌
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// 为HTTP客户端添加身份验证令牌支持
    /// </summary>
    /// <param name="builder">HTTP客户端构建器</param>
    /// <returns>配置后的HTTP客户端构建器</returns>
    public static IHttpClientBuilder AddAuthToken(this IHttpClientBuilder builder)
    {
        // 添加HTTP上下文访问器，用于获取当前请求的上下文
        builder.Services.AddHttpContextAccessor();

        // 尝试添加授权处理程序作为瞬态服务，如果已存在则不添加
        builder.Services.TryAddTransient<HttpClientAuthorizationDelegatingHandler>();

        // 将授权处理程序添加到HTTP消息处理管道中
        builder.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

        return builder;
    }

    /// <summary>
    /// 处理HTTP请求授权的委托处理程序
    /// 在请求发送前自动添加Bearer令牌
    /// </summary>
    private class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 初始化HttpClientAuthorizationDelegatingHandler的新实例
        /// </summary>
        /// <param name="httpContextAccessor">HTTP上下文访问器</param>
        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 使用指定的内部处理程序初始化HttpClientAuthorizationDelegatingHandler的新实例
        /// </summary>
        /// <param name="httpContextAccessor">HTTP上下文访问器</param>
        /// <param name="innerHandler">内部HTTP消息处理程序</param>
        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 拦截HTTP请求并添加授权头
        /// </summary>
        /// <param name="request">HTTP请求消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP响应消息</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 检查是否存在HTTP上下文
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                // 尝试从当前上下文获取访问令牌
                var accessToken = await context.GetTokenAsync("access_token");

                // 如果令牌存在，将其添加到请求的授权头中
                if (accessToken is not null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            // 调用基类方法继续处理请求
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
