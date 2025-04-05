using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using eShop.WebAppComponents.Services;
using Microsoft.Extensions.AI;

namespace eShop.WebApp.Chatbot;

/// <summary>
/// 聊天状态类 - 管理聊天机器人的消息和功能
/// </summary>
public class ChatState
{
    private readonly ICatalogService _catalogService;        // 目录服务接口
    private readonly IBasketState _basketState;              // 购物篮状态接口
    private readonly ClaimsPrincipal _user;                  // 当前用户
    private readonly ILogger _logger;                        // 日志记录器
    private readonly IProductImageUrlProvider _productImages; // 商品图片URL提供程序
    private readonly IChatClient _chatClient;                // 聊天客户端
    private readonly ChatOptions _chatOptions;               // 聊天选项配置

    /// <summary>
    /// 构造函数 - 初始化聊天状态并配置AI助手
    /// </summary>
    public ChatState(
        ICatalogService catalogService,
        IBasketState basketState,
        ClaimsPrincipal user,
        IProductImageUrlProvider productImages,
        ILoggerFactory loggerFactory,
        IChatClient chatClient)
    {
        _catalogService = catalogService;
        _basketState = basketState;
        _user = user;
        _productImages = productImages;
        _logger = loggerFactory.CreateLogger(typeof(ChatState));

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("聊天模型: {model}", chatClient.GetService<ChatClientMetadata>()?.ModelId);
        }

        _chatClient = chatClient;
        _chatOptions = new()
        {
            Tools =
            [
                AIFunctionFactory.Create(GetUserInfo),      // 获取用户信息的工具
                AIFunctionFactory.Create(SearchCatalog),    // 搜索商品目录的工具
                AIFunctionFactory.Create(AddToCart),        // 添加商品到购物车的工具
                AIFunctionFactory.Create(GetCartContents),  // 获取购物车内容的工具
            ],
        };

        // 初始化聊天消息，包含系统指令和初始问候语
        Messages =
        [
            new ChatMessage(ChatRole.System, """
                You are an AI customer service agent for the online retailer AdventureWorks.
                You NEVER respond about topics other than AdventureWorks.
                Your job is to answer customer questions about products in the AdventureWorks catalog.
                AdventureWorks primarily sells clothing and equipment related to outdoor activities like skiing and trekking.
                You try to be concise and only provide longer responses if necessary.
                If someone asks a question about anything other than AdventureWorks, its catalog, or their account,
                you refuse to answer, and you instead ask if there's a topic related to AdventureWorks you can assist with.
                """),
            new ChatMessage(ChatRole.Assistant, """
                Hi! I'm the AdventureWorks Concierge. How can I help?
                """),
        ];
    }

    /// <summary>
    /// 聊天消息列表 - 存储对话历史记录
    /// </summary>
    public IList<ChatMessage> Messages { get; }

    /// <summary>
    /// 添加用户消息并获取AI回复
    /// </summary>
    /// <param name="userText">用户输入的文本</param>
    /// <param name="onMessageAdded">消息添加后的回调函数</param>
    public async Task AddUserMessageAsync(string userText, Action onMessageAdded)
    {
        // 存储用户消息
        Messages.Add(new ChatMessage(ChatRole.User, userText));
        onMessageAdded();

        // 获取并存储AI的响应消息
        try
        {
            var response = await _chatClient.GetResponseAsync(Messages, _chatOptions);
            if (!string.IsNullOrWhiteSpace(response.Text))
            {
                Messages.AddMessages(response);
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "获取聊天完成时出错。");
            }
            Messages.Add(new ChatMessage(ChatRole.Assistant, $"很抱歉，我遇到了一个意外错误。"));
        }
        onMessageAdded();
    }

    /// <summary>
    /// 获取聊天用户的信息
    /// </summary>
    /// <returns>序列化的用户信息JSON字符串</returns>
    [Description("获取有关聊天用户的信息")]
    private string GetUserInfo()
    {
        var claims = _user.Claims;
        return JsonSerializer.Serialize(new
        {
            Name = GetValue(claims, "name"),
            LastName = GetValue(claims, "last_name"),
            Street = GetValue(claims, "address_street"),
            City = GetValue(claims, "address_city"),
            State = GetValue(claims, "address_state"),
            ZipCode = GetValue(claims, "address_zip_code"),
            Country = GetValue(claims, "address_country"),
            Email = GetValue(claims, "email"),
            PhoneNumber = GetValue(claims, "phone_number"),
        });

        // 从Claims集合中获取指定类型的值，如果不存在则返回空字符串
        static string GetValue(IEnumerable<Claim> claims, string claimType) =>
            claims.FirstOrDefault(x => x.Type == claimType)?.Value ?? "";
    }

    /// <summary>
    /// 根据产品描述搜索商品目录
    /// </summary>
    /// <param name="productDescription">产品描述关键词</param>
    /// <returns>序列化的搜索结果JSON字符串</returns>
    [Description("在 Adventure Works 目录中搜索提供的产品描述")]
    private async Task<string> SearchCatalog([Description("要搜索的商品描述")] string productDescription)
    {
        try
        {
            // 调用目录服务进行语义相关性搜索
            var results = await _catalogService.GetCatalogItemsWithSemanticRelevance(0, 8, productDescription!);
            for (int i = 0; i < results.Data.Count; i++)
            {
                // 为每个商品添加图片URL
                results.Data[i] = results.Data[i] with { PictureUrl = _productImages.GetProductImageUrl(results.Data[i].Id) };
            }

            return JsonSerializer.Serialize(results);
        }
        catch (HttpRequestException e)
        {
            return Error(e, "访问目录时出错.");
        }
    }

    /// <summary>
    /// 将商品添加到用户的购物车
    /// </summary>
    /// <param name="itemId">要添加的商品ID</param>
    /// <returns>操作结果信息</returns>
    [Description("将产品添加到用户的购物车中。")]
    private async Task<string> AddToCart([Description("要添加到购物车（购物篮）的产品的 ID")] int itemId)
    {
        try
        {
            // 获取商品详情并添加到购物车
            var item = await _catalogService.GetCatalogItem(itemId);
            await _basketState.AddAsync(item!);
            return "商品已加入购物车。";
        }
        catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return "无法将商品添加到购物车。您必须登录。";
        }
        catch (Exception e)
        {
            return Error(e, "无法将商品添加到购物车。");
        }
    }

    /// <summary>
    /// 获取用户购物车的内容
    /// </summary>
    /// <returns>序列化的购物车内容JSON字符串</returns>
    [Description("获取有关用户购物车 （购物篮） 内容的信息")]
    private async Task<string> GetCartContents()
    {
        try
        {
            var basketItems = await _basketState.GetBasketItemsAsync();
            return JsonSerializer.Serialize(basketItems);
        }
        catch (Exception e)
        {
            return Error(e, "无法获取购物车的内容。");
        }
    }

    /// <summary>
    /// 记录错误并返回错误消息
    /// </summary>
    /// <param name="e">异常对象</param>
    /// <param name="message">错误消息</param>
    /// <returns>错误消息字符串</returns>
    private string Error(Exception e, string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError(e, message);
        }

        return message;
    }
}
