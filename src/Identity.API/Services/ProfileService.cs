namespace eShop.Identity.API.Services
{
    /// <summary>
    /// 用户配置文件服务，实现IProfileService接口，
    /// 用于向IdentityServer提供用户信息和激活状态
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// 构造函数，注入UserManager服务
        /// </summary>
        /// <param name="userManager">用户管理器</param>
        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// 获取用户配置数据，将用户声明添加到令牌中
        /// </summary>
        /// <param name="context">配置数据请求上下文</param>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // 确保主题存在，否则抛出异常
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            // 从主题声明中获取用户ID
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;

            // 根据ID查找用户
            var user = await _userManager.FindByIdAsync(subjectId);
            if (user == null)
                throw new ArgumentException("Invalid subject identifier");

            // 获取用户声明并设置到上下文中
            var claims = GetClaimsFromUser(user);
            context.IssuedClaims = claims.ToList();
        }

        /// <summary>
        /// 检查用户是否处于活动状态
        /// </summary>
        /// <param name="context">活动状态检查上下文</param>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            // 确保主题存在，否则抛出异常
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            // 从主题声明中获取用户ID
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;
            var user = await _userManager.FindByIdAsync(subjectId);

            // 默认设置为非活动状态
            context.IsActive = false;

            if (user != null)
            {
                // 如果支持安全戳，则验证安全戳是否匹配
                if (_userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if (security_stamp != null)
                    {
                        var db_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        if (db_security_stamp != security_stamp)
                            return; // 安全戳不匹配，维持非活动状态
                    }
                }

                // 用户活动状态取决于锁定状态
                context.IsActive =
                    !user.LockoutEnabled || // 未启用锁定
                    !user.LockoutEnd.HasValue || // 未设置锁定结束时间
                    user.LockoutEnd <= DateTime.UtcNow; // 锁定已过期
            }
        }

        /// <summary>
        /// 从用户信息中获取声明列表
        /// </summary>
        /// <param name="user">应用程序用户</param>
        /// <returns>用户声明集合</returns>
        private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
        {
            // 基础声明：用户ID和用户名
            var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Subject, user.Id),
                    new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                };

            // 添加个人信息声明
            if (!string.IsNullOrWhiteSpace(user.Name))
                claims.Add(new Claim("name", user.Name));

            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim("last_name", user.LastName));

            // 添加支付信息声明
            if (!string.IsNullOrWhiteSpace(user.CardNumber))
                claims.Add(new Claim("card_number", user.CardNumber));

            if (!string.IsNullOrWhiteSpace(user.CardHolderName))
                claims.Add(new Claim("card_holder", user.CardHolderName));

            if (!string.IsNullOrWhiteSpace(user.SecurityNumber))
                claims.Add(new Claim("card_security_number", user.SecurityNumber));

            if (!string.IsNullOrWhiteSpace(user.Expiration))
                claims.Add(new Claim("card_expiration", user.Expiration));

            // 添加地址信息声明
            if (!string.IsNullOrWhiteSpace(user.City))
                claims.Add(new Claim("address_city", user.City));

            if (!string.IsNullOrWhiteSpace(user.Country))
                claims.Add(new Claim("address_country", user.Country));

            if (!string.IsNullOrWhiteSpace(user.State))
                claims.Add(new Claim("address_state", user.State));

            if (!string.IsNullOrWhiteSpace(user.Street))
                claims.Add(new Claim("address_street", user.Street));

            if (!string.IsNullOrWhiteSpace(user.ZipCode))
                claims.Add(new Claim("address_zip_code", user.ZipCode));

            // 添加邮箱相关声明
            if (_userManager.SupportsUserEmail)
            {
                claims.AddRange(new[]
                {
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                    });
            }

            // 添加电话号码相关声明
            if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.AddRange(new[]
                {
                        new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                    });
            }

            return claims;
        }
    }
}
