namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// CardType实体的实体类型配置
/// 用于配置支付卡类型与数据库表的映射关系
/// </summary>
class CardTypeEntityTypeConfiguration
    : IEntityTypeConfiguration<CardType>
{
    /// <summary>
    /// 配置CardType实体的数据库映射
    /// </summary>
    /// <param name="cardTypesConfiguration">CardType实体类型构建器</param>
    public void Configure(EntityTypeBuilder<CardType> cardTypesConfiguration)
    {
        // 设置表名为"cardtypes"
        cardTypesConfiguration.ToTable("cardtypes");

        // 配置Id属性不自动生成值
        cardTypesConfiguration.Property(ct => ct.Id)
            .ValueGeneratedNever();

        // 配置Name属性，最大长度为200，且为必填项
        cardTypesConfiguration.Property(ct => ct.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}
