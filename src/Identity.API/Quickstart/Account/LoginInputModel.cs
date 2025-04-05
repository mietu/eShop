// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示登录页面的输入模型，用于接收用户的登录凭据和相关选项
/// </summary>
public class LoginInputModel
{
    /// <summary>
    /// 获取或设置用户名
    /// </summary>
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// 获取或设置密码
    /// </summary>
    [Required]
    public string Password { get; set; }

    /// <summary>
    /// 获取或设置是否记住登录状态
    /// </summary>
    public bool RememberLogin { get; set; }

    /// <summary>
    /// 获取或设置登录成功后的返回URL
    /// </summary>
    public string ReturnUrl { get; set; }
}
