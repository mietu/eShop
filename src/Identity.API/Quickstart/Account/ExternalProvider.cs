// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示外部身份验证提供程序的类。
/// 用于在身份认证UI中显示和处理外部登录选项。
/// </summary>
public class ExternalProvider
{
    /// <summary>
    /// 获取或设置提供程序的显示名称。
    /// 这是在用户界面中向用户展示的名称。
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 获取或设置身份验证方案的名称。
    /// 用于识别和调用特定的身份验证处理程序。
    /// </summary>
    public string AuthenticationScheme { get; set; }
}
