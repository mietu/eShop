namespace eShop.OrderProcessor;

/// <summary>
/// 配置后台任务的选项
/// </summary>
public class BackgroundTaskOptions
{
    /// <summary>
    /// 获取或设置宽限期时间（以秒为单位）
    /// </summary>
    public int GracePeriodTime { get; set; }

    /// <summary>
    /// 获取或设置检查更新的时间间隔（以秒为单位）
    /// </summary>
    public int CheckUpdateTime { get; set; }
}
