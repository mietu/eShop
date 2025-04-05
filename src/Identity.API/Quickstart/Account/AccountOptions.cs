// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 账户相关的配置选项
/// </summary>
public class AccountOptions
{
    /// <summary>
    /// 是否允许用户使用本地账户登录
    /// 默认为 true，表示允许本地登录
    /// </summary>
    public static bool AllowLocalLogin = true;

    /// <summary>
    /// 是否允许"记住我"功能
    /// 默认为 true，表示允许用户选择记住登录状态
    /// </summary>
    public static bool AllowRememberLogin = true;

    /// <summary>
    /// "记住我"登录状态的持续时间
    /// 默认为 30 天
    /// </summary>
    public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

    /// <summary>
    /// 登出时是否显示确认提示
    /// 默认为 false，表示不显示确认提示，直接登出
    /// </summary>
    public static bool ShowLogoutPrompt = false;

    /// <summary>
    /// 登出后是否自动重定向
    /// 默认为 true，表示登出后自动重定向到指定页面
    /// </summary>
    public static bool AutomaticRedirectAfterSignOut = true;

    /// <summary>
    /// 凭据无效时显示的错误消息
    /// </summary>
    public static string InvalidCredentialsErrorMessage = "Invalid username or password";
}
