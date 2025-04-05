using eShop.Basket.API.Grpc;
using eShop.WebApp;
using eShop.WebApp.Services.OrderStatus.IntegrationEvents;
using eShop.WebAppComponents.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenAI;

/// <summary>
/// 提供应用程序服务配置的扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加应用程序所需的所有服务到服务容器
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // 添加身份验证服务
        builder.AddAuthenticationServices();

        // 添加RabbitMQ事件总线和相关订阅
        builder.AddRabbitMqEventBus("EventBus")
               .AddEventBusSubscriptions();

        // 添加HTTP转发器和服务发现
        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // 添加应用服务
        builder.Services.AddScoped<BasketState>();         // 购物篮状态，作用域为每次请求
        builder.Services.AddScoped<LogOutService>();       // 登出服务
        builder.Services.AddSingleton<BasketService>();    // 购物篮服务，单例模式
        builder.Services.AddSingleton<OrderStatusNotificationService>(); // 订单状态通知服务
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>(); // 商品图片URL提供器
        builder.AddAIServices();                           // 添加AI服务

        // 注册HTTP和GRPC客户端
        builder.Services.AddGrpcClient<Basket.BasketClient>(o => o.Address = new("http://basket-api"))
            .AddAuthToken();                               // 添加购物篮gRPC客户端

        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://catalog-api"))
            .AddApiVersion(2.0)                            // 指定API版本
            .AddAuthToken();                               // 添加商品目录HTTP客户端

        builder.Services.AddHttpClient<OrderingService>(o => o.BaseAddress = new("http://ordering-api"))
            .AddApiVersion(1.0)                            // 指定API版本
            .AddAuthToken();                               // 添加订单HTTP客户端
    }

    /// <summary>
    /// 为事件总线添加订阅，用于处理订单状态变更事件
    /// </summary>
    /// <param name="eventBus">事件总线构建器</param>
    public static void AddEventBusSubscriptions(this IEventBusBuilder eventBus)
    {
        // 订阅各种订单状态变更事件，并指定对应的处理程序
        eventBus.AddSubscription<OrderStatusChangedToAwaitingValidationIntegrationEvent, OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToPaidIntegrationEvent, OrderStatusChangedToPaidIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToShippedIntegrationEvent, OrderStatusChangedToShippedIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToCancelledIntegrationEvent, OrderStatusChangedToCancelledIntegrationEventHandler>();
        eventBus.AddSubscription<OrderStatusChangedToSubmittedIntegrationEvent, OrderStatusChangedToSubmittedIntegrationEventHandler>();
    }

    /// <summary>
    /// 配置身份验证服务，包括Cookie认证和OpenID Connect
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    public static void AddAuthenticationServices(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        // 确保sub声明不会被映射到其他声明类型
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        // 从配置获取必要的身份验证URL和设置
        var identityUrl = configuration.GetRequiredValue("IdentityUrl");
        var callBackUrl = configuration.GetRequiredValue("CallBackUrl");
        var sessionCookieLifetime = configuration.GetValue("SessionCookieLifetimeMinutes", 60);

        // 添加身份验证服务
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options => options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionCookieLifetime))
        .AddOpenIdConnect(options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = identityUrl;
            options.SignedOutRedirectUri = callBackUrl;
            options.ClientId = "webapp";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = false;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("orders");
            options.Scope.Add("basket");
        });

        // 添加Blazor身份验证服务
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddCascadingAuthenticationState();
    }

    /// <summary>
    /// 添加AI服务，支持Ollama或OpenAI
    /// </summary>
    /// <param name="builder">主机应用程序构建器</param>
    private static void AddAIServices(this IHostApplicationBuilder builder)
    {
        // 如果启用了Ollama，则使用Ollama API客户端
        if (builder.Configuration["OllamaEnabled"] is string ollamaEnabled && bool.Parse(ollamaEnabled))
        {
            builder.AddOllamaApiClient("chat")
                .AddChatClient()
                .UseFunctionInvocation();
        }
        else
        {
            // 否则尝试使用OpenAI
            var chatModel = builder.Configuration.GetSection("AI").Get<AIOptions>()?.OpenAI?.ChatModel;
            if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("openai")) && !string.IsNullOrWhiteSpace(chatModel))
            {
                builder.AddOpenAIClientFromConfiguration("openai");
                builder.Services.AddChatClient(sp => sp.GetRequiredService<OpenAIClient>().AsChatClient(chatModel ?? "gpt-4o-mini"))
                    .UseFunctionInvocation()
                    .UseOpenTelemetry(configure: t => t.EnableSensitiveData = true)
                    .UseLogging();
            }
        }
    }

    /// <summary>
    /// 获取当前用户的买家ID
    /// </summary>
    /// <param name="authenticationStateProvider">身份验证状态提供器</param>
    /// <returns>买家ID或null</returns>
    public static async Task<string?> GetBuyerIdAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("sub")?.Value; // 从sub声明中获取用户ID
    }

    /// <summary>
    /// 获取当前用户的用户名
    /// </summary>
    /// <param name="authenticationStateProvider">身份验证状态提供器</param>
    /// <returns>用户名或null</returns>
    public static async Task<string?> GetUserNameAsync(this AuthenticationStateProvider authenticationStateProvider)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.FindFirst("name")?.Value; // 从name声明中获取用户名
    }
}
