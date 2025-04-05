// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 授权视图模型，用于展示用户授予的所有客户端权限
/// </summary>
public class GrantsViewModel
{
    /// <summary>
    /// 用户授予的所有客户端权限列表
    /// </summary>
    public IEnumerable<GrantViewModel> Grants { get; set; }
}

/// <summary>
/// 单个客户端授权的视图模型
/// </summary>
public class GrantViewModel
{
    /// <summary>
    /// 客户端标识符
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// 客户端名称
    /// </summary>
    public string ClientName { get; set; }

    /// <summary>
    /// 客户端URL
    /// </summary>
    public string ClientUrl { get; set; }

    /// <summary>
    /// 客户端Logo URL
    /// </summary>
    public string ClientLogoUrl { get; set; }

    /// <summary>
    /// 授权描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 授权创建时间
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// 授权过期时间（可为空）
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// 身份授权名称列表
    /// </summary>
    public IEnumerable<string> IdentityGrantNames { get; set; }

    /// <summary>
    /// API授权名称列表
    /// </summary>
    public IEnumerable<string> ApiGrantNames { get; set; }
}
