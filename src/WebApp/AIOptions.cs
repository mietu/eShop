namespace eShop.WebApp;

/// <summary>
/// AI选项配置类，用于管理应用中的AI相关设置。
/// </summary>
public class AIOptions
{
    /// <summary>Settings related to the use of OpenAI.</summary>
    /// <summary>与OpenAI使用相关的设置。</summary>
    public OpenAIOptions OpenAI { get; set; } = new();
}

/// <summary>
/// OpenAI选项配置类，包含与OpenAI服务相关的具体设置。
/// </summary>
public class OpenAIOptions
{
    /// <summary>The name of the chat model to use.</summary>
    /// <remarks>When using Azure OpenAI, this should be the "Deployment name" of the chat model.</remarks>
    /// <summary>要使用的聊天模型名称。</summary>
    /// <remarks>当使用Azure OpenAI时，这应该是聊天模型的"部署名称"。</remarks>
    public string ChatModel { get; set; } = "gpt-4o-mini";
}
