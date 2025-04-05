// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示登出操作的输入模型，用于接收登出请求的相关参数
/// </summary>
public class LogoutInputModel
{
    /// <summary>
    /// 获取或设置登出操作的唯一标识符
    /// 此标识符用于关联登出请求和相应的登出流程
    /// </summary>
    public string LogoutId { get; set; }
}
