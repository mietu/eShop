namespace eShop.Identity.API.Services
{
    /// <summary>
    /// Entity Framework实现的登录服务，提供用户认证和登录功能
    /// </summary>
    public class EFLoginService : ILoginService<ApplicationUser>
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// 构造函数，初始化用户管理器和登录管理器
        /// </summary>
        /// <param name="userManager">用户管理器，用于用户相关操作</param>
        /// <param name="signInManager">登录管理器，用于处理用户登录</param>
        public EFLoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// 根据用户名(邮箱)查找用户
        /// </summary>
        /// <param name="user">用户的邮箱地址</param>
        /// <returns>找到的用户对象，如果未找到则返回null</returns>
        public async Task<ApplicationUser> FindByUsername(string user)
        {
            return await _userManager.FindByEmailAsync(user);
        }

        /// <summary>
        /// 验证用户凭据是否有效
        /// </summary>
        /// <param name="user">需要验证的用户对象</param>
        /// <param name="password">用户输入的密码</param>
        /// <returns>如果凭据有效返回true，否则返回false</returns>
        public async Task<bool> ValidateCredentials(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// 使用默认设置登录用户
        /// </summary>
        /// <param name="user">要登录的用户</param>
        /// <returns>登录操作的任务</returns>
        public Task SignIn(ApplicationUser user)
        {
            return _signInManager.SignInAsync(user, true);
        }

        /// <summary>
        /// 使用自定义属性登录用户
        /// </summary>
        /// <param name="user">要登录的用户</param>
        /// <param name="properties">认证属性</param>
        /// <param name="authenticationMethod">认证方法，如果为null则使用默认方法</param>
        /// <returns>登录操作的任务</returns>
        public Task SignInAsync(ApplicationUser user, AuthenticationProperties properties, string authenticationMethod = null)
        {
            return _signInManager.SignInAsync(user, properties, authenticationMethod);
        }
    }
}
