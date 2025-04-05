// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示用户同意授权的视图模型
/// </summary>
public class ConsentViewModel : ConsentInputModel
{
    /// <summary>
    /// 获取或设置客户端应用的名称
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    /// 获取或设置客户端应用的URL
    /// </summary>
    public string ClientUrl { get; set; }

    /// <summary>
    /// 获取或设置客户端应用的Logo URL
    /// </summary>
    public string ClientLogoUrl { get; set; }

    /// <summary>
    /// 获取或设置一个值，该值指示是否允许记住用户的同意选择
    /// </summary>
    public bool AllowRememberConsent { get; set; }

    /// <summary>
    /// 获取或设置身份相关的权限范围集合
    /// </summary>
    public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }

    /// <summary>
    /// 获取或设置API相关的权限范围集合
    /// </summary>
    public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
}
