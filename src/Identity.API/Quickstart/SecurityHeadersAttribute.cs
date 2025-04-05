// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 安全头属性过滤器，用于向HTTP响应添加各种安全相关的头信息
/// 这些头信息有助于防止常见的网络安全攻击，如XSS、点击劫持等
/// </summary>
public class SecurityHeadersAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 在结果执行前处理响应头
    /// </summary>
    /// <param name="context">当前HTTP请求的上下文</param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var result = context.Result;
        if (result is ViewResult)
        {
            // X-Content-Type-Options: 防止MIME类型嗅探攻击
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            }

            // X-Frame-Options: 防止点击劫持攻击，控制页面能否在frame中显示
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.HttpContext.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
            }

            // Content-Security-Policy: 内容安全策略，限制页面可以加载的资源
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
            var csp = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";
            // 当生产环境启用HTTPS后，可考虑添加upgrade-insecure-requests指令
            //csp += "upgrade-insecure-requests;";
            // 如需允许加载第三方图片，可添加相应的img-src指令
            // csp += "img-src 'self' https://pbs.twimg.com;";

            // 为标准兼容的现代浏览器添加CSP头
            if (!context.HttpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.HttpContext.Response.Headers.Append("Content-Security-Policy", csp);
            }
            // 为旧版IE浏览器添加X-Content-Security-Policy头
            if (!context.HttpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
            {
                context.HttpContext.Response.Headers.Append("X-Content-Security-Policy", csp);
            }

            // Referrer-Policy: 控制HTTP请求中Referer头的信息
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
            var referrer_policy = "no-referrer";
            if (!context.HttpContext.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.HttpContext.Response.Headers.Append("Referrer-Policy", referrer_policy);
            }
        }
    }
}
