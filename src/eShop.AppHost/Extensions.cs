using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Configuration;

namespace eShop.AppHost;

/// <summary>
/// 提供eShop应用程序的扩展方法
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// 为应用程序中的所有项目添加转发头支持。
    /// 这使得应用程序可以正确处理来自代理或负载均衡器的请求。
    /// </summary>
    /// <param name="builder">分布式应用程序构建器</param>
    /// <returns>配置后的构建器实例以支持方法链</returns>
    public static IDistributedApplicationBuilder AddForwardedHeaders(this IDistributedApplicationBuilder builder)
    {
        // 注册生命周期钩子，在应用启动前设置必要的环境变量
        builder.Services.TryAddLifecycleHook<AddForwardHeadersHook>();
        return builder;
    }

    /// <summary>
    /// 处理转发头的生命周期钩子实现
    /// </summary>
    private class AddForwardHeadersHook : IDistributedApplicationLifecycleHook
    {
        /// <summary>
        /// 在应用启动前执行，为所有项目资源添加转发头支持
        /// </summary>
        /// <param name="appModel">分布式应用程序模型</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务</returns>
        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            // 遍历应用中的所有项目资源
            foreach (var p in appModel.GetProjectResources())
            {
                // 为每个项目添加环境变量注解，启用转发头支持
                p.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
                {
                    context.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = "true";
                }));
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 配置eShop项目使用OpenAI进行文本嵌入和聊天功能。
    /// 支持通过连接字符串使用现有OpenAI资源或通过Azure配置新资源。
    /// </summary>
    /// <param name="builder">分布式应用程序构建器</param>
    /// <param name="catalogApi">目录API项目资源构建器</param>
    /// <param name="webApp">Web应用项目资源构建器</param>
    /// <returns>配置后的构建器实例以支持方法链</returns>
    public static IDistributedApplicationBuilder AddOpenAI(this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> catalogApi,
        IResourceBuilder<ProjectResource> webApp)
    {
        // to use an existing OpenAI resource as a connection string, add the following to the AppHost user secrets:
        // "ConnectionStrings": {
        //   "openai": "Key=<API Key>" (to use https://api.openai.com/)
        //     -or-
        //   "openai": "Endpoint=https://<name>.openai.azure.com/" (to use Azure OpenAI)
        // }

        // 定义资源名称和模型名称常量
        const string openAIName = "openai";
        const string textEmbeddingModelName = "text-embedding-3-small";
        const string chatModelName = "gpt-4o-mini";

        // 声明OpenAI资源构建器变量
        IResourceBuilder<IResourceWithConnectionString> openAI;

        // 检查是否已通过用户机密配置了连接字符串
        if (builder.Configuration.GetConnectionString(openAIName) is not null)
        {
            // 使用现有连接字符串配置OpenAI
            openAI = builder.AddConnectionString(openAIName);
        }
        else
        {
            // to use Azure provisioning, add the following to the AppHost user secrets:
            // "Azure": {
            //   "SubscriptionId": "<your subscription ID>",
            //   "ResourceGroupPrefix": "<prefix>",
            //   "Location": "<location>"
            // }

            // 使用Azure配置OpenAI资源
            var openAITyped = builder.AddAzureOpenAI(openAIName);

            // to use an existing Azure OpenAI resource via provisioning, add the following to the AppHost user secrets:
            // "Parameters": {
            //   "openaiName": "<Azure OpenAI resource name>",
            //   "openaiResourceGroup": "<Azure OpenAI resource group>"
            // }
            // - or -
            // leave the parameters out to create a new Azure OpenAI resource

            // 检查是否指定了现有Azure OpenAI资源的参数
            if (builder.Configuration["Parameters:openaiName"] is not null &&
                builder.Configuration["Parameters:openaiResourceGroup"] is not null)
            {
                // 使用现有Azure OpenAI资源
                openAITyped.AsExisting(
                    builder.AddParameter("openaiName"),
                    builder.AddParameter("openaiResourceGroup"));
            }

            // 配置模型部署
            openAITyped
                // 配置聊天模型部署
                .AddDeployment(new AzureOpenAIDeployment(chatModelName, "gpt-4o-mini", "2024-07-18"))
                // 配置文本嵌入模型部署，设置较高的容量以支持初始嵌入操作
                .AddDeployment(new AzureOpenAIDeployment(textEmbeddingModelName, "text-embedding-3-small", "1", skuCapacity: 20)); // 20k tokens per minute are needed to seed the initial embeddings

            openAI = openAITyped;
        }

        // 配置目录API以使用文本嵌入模型
        catalogApi
            .WithReference(openAI)
            .WithEnvironment("AI__OPENAI__EMBEDDINGMODEL", textEmbeddingModelName);

        // 配置Web应用以使用聊天模型
        webApp
            .WithReference(openAI)
            .WithEnvironment("AI__OPENAI__CHATMODEL", chatModelName);

        return builder;
    }

    /// <summary>
    /// 配置eShop项目使用Ollama进行文本嵌入和聊天功能。
    /// Ollama是一个本地运行的开源大语言模型服务。
    /// </summary>
    /// <param name="builder">分布式应用程序构建器</param>
    /// <param name="catalogApi">目录API项目资源构建器</param>
    /// <param name="webApp">Web应用项目资源构建器</param>
    /// <returns>配置后的构建器实例以支持方法链</returns>
    public static IDistributedApplicationBuilder AddOllama(this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> catalogApi,
        IResourceBuilder<ProjectResource> webApp)
    {
        // 配置Ollama资源，启用数据卷、GPU支持和Web界面
        var ollama = builder.AddOllama("ollama")
            .WithDataVolume()    // 添加持久化数据卷
            .WithGPUSupport()    // 启用GPU加速
            .WithOpenWebUI();    // 启用Web用户界面

        // 添加用于文本嵌入的模型
        var embeddings = ollama.AddModel("embedding", "all-minilm");
        // 添加用于聊天的模型
        var chat = ollama.AddModel("chat", "llama3.1");

        // 配置目录API以使用嵌入模型
        catalogApi.WithReference(embeddings)
            .WithEnvironment("OllamaEnabled", "true")
            .WaitFor(embeddings);  // 确保在API启动前嵌入模型已准备就绪

        // 配置Web应用以使用聊天模型
        webApp.WithReference(chat)
            .WithEnvironment("OllamaEnabled", "true")
            .WaitFor(chat);  // 确保在Web应用启动前聊天模型已准备就绪

        return builder;
    }
}
