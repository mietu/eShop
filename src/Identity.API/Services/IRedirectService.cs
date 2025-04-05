namespace eShop.Identity.API.Services
{
    /// <summary>
    /// 提供重定向服务的接口，用于处理URL重定向相关操作
    /// </summary>
    public interface IRedirectService
    {
        /// <summary>
        /// 从返回URL中提取重定向URI
        /// </summary>
        /// <param name="url">包含重定向信息的URL</param>
        /// <returns>提取出的重定向URI</returns>
        string ExtractRedirectUriFromReturnUrl(string url);
    }
}
