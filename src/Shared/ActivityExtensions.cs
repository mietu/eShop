using System.Diagnostics;

/// <summary>
/// 提供 System.Diagnostics.Activity 类的扩展方法
/// </summary>
internal static class ActivityExtensions
{
    /// <summary>
    /// 为活动添加标准化的异常标签
    /// </summary>
    /// <param name="activity">要添加标签的活动实例</param>
    /// <param name="ex">要记录的异常</param>
    /// <remarks>
    /// 实现遵循 OpenTelemetry 异常语义约定
    /// 参考: https://opentelemetry.io/docs/specs/otel/trace/semantic_conventions/exceptions/
    /// </remarks>
    public static void SetExceptionTags(this Activity activity, Exception ex)
    {
        // 如果活动为空，则不执行任何操作
        if (activity is null)
        {
            return;
        }

        // 添加异常消息标签
        activity.AddTag("exception.message", ex.Message);

        // 添加异常堆栈跟踪标签，使用 ToString() 获取完整的异常信息
        activity.AddTag("exception.stacktrace", ex.ToString());

        // 添加异常类型标签，使用完全限定类型名
        activity.AddTag("exception.type", ex.GetType().FullName);

        // 将活动状态设置为错误
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
