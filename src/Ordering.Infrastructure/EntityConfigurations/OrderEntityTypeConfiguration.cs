namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 订单实体类型配置类
/// 用于配置 Order 聚合根的 Entity Framework Core 映射规则
/// 负责定义表名、主键策略、值对象映射、导航属性及关系配置等
/// </summary>
class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// 配置 Order 实体的 EF Core 映射规则
    /// </summary>
    /// <param name="orderConfiguration">Order 实体的类型构建器</param>
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        // 设置数据库表名为 "orders"
        orderConfiguration.ToTable("orders");

        // 忽略领域事件集合，不映射到数据库
        orderConfiguration.Ignore(b => b.DomainEvents);

        // 使用 HiLo 算法生成 ID 值，指定序列名称为 "orderseq"
        orderConfiguration.Property(o => o.Id)
            .UseHiLo("orderseq");

        // 配置 Address 值对象作为拥有实体类型(owned entity)
        // EF Core 2.0+ 支持值对象持久化为拥有实体
        orderConfiguration
            .OwnsOne(o => o.Address);

        // 将 OrderStatus 枚举映射为字符串类型，并设置最大长度
        orderConfiguration
            .Property(o => o.OrderStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        // 将 PaymentId 属性映射到数据库中的 PaymentMethodId 列
        orderConfiguration
            .Property(o => o.PaymentId)
            .HasColumnName("PaymentMethodId");

        // 配置 Order 与 PaymentMethod 的一对多关系
        // 设置外键为 PaymentId，删除行为为限制(Restrict)
        orderConfiguration.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(o => o.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // 配置 Order 与 Buyer 的一对多关系
        // 设置外键为 BuyerId
        orderConfiguration.HasOne(o => o.Buyer)
            .WithMany()
            .HasForeignKey(o => o.BuyerId);
    }
}
