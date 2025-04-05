using System.Diagnostics;
using Microsoft.Extensions.AI;
using Pgvector;

namespace eShop.Catalog.API.Services;

/// <summary>
/// 目录AI服务实现类，用于生成和管理商品的语义嵌入向量
/// </summary>
public sealed class CatalogAI : ICatalogAI
{
    /// <summary>嵌入向量的维度大小</summary>
    private const int EmbeddingDimensions = 384;
    /// <summary>嵌入向量生成器</summary>
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    /// <summary>Web主机环境</summary>
    private readonly IWebHostEnvironment _environment;
    /// <summary>用于AI操作的日志记录器</summary>
    private readonly ILogger _logger;

    /// <summary>
    /// 构造函数，初始化目录AI服务
    /// </summary>
    /// <param name="environment">Web主机环境</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="embeddingGenerator">嵌入向量生成器，可为null表示禁用AI功能</param>
    public CatalogAI(IWebHostEnvironment environment, ILogger<CatalogAI> logger, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = null)
    {
        _embeddingGenerator = embeddingGenerator;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 获取AI系统是否已启用
    /// </summary>
    public bool IsEnabled => _embeddingGenerator is not null;

    /// <summary>
    /// 为指定的商品项生成嵌入向量
    /// </summary>
    /// <param name="item">要生成嵌入向量的商品项</param>
    /// <returns>商品的嵌入向量，如果AI未启用则返回null</returns>
    public ValueTask<Vector> GetEmbeddingAsync(CatalogItem item) =>
        IsEnabled ?
            GetEmbeddingAsync(CatalogItemToString(item)) :
            ValueTask.FromResult<Vector>(null);

    /// <summary>
    /// 为多个商品项批量生成嵌入向量
    /// </summary>
    /// <param name="items">要生成嵌入向量的商品项集合</param>
    /// <returns>商品项的嵌入向量列表，如果AI未启用则返回null</returns>
    public async ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(IEnumerable<CatalogItem> items)
    {
        if (IsEnabled)
        {
            // 记录开始时间戳用于性能追踪
            long timestamp = Stopwatch.GetTimestamp();

            // 批量生成嵌入向量
            GeneratedEmbeddings<Embedding<float>> embeddings = await _embeddingGenerator.GenerateAsync(items.Select(CatalogItemToString));
            var results = embeddings.Select(m => new Vector(m.Vector[0..EmbeddingDimensions])).ToList();

            // 记录追踪日志
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("生成了 {EmbeddingsCount} 个嵌入向量，耗时 {ElapsedMilliseconds}秒", results.Count, Stopwatch.GetElapsedTime(timestamp).TotalSeconds);
            }

            return results;
        }

        return null;
    }

    /// <summary>
    /// 为指定的文本生成嵌入向量
    /// </summary>
    /// <param name="text">要生成嵌入向量的文本</param>
    /// <returns>文本的嵌入向量，如果AI未启用则返回null</returns>
    public async ValueTask<Vector> GetEmbeddingAsync(string text)
    {
        if (IsEnabled)
        {
            // 记录开始时间戳用于性能追踪
            long timestamp = Stopwatch.GetTimestamp();

            // 生成嵌入向量
            var embedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync(text);
            embedding = embedding[0..EmbeddingDimensions];

            // 记录追踪日志
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("为文本生成嵌入向量，耗时 {ElapsedMilliseconds}秒: '{Text}'", Stopwatch.GetElapsedTime(timestamp).TotalSeconds, text);
            }

            return new Vector(embedding);
        }

        return null;
    }

    /// <summary>
    /// 将商品项转换为用于生成嵌入向量的文本字符串
    /// </summary>
    /// <param name="item">要转换的商品项</param>
    /// <returns>组合了商品名称和描述的文本字符串</returns>
    private static string CatalogItemToString(CatalogItem item) => $"{item.Name} {item.Description}";
}
