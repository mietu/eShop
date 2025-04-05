namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// OrderItem 实体的 Entity Framework Core 配置类
/// 定义订单项在数据库中的映射规则
/// </summary>
class OrderItemEntityTypeConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    /// <summary>
    /// 配置 OrderItem 实体的数据库映射
    /// </summary>
    /// <param name="orderItemConfiguration">OrderItem 实体类型构建器</param>
    public void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
    {
        // 将实体映射到名为 "orderItems" 的数据库表
        orderItemConfiguration.ToTable("orderItems");

        // 忽略领域事件集合，不将其映射到数据库
        orderItemConfiguration.Ignore(b => b.DomainEvents);

        // 配置 Id 属性使用 HiLo 算法生成值
        // HiLo 是一种高性能的 ID 生成策略，减少了数据库访问
        orderItemConfiguration.Property(o => o.Id)
            .UseHiLo("orderitemseq");

        // 添加名为 "OrderId" 的影子属性（Shadow Property）
        // 这个属性不在实体类中定义，但在数据库中存在，用于表示订单项与订单的关系
        orderItemConfiguration.Property<int>("OrderId");
    }
}
