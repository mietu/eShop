// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示设备授权流程的视图模型，继承自同意视图模型，增加了设备授权特有的属性
/// </summary>
public class DeviceAuthorizationViewModel : ConsentViewModel
{
    /// <summary>
    /// 获取或设置用户代码，用于在设备授权流程中标识用户的输入代码
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示是否需要用户确认输入的用户代码
    /// </summary>
    public bool ConfirmUserCode { get; set; }
}
