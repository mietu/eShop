namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// 支付方式实体的 Entity Framework Core 配置类
/// 用于定义支付方式实体与数据库表之间的映射关系
/// </summary>
class PaymentMethodEntityTypeConfiguration
    : IEntityTypeConfiguration<PaymentMethod>
{
    /// <summary>
    /// 配置支付方式实体的数据库映射
    /// </summary>
    /// <param name="paymentConfiguration">支付方式实体类型构建器</param>
    public void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
    {
        // 设置表名为 "paymentmethods"
        paymentConfiguration.ToTable("paymentmethods");

        // 忽略领域事件集合，不进行持久化
        paymentConfiguration.Ignore(b => b.DomainEvents);

        // 配置 Id 属性使用 HiLo 算法生成值，序列名为 "paymentseq"
        paymentConfiguration.Property(b => b.Id)
            .UseHiLo("paymentseq");

        // 配置买家外键
        paymentConfiguration.Property<int>("BuyerId");

        // 映射私有字段 _cardHolderName 到数据库列 CardHolderName
        paymentConfiguration
            .Property("_cardHolderName")
            .HasColumnName("CardHolderName")
            .HasMaxLength(200);

        // 映射私有字段 _alias 到数据库列 Alias
        paymentConfiguration
            .Property("_alias")
            .HasColumnName("Alias")
            .HasMaxLength(200);

        // 映射私有字段 _cardNumber 到数据库列 CardNumber，设为必填且最大长度25
        paymentConfiguration
            .Property("_cardNumber")
            .HasColumnName("CardNumber")
            .HasMaxLength(25)
            .IsRequired();

        // 映射私有字段 _expiration 到数据库列 Expiration
        paymentConfiguration
            .Property("_expiration")
            .HasColumnName("Expiration")
            .HasMaxLength(25);

        // 映射私有字段 _cardTypeId 到数据库列 CardTypeId
        paymentConfiguration
            .Property("_cardTypeId")
            .HasColumnName("CardTypeId");

        // 配置与 CardType 的一对多关系，使用 _cardTypeId 作为外键
        paymentConfiguration.HasOne(p => p.CardType)
            .WithMany()
            .HasForeignKey("_cardTypeId");
    }
}
