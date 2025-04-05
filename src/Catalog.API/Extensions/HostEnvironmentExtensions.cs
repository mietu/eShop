using System.Reflection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// 提供IHostEnvironment接口的扩展方法
/// </summary>
internal static class HostEnvironmentExtensions
{
    /// <summary>
    /// 检查当前环境是否为构建环境
    /// </summary>
    /// <param name="hostEnvironment">IHostEnvironment实例</param>
    /// <returns>
    /// 如果当前环境为"Build"或者正在使用"GetDocument.Insider"工具运行，则返回true；
    /// 否则返回false
    /// </returns>
    /// <remarks>
    /// 此方法用于识别两种构建时场景：
    /// 1. 环境显式设置为"Build"
    /// 2. 应用程序正在通过OpenAPI文档生成工具"GetDocument.Insider"运行
    /// 
    /// 在这些情况下，应用程序可能需要特殊的配置或忽略某些运行时组件的初始化
    /// </remarks>
    public static bool IsBuild(this IHostEnvironment hostEnvironment)
    {
        // 检查环境是否为"Build"或入口程序集是否为"GetDocument.Insider"
        // 用于处理通过OpenAPI构建时生成工具(GetDocument.Insider)启动应用程序的场景
        return hostEnvironment.IsEnvironment("Build")
            || Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
