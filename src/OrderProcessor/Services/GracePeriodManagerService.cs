using eShop.EventBus.Abstractions;
using eShop.OrderProcessor.Events;
using Microsoft.Extensions.Options;
using Npgsql;

namespace eShop.OrderProcessor.Services
{
    /// <summary>
    /// 宽限期管理服务 - 负责检查订单宽限期并发布相关事件
    /// 这是一个后台服务，定期检查已过宽限期的订单并发布确认事件
    /// </summary>
    public class GracePeriodManagerService(
        IOptions<BackgroundTaskOptions> options,
        IEventBus eventBus,
        ILogger<GracePeriodManagerService> logger,
        NpgsqlDataSource dataSource) : BackgroundService
    {
        private readonly BackgroundTaskOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// 执行后台任务的主要方法
        /// </summary>
        /// <param name="stoppingToken">用于通知任务停止的取消标记</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 从配置中获取检查间隔时间
            var delayTime = TimeSpan.FromSeconds(_options.CheckUpdateTime);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GracePeriodManagerService正在启动。");
                stoppingToken.Register(() => logger.LogDebug("GracePeriodManagerService后台任务正在停止。"));
            }

            // 持续运行直到收到停止信号
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("GracePeriodManagerService后台任务正在执行后台工作。");
                }

                // 检查已经过了宽限期的订单
                await CheckConfirmedGracePeriodOrders();

                // 等待指定时间后再次检查
                await Task.Delay(delayTime, stoppingToken);
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GracePeriodManagerService后台任务正在停止.");
            }
        }

        /// <summary>
        /// 检查已确认的处于宽限期的订单，并为其发布集成事件
        /// </summary>
        private async Task CheckConfirmedGracePeriodOrders()
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("检查已确认的宽限期订单");
            }

            // 获取所有已过宽限期的订单ID
            var orderIds = await GetConfirmedGracePeriodOrders();

            // 为每个订单发布宽限期确认事件
            foreach (var orderId in orderIds)
            {
                var confirmGracePeriodEvent = new GracePeriodConfirmedIntegrationEvent(orderId);

                logger.LogInformation("发布集成事件: {IntegrationEventId} - ({@IntegrationEvent})", confirmGracePeriodEvent.Id, confirmGracePeriodEvent);

                // 通过事件总线发布事件
                await eventBus.PublishAsync(confirmGracePeriodEvent);
            }
        }

        /// <summary>
        /// 从数据库中获取所有已过宽限期且状态为"已提交"的订单ID
        /// </summary>
        /// <returns>符合条件的订单ID列表</returns>
        private async ValueTask<List<int>> GetConfirmedGracePeriodOrders()
        {
            try
            {
                using var conn = dataSource.CreateConnection();
                using var command = conn.CreateCommand();
                // 查询已过宽限期的订单，条件为：
                // 1. 当前时间减去订单日期 >= 宽限期时间
                // 2. 订单状态为"已提交"
                command.CommandText = """
                        SELECT "Id"
                        FROM ordering.orders
                        WHERE CURRENT_TIMESTAMP - "OrderDate" >= @GracePeriodTime AND "OrderStatus" = 'Submitted'
                        """;
                command.Parameters.AddWithValue("GracePeriodTime", TimeSpan.FromMinutes(_options.GracePeriodTime));

                List<int> ids = [];

                await conn.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetInt32(0));
                }

                return ids;
            }
            catch (NpgsqlException exception)
            {
                // 记录数据库连接错误
                logger.LogError(exception, "建立数据库连接时出现致命错误");
            }

            // 发生异常时返回空列表
            return [];
        }
    }
}
