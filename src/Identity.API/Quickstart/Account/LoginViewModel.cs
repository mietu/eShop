// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 登录视图模型，包含用于显示登录页面的所有必要数据
/// 继承自LoginInputModel以获取基本的登录表单输入（用户名、密码等）
/// </summary>
public class LoginViewModel : LoginInputModel
{
    /// <summary>
    /// 指示是否允许用户选择"记住登录"选项
    /// </summary>
    public bool AllowRememberLogin { get; set; } = true;

    /// <summary>
    /// 指示是否启用本地（用户名/密码）登录方式
    /// </summary>
    public bool EnableLocalLogin { get; set; } = true;

    /// <summary>
    /// 可用的外部身份提供商列表（如Google、Facebook等）
    /// </summary>
    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

    /// <summary>
    /// 可见的外部身份提供商列表，排除了没有显示名称的提供商
    /// </summary>
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    /// <summary>
    /// 指示是否仅提供外部登录方式（禁用本地登录且只有一个外部提供商）
    /// </summary>
    public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;

    /// <summary>
    /// 如果只有外部登录可用，则返回外部登录的认证方案；否则返回null
    /// </summary>
    public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
}
