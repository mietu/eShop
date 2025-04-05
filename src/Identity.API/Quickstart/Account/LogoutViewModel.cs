// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示注销视图的视图模型，继承自<see cref="LogoutInputModel"/>
/// 用于控制注销流程的显示和行为
/// </summary>
public class LogoutViewModel : LogoutInputModel
{
    /// <summary>
    /// 获取或设置一个值，该值指示是否显示注销提示确认界面
    /// 默认值为true，表示在注销时会显示确认提示
    /// </summary>
    public bool ShowLogoutPrompt { get; set; } = true;
}
