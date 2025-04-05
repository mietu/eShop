namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 买家实体的 Entity Framework Core 配置类
/// 定义了买家实体到数据库表的映射关系和约束
/// </summary>
class BuyerEntityTypeConfiguration
    : IEntityTypeConfiguration<Buyer>
{
    /// <summary>
    /// 配置买家实体的数据库映射
    /// </summary>
    /// <param name="buyerConfiguration">买家实体的类型构建器</param>
    public void Configure(EntityTypeBuilder<Buyer> buyerConfiguration)
    {
        // 将实体映射到名为"buyers"的数据库表
        buyerConfiguration.ToTable("buyers");

        // 忽略领域事件集合，不将其映射到数据库
        buyerConfiguration.Ignore(b => b.DomainEvents);

        // 使用HiLo算法配置Id生成策略，从"buyerseq"序列获取值
        buyerConfiguration.Property(b => b.Id)
            .UseHiLo("buyerseq");

        // 设置IdentityGuid属性的最大长度为200个字符
        buyerConfiguration.Property(b => b.IdentityGuid)
            .HasMaxLength(200);

        // 在IdentityGuid列上创建唯一索引，确保买家身份的唯一性
        buyerConfiguration.HasIndex("IdentityGuid")
            .IsUnique(true);

        // 配置与PaymentMethods的一对多关系
        // 一个买家可以有多个支付方式，每个支付方式属于一个买家
        buyerConfiguration.HasMany(b => b.PaymentMethods)
            .WithOne();
    }
}
