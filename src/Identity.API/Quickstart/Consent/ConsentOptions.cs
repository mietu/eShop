// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 用户同意界面的配置选项类
/// </summary>
public class ConsentOptions
{
    /// <summary>
    /// 是否启用离线访问权限
    /// </summary>
    public static bool EnableOfflineAccess = true;

    /// <summary>
    /// 离线访问权限的显示名称
    /// </summary>
    public static string OfflineAccessDisplayName = "Offline Access";

    /// <summary>
    /// 离线访问权限的描述文本
    /// </summary>
    public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

    /// <summary>
    /// 当用户未选择任何权限时显示的错误消息
    /// </summary>
    public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";

    /// <summary>
    /// 当用户选择无效时显示的错误消息
    /// </summary>
    public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
}
