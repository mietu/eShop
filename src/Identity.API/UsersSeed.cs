
namespace eShop.Identity.API;

/// <summary>
/// 用户数据种子类，用于初始化身份认证系统的默认用户
/// </summary>
public class UsersSeed(ILogger<UsersSeed> logger, UserManager<ApplicationUser> userManager) : IDbSeeder<ApplicationDbContext>
{
    /// <summary>
    /// 执行数据库种子数据初始化，创建默认用户
    /// </summary>
    /// <param name="context">应用程序数据库上下文</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // 查找是否已存在用户alice
        var alice = await userManager.FindByNameAsync("alice");

        if (alice == null)
        {
            // 如果alice不存在，创建新用户实例
            alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
                CardHolderName = "Alice Smith",
                CardNumber = "XXXXXXXXXXXX1881",
                CardType = 1,
                City = "Redmond",
                Country = "U.S.",
                Expiration = "12/24",
                Id = Guid.NewGuid().ToString(),
                LastName = "Smith",
                Name = "Alice",
                PhoneNumber = "1234567890",
                ZipCode = "98052",
                State = "WA",
                Street = "15703 NE 61st Ct",
                SecurityNumber = "123"
            };

            // 创建用户并设置密码
            var result = userManager.CreateAsync(alice, "Pass123$").Result;

            // 检查创建结果
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            // 记录创建成功的日志
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("alice created");
            }
        }
        else
        {
            // 用户已存在，记录日志
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("alice already exists");
            }
        }

        // 查找是否已存在用户bob
        var bob = await userManager.FindByNameAsync("bob");

        if (bob == null)
        {
            // 如果bob不存在，创建新用户实例
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true,
                CardHolderName = "Bob Smith",
                CardNumber = "XXXXXXXXXXXX1881",
                CardType = 1,
                City = "Redmond",
                Country = "U.S.",
                Expiration = "12/24",
                Id = Guid.NewGuid().ToString(),
                LastName = "Smith",
                Name = "Bob",
                PhoneNumber = "1234567890",
                ZipCode = "98052",
                State = "WA",
                Street = "15703 NE 61st Ct",
                SecurityNumber = "456"
            };

            // 创建用户并设置密码
            var result = await userManager.CreateAsync(bob, "Pass123$");

            // 检查创建结果
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            // 记录创建成功的日志
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("bob created");
            }
        }
        else
        {
            // 用户已存在，记录日志
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("bob already exists");
            }
        }
    }
}
