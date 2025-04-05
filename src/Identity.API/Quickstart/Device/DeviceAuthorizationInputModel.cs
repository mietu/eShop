// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示设备授权流程的输入模型，继承自用户同意输入模型
/// </summary>
public class DeviceAuthorizationInputModel : ConsentInputModel
{
    /// <summary>
    /// 获取或设置用户输入的设备验证码，用于关联设备授权请求
    /// </summary>
    public string UserCode { get; set; }
}
