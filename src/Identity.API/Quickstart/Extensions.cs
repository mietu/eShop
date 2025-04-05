namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 提供身份服务器相关的扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 检查重定向URI是否为本地客户端(非Web客户端)。
    /// 本地客户端通常是桌面或移动应用程序，其重定向URI不以http或https开头。
    /// </summary>
    /// <param name="context">授权请求上下文</param>
    /// <returns>如果重定向URI是本地客户端则返回true，否则返回false</returns>
    public static bool IsNativeClient(this AuthorizationRequest context)
    {
        return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
           && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
    }

    /// <summary>
    /// 创建一个加载页面，用于在重定向到目标URI之前显示。
    /// 此方法通常用于中间页面，例如在授权流程中显示加载状态或处理状态。
    /// </summary>
    /// <param name="controller">当前控制器实例</param>
    /// <param name="viewName">要渲染的视图名称</param>
    /// <param name="redirectUri">用户最终将被重定向到的URI</param>
    /// <returns>表示加载页面的操作结果</returns>
    public static IActionResult LoadingPage(this Controller controller, string viewName, string redirectUri)
    {
        // 设置HTTP状态码为200(OK)
        controller.HttpContext.Response.StatusCode = 200;
        // 清除Location响应头，确保浏览器不会立即跳转
        controller.HttpContext.Response.Headers["Location"] = "";

        // 返回视图，并传递包含重定向URL的模型
        return controller.View(viewName, new RedirectViewModel { RedirectUrl = redirectUri });
    }
}
