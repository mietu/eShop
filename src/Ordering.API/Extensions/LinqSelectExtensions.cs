namespace eShop.Ordering.API.Extensions;

/// <summary>
/// 提供用于处理LINQ查询中异常情况的扩展方法
/// </summary>
public static class LinqSelectExtensions
{
    /// <summary>
    /// 对集合中的每个元素执行选择器函数，并捕获过程中发生的任何异常
    /// </summary>
    /// <typeparam name="TSource">源集合中元素的类型</typeparam>
    /// <typeparam name="TResult">结果集合中元素的类型</typeparam>
    /// <param name="enumerable">要处理的源集合</param>
    /// <param name="selector">应用于每个元素的转换函数</param>
    /// <returns>包含转换结果和任何捕获异常的结果集合</returns>
    public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
    {
        foreach (TSource element in enumerable)
        {
            SelectTryResult<TSource, TResult> returnedValue;
            try
            {
                returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
            }
            catch (Exception ex)
            {
                returnedValue = new SelectTryResult<TSource, TResult>(element, default, ex);
            }
            yield return returnedValue;
        }
    }

    /// <summary>
    /// 处理SelectTry方法捕获的异常，允许通过提供的处理函数转换异常为结果
    /// </summary>
    /// <typeparam name="TSource">源集合中元素的类型</typeparam>
    /// <typeparam name="TResult">结果集合中元素的类型</typeparam>
    /// <param name="enumerable">由SelectTry生成的结果集合</param>
    /// <param name="exceptionHandler">将异常转换为结果的函数</param>
    /// <returns>转换后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
    {
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.CaughtException));
    }

    /// <summary>
    /// 处理SelectTry方法捕获的异常，允许通过提供的处理函数将源元素和异常转换为结果
    /// </summary>
    /// <typeparam name="TSource">源集合中元素的类型</typeparam>
    /// <typeparam name="TResult">结果集合中元素的类型</typeparam>
    /// <param name="enumerable">由SelectTry生成的结果集合</param>
    /// <param name="exceptionHandler">将源元素和异常转换为结果的函数</param>
    /// <returns>转换后的结果集合</returns>
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<TSource, Exception, TResult> exceptionHandler)
    {
        return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
    }

    /// <summary>
    /// 包含尝试转换源元素时的结果和可能捕获的异常
    /// </summary>
    /// <typeparam name="TSource">源元素的类型</typeparam>
    /// <typeparam name="TResult">目标结果的类型</typeparam>
    public class SelectTryResult<TSource, TResult>
    {
        /// <summary>
        /// 初始化SelectTryResult的新实例
        /// </summary>
        /// <param name="source">源元素</param>
        /// <param name="result">转换结果</param>
        /// <param name="exception">转换过程中捕获的异常，如无异常则为null</param>
        internal SelectTryResult(TSource source, TResult result, Exception exception)
        {
            Source = source;
            Result = result;
            CaughtException = exception;
        }

        /// <summary>
        /// 获取源元素
        /// </summary>
        public TSource Source { get; private set; }

        /// <summary>
        /// 获取转换结果
        /// </summary>
        public TResult Result { get; private set; }

        /// <summary>
        /// 获取转换过程中捕获的异常，如无异常则为null
        /// </summary>
        public Exception CaughtException { get; private set; }
    }
}
