// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示用户授权同意的输入模型
/// </summary>
public class ConsentInputModel
{
    /// <summary>
    /// 获取或设置用户点击的按钮值，通常用于确定用户是同意还是拒绝授权
    /// </summary>
    public string Button { get; set; }

    /// <summary>
    /// 获取或设置用户同意授予的作用域集合
    /// </summary>
    public IEnumerable<string> ScopesConsented { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示是否记住用户的同意决定
    /// </summary>
    public bool RememberConsent { get; set; }

    /// <summary>
    /// 获取或设置授权完成后的返回URL
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    /// 获取或设置用户提供的关于此次授权的描述
    /// </summary>
    public string Description { get; set; }
}
