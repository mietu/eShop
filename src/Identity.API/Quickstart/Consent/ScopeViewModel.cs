// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 表示同意页面上的授权范围(scope)的视图模型
/// </summary>
public class ScopeViewModel
{
    /// <summary>
    /// 获取或设置范围的唯一标识符值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 获取或设置范围的显示名称，用于在UI中展示
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 获取或设置范围的详细描述，提供有关此授权范围用途的额外信息
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示此范围是否应在UI中被强调显示
    /// </summary>
    public bool Emphasize { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示此范围是否为必需项，用户无法取消选择必需的范围
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示用户是否已选择授予此范围的权限
    /// </summary>
    public bool Checked { get; set; }
}
