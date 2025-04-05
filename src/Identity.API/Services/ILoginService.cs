namespace eShop.Identity.API.Services
{
    /// <summary>
    /// 提供用户登录相关功能的服务接口
    /// </summary>
    /// <typeparam name="T">用户类型</typeparam>
    public interface ILoginService<T>
    {
        /// <summary>
        /// 验证用户凭证
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="password">用户密码</param>
        /// <returns>如果凭证有效则返回true，否则返回false</returns>
        Task<bool> ValidateCredentials(T user, string password);

        /// <summary>
        /// 根据用户名查找用户
        /// </summary>
        /// <param name="user">用户名</param>
        /// <returns>找到的用户对象，若未找到则可能返回null</returns>
        Task<T> FindByUsername(string user);

        /// <summary>
        /// 为用户创建登录会话
        /// </summary>
        /// <param name="user">要登录的用户对象</param>
        Task SignIn(T user);

        /// <summary>
        /// 使用指定参数为用户创建登录会话
        /// </summary>
        /// <param name="user">要登录的用户对象</param>
        /// <param name="properties">身份验证属性</param>
        /// <param name="authenticationMethod">身份验证方法，默认为null</param>
        Task SignInAsync(T user, AuthenticationProperties properties, string authenticationMethod = null);
    }
}
