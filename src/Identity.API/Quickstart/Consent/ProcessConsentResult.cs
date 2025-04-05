// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示处理用户授权同意的结果
/// 此类用于封装授权同意处理后的各种可能结果，包括重定向、显示视图或验证错误
/// </summary>
public class ProcessConsentResult
{
    /// <summary>
    /// 获取一个值，该值指示是否应重定向用户
    /// </summary>
    public bool IsRedirect => RedirectUri != null;

    /// <summary>
    /// 获取或设置用户应被重定向到的URI
    /// </summary>
    public string RedirectUri { get; set; }

    /// <summary>
    /// 获取或设置请求授权的客户端应用
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// 获取一个值，该值指示是否应向用户显示视图
    /// </summary>
    public bool ShowView => ViewModel != null;

    /// <summary>
    /// 获取或设置要显示给用户的同意视图模型
    /// </summary>
    public ConsentViewModel ViewModel { get; set; }

    /// <summary>
    /// 获取一个值，该值指示处理过程中是否存在验证错误
    /// </summary>
    public bool HasValidationError => ValidationError != null;

    /// <summary>
    /// 获取或设置验证过程中发生的错误信息
    /// </summary>
    public string ValidationError { get; set; }
}
