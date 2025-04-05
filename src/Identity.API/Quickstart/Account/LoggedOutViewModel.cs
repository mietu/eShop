// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示用户登出后视图所需的数据模型
/// </summary>
public class LoggedOutViewModel
{
    /// <summary>
    /// 登出后重定向的URI
    /// </summary>
    public string PostLogoutRedirectUri { get; set; }

    /// <summary>
    /// 客户端名称
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    /// 登出iframe的URL，用于单点登出功能
    /// </summary>
    public string SignOutIframeUrl { get; set; }

    /// <summary>
    /// 指示是否在登出后自动重定向
    /// </summary>
    public bool AutomaticRedirectAfterSignOut { get; set; }

    /// <summary>
    /// 登出操作的唯一标识符
    /// </summary>
    public string LogoutId { get; set; }

    /// <summary>
    /// 指示是否触发外部身份验证提供程序的登出
    /// </summary>
    public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

    /// <summary>
    /// 外部身份验证方案名称
    /// </summary>
    public string ExternalAuthenticationScheme { get; set; }
}
