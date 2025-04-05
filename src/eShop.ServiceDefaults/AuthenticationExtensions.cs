using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace eShop.ServiceDefaults;

/// <summary>
/// 提供身份验证相关扩展方法的静态类
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// 为应用程序添加默认的身份验证和授权配置
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    /// <returns>服务集合，以支持方法链式调用</returns>
    /// <remarks>
    /// 此方法从配置中读取Identity部分来设置JWT Bearer身份验证。
    /// 如果Identity部分不存在，则不会配置身份验证。
    /// 配置示例:
    /// {
    ///   "Identity": {
    ///     "Url": "http://identity",
    ///     "Audience": "basket"
    ///    }
    /// }
    /// </remarks>
    public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // 获取Identity配置部分
        var identitySection = configuration.GetSection("Identity");

        if (!identitySection.Exists())
        {
            // 配置中没有Identity部分，不配置身份验证
            return services;
        }

        // 防止将"sub"声明映射到nameidentifier，保持原始sub声明
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication().AddJwtBearer(options =>
        {
            // 从配置中获取身份验证服务URL和受众
            var identityUrl = identitySection.GetRequiredValue("Url");
            var audience = identitySection.GetRequiredValue("Audience");

            // 配置JWT Bearer选项
            options.Authority = identityUrl;      // 身份验证服务器地址
            options.RequireHttpsMetadata = false; // 不要求HTTPS元数据
            options.Audience = audience;          // 设置目标受众

#if DEBUG
            // 调试模式下，添加Android模拟器本地测试支持
            // 参考：https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-8.0#android
            options.TokenValidationParameters.ValidIssuers = [identityUrl, "https://10.0.2.2:5243"];
#else
            // 生产环境仅接受配置的身份服务URL作为有效颁发者
            options.TokenValidationParameters.ValidIssuers = [identityUrl];
#endif

            // 禁用受众验证，因为已在上面设置了Audience属性
            options.TokenValidationParameters.ValidateAudience = false;
        });

        // 添加授权服务
        services.AddAuthorization();

        return services;
    }
}
