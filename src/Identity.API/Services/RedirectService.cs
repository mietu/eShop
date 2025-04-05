namespace eShop.Identity.API.Services
{
    /// <summary>
    /// 提供用于处理重定向URL的服务
    /// </summary>
    public class RedirectService : IRedirectService
    {
        /// <summary>
        /// 从返回URL中提取重定向URI
        /// </summary>
        /// <param name="url">包含重定向URI的返回URL</param>
        /// <returns>提取出的重定向URI，如果未找到则返回空字符串</returns>
        public string ExtractRedirectUriFromReturnUrl(string url)
        {
            // 解码HTML字符，确保URL中的特殊字符被正确处理
            var decodedUrl = System.Net.WebUtility.HtmlDecode(url);

            // 按"redirect_uri="分割URL以获取重定向URI部分
            var results = Regex.Split(decodedUrl, "redirect_uri=");
            if (results.Length < 2)
                return ""; // 如果没有找到redirect_uri参数，返回空字符串

            string result = results[1]; // 获取redirect_uri=之后的部分

            // 确定分割关键字，根据URL内容选择适当的终止标记
            string splitKey;
            if (result.Contains("signin-oidc"))
                splitKey = "signin-oidc"; // 对于OpenID Connect身份验证回调使用此标记
            else
                splitKey = "scope"; // 否则使用scope参数作为终止标记

            // 根据确定的分割关键字再次分割，获取纯净的重定向URI
            results = Regex.Split(result, splitKey);
            if (results.Length < 2)
                return ""; // 如果没有找到终止标记，返回空字符串

            result = results[0]; // 获取终止标记之前的部分

            // 替换URL编码的特殊字符，恢复URI的原始形式
            // %3A -> :
            // %2F -> /
            // & -> 空字符串（移除查询参数分隔符）
            return result.Replace("%3A", ":").Replace("%2F", "/").Replace("&", "");
        }
    }
}
