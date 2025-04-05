namespace eShop.Identity.API.Configuration
{
    /// <summary>
    /// 身份服务器配置类，定义API资源、作用域、身份资源和客户端
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 定义系统中的API资源 - 代表需要保护的后端服务
        /// ApiResources定义了可以被访问的API端点及其相关信息
        /// </summary>
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
                {
                    new ApiResource("orders", "Orders Service"),     // 订单服务API
                    new ApiResource("basket", "Basket Service"),     // 购物篮服务API
                    new ApiResource("webhooks", "Webhooks registration Service"), // Webhook注册服务API
                };
        }

        /// <summary>
        /// 定义API作用域 - 控制访问API的权限范围
        /// ApiScope用于保护API，效果与IdentityServer 3.x中的API资源相同
        /// 客户端请求的令牌中包含这些作用域，以限定访问权限
        /// </summary>
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
                {
                    new ApiScope("orders", "Orders Service"),       // 订单服务作用域
                    new ApiScope("basket", "Basket Service"),       // 购物篮服务作用域
                    new ApiScope("webhooks", "Webhooks registration Service"), // Webhook注册服务作用域
                };
        }

        /// <summary>
        /// 定义身份资源 - 用户个人信息的数据（如用户ID、姓名、电子邮件地址等）
        /// 这些资源可以通过OpenID Connect协议返回给客户端
        /// 更多信息请参见: http://docs.identityserver.io/en/release/configuration/resources.html
        /// </summary>
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),   // 标准OpenID Connect资源，包含用户的唯一标识符
                    new IdentityResources.Profile()   // 标准OpenID Connect资源，包含基本的用户配置文件信息
                };
        }

        /// <summary>
        /// 定义客户端配置 - 指定哪些应用可以访问身份服务器及其API资源
        /// 每个客户端可以配置不同的授权类型、重定向URI、访问令牌生命周期等
        /// </summary>
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
                {
                    // MAUI移动应用客户端
                    new Client
                    {
                        ClientId = "maui",
                        ClientName = "eShop MAUI OpenId Client",
                        AllowedGrantTypes = GrantTypes.Code,    // 授权码流程，更安全的OAuth流程               
                        // 用于在后端通道检索访问令牌的客户端密钥
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())  // 客户端密钥（已哈希处理）
                        },
                        RedirectUris = { configuration["MauiCallback"] },  // 授权成功后的重定向URI
                        RequireConsent = false,  // 不需要用户同意屏幕
                        RequirePkce = true,      // 使用PKCE增强授权码流程安全性
                        PostLogoutRedirectUris = { $"{configuration["MauiCallback"]}/Account/Redirecting" },  // 注销后重定向URI
                        //AllowedCorsOrigins = { "http://eshopxamarin" },  // 允许的CORS源
                        AllowedScopes = new List<string>  // 客户端允许请求的作用域
                        {
                            IdentityServerConstants.StandardScopes.OpenId,  // OpenID Connect核心
                            IdentityServerConstants.StandardScopes.Profile, // 用户配置文件
                            IdentityServerConstants.StandardScopes.OfflineAccess, // 允许刷新令牌
                            "orders",        // 订单服务API
                            "basket",        // 购物篮服务API
                            "mobileshoppingagg", // 移动购物聚合API
                            "webhooks"       // Webhook服务API
                        },
                        AllowOfflineAccess = true,  // 允许请求刷新令牌以便长期API访问
                        AllowAccessTokensViaBrowser = true,  // 允许通过浏览器传递访问令牌
                        AlwaysIncludeUserClaimsInIdToken = true,  // 始终在ID令牌中包含用户声明
                        AccessTokenLifetime = 60*60*2, // 访问令牌生命周期为2小时
                        IdentityTokenLifetime= 60*60*2 // ID令牌生命周期为2小时
                    },
                    
                    // Web应用客户端
                    new Client
                    {
                        ClientId = "webapp",
                        ClientName = "WebApp Client",
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())  // 客户端密钥
                        },
                        ClientUri = $"{configuration["WebAppClient"]}",  // 客户端公共URI
                        AllowedGrantTypes = GrantTypes.Code,  // 授权码流程
                        AllowAccessTokensViaBrowser = false,  // 不允许通过浏览器传递访问令牌
                        RequireConsent = false,  // 不需要用户同意屏幕
                        AllowOfflineAccess = true,  // 允许刷新令牌
                        AlwaysIncludeUserClaimsInIdToken = true,  // 始终在ID令牌中包含用户声明
                        RequirePkce = false,  // 不要求PKCE（对于Web应用，建议最佳实践应启用）
                        RedirectUris = new List<string>  // 授权成功后的重定向URI
                        {
                            $"{configuration["WebAppClient"]}/signin-oidc"
                        },
                        PostLogoutRedirectUris = new List<string>  // 注销后重定向URI
                        {
                            $"{configuration["WebAppClient"]}/signout-callback-oidc"
                        },
                        AllowedScopes = new List<string>  // 客户端允许请求的作用域
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "orders",
                            "basket",
                            "webshoppingagg",  // Web购物聚合API
                            "webhooks"
                        },
                        AccessTokenLifetime = 60*60*2, // 访问令牌生命周期为2小时
                        IdentityTokenLifetime= 60*60*2 // ID令牌生命周期为2小时
                    },
                    
                    // Webhooks Web客户端
                    new Client
                    {
                        ClientId = "webhooksclient",
                        ClientName = "Webhooks Client",
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())  // 客户端密钥
                        },
                        ClientUri = $"{configuration["WebhooksWebClient"]}",  // 客户端公共URI
                        AllowedGrantTypes = GrantTypes.Code,  // 授权码流程
                        AllowAccessTokensViaBrowser = false,  // 不允许通过浏览器传递访问令牌
                        RequireConsent = false,  // 不需要用户同意屏幕
                        AllowOfflineAccess = true,  // 允许刷新令牌
                        AlwaysIncludeUserClaimsInIdToken = true,  // 始终在ID令牌中包含用户声明
                        RedirectUris = new List<string>  // 授权成功后的重定向URI
                        {
                            $"{configuration["WebhooksWebClient"]}/signin-oidc"
                        },
                        PostLogoutRedirectUris = new List<string>  // 注销后重定向URI
                        {
                            $"{configuration["WebhooksWebClient"]}/signout-callback-oidc"
                        },
                        AllowedScopes = new List<string>  // 客户端允许请求的作用域
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "webhooks"  // 仅需访问Webhook服务
                        },
                        AccessTokenLifetime = 60*60*2, // 访问令牌生命周期为2小时
                        IdentityTokenLifetime= 60*60*2 // ID令牌生命周期为2小时
                    },
                    
                    // 以下是Swagger UI客户端，用于API文档和测试
                    
                    // 购物篮服务Swagger UI客户端
                    new Client
                    {
                        ClientId = "basketswaggerui",
                        ClientName = "Basket Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,  // 隐式授权流程（适用于Swagger UI）
                        AllowAccessTokensViaBrowser = true,  // 允许通过浏览器传递访问令牌

                        RedirectUris = { $"{configuration["BasketApiClient"]}/swagger/oauth2-redirect.html" },  // Swagger OAuth回调
                        PostLogoutRedirectUris = { $"{configuration["BasketApiClient"]}/swagger/" },  // 注销后重定向到Swagger

                        AllowedScopes =  // 只允许访问购物篮服务
                        {
                            "basket"
                        }
                    },
                    
                    // 订单服务Swagger UI客户端
                    new Client
                    {
                        ClientId = "orderingswaggerui",
                        ClientName = "Ordering Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,  // 隐式授权流程
                        AllowAccessTokensViaBrowser = true,  // 允许通过浏览器传递访问令牌

                        RedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/oauth2-redirect.html" },  // Swagger OAuth回调
                        PostLogoutRedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/" },  // 注销后重定向到Swagger

                        AllowedScopes =  // 只允许访问订单服务
                        {
                            "orders"
                        }
                    },
                    
                    // Webhook服务Swagger UI客户端
                    new Client
                    {
                        ClientId = "webhooksswaggerui",
                        ClientName = "WebHooks Service Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,  // 隐式授权流程
                        AllowAccessTokensViaBrowser = true,  // 允许通过浏览器传递访问令牌

                        RedirectUris = { $"{configuration["WebhooksApiClient"]}/swagger/oauth2-redirect.html" },  // Swagger OAuth回调
                        PostLogoutRedirectUris = { $"{configuration["WebhooksApiClient"]}/swagger/" },  // 注销后重定向到Swagger

                        AllowedScopes =  // 只允许访问Webhook服务
                        {
                            "webhooks"
                        }
                    }
                };
        }
    }
}
