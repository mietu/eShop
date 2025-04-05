// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示错误页面的视图模型，用于向用户显示错误信息。
/// 该类封装了 ErrorMessage 对象，使其可以在视图中方便地访问和显示。
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// 初始化 <see cref="ErrorViewModel"/> 类的新实例。
    /// 创建一个空的错误视图模型。
    /// </summary>
    public ErrorViewModel()
    {
    }

    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="ErrorViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="error">错误消息文本。</param>
    public ErrorViewModel(string error)
    {
        Error = new ErrorMessage { Error = error };
    }

    /// <summary>
    /// 获取或设置错误信息对象。
    /// 该属性包含要显示给用户的错误详情。
    /// </summary>
    public ErrorMessage Error { get; set; }
}
