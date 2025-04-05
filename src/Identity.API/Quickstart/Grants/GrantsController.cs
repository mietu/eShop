// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 此示例控制器允许用户撤销授予客户端的权限
/// </summary>
[SecurityHeaders] // 安全头部过滤器，用于添加安全相关的HTTP头
[Authorize] // 要求用户认证才能访问此控制器
public class GrantsController : Controller
{
    private readonly IIdentityServerInteractionService _interaction; // 用于IdentityServer交互的服务
    private readonly IClientStore _clients; // 用于获取客户端信息的存储服务
    private readonly IResourceStore _resources; // 用于获取资源信息的存储服务
    private readonly IEventService _events; // 用于记录事件的服务

    /// <summary>
    /// 构造函数，注入必要的服务
    /// </summary>
    public GrantsController(IIdentityServerInteractionService interaction,
        IClientStore clients,
        IResourceStore resources,
        IEventService events)
    {
        _interaction = interaction;
        _clients = clients;
        _resources = resources;
        _events = events;
    }

    /// <summary>
    /// 显示授权列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // 构建视图模型并返回Index视图
        return View("Index", await BuildViewModelAsync());
    }

    /// <summary>
    /// 处理撤销客户端授权的请求
    /// </summary>
    /// <param name="clientId">要撤销授权的客户端ID</param>
    [HttpPost]
    [ValidateAntiForgeryToken] // 防止跨站请求伪造攻击
    public async Task<IActionResult> Revoke(string clientId)
    {
        // 撤销用户对特定客户端的所有授权
        await _interaction.RevokeUserConsentAsync(clientId);
        // 记录撤销授权事件
        await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

        // 重定向回授权列表页面
        return RedirectToAction("Index");
    }

    /// <summary>
    /// 构建授权视图模型
    /// </summary>
    /// <returns>包含用户所有授权信息的视图模型</returns>
    private async Task<GrantsViewModel> BuildViewModelAsync()
    {
        // 获取当前用户的所有授权
        var grants = await _interaction.GetAllUserGrantsAsync();

        var list = new List<GrantViewModel>();
        foreach (var grant in grants)
        {
            // 根据客户端ID查找客户端信息
            var client = await _clients.FindClientByIdAsync(grant.ClientId);
            if (client != null)
            {
                // 查找授权相关的资源信息
                var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                // 创建授权视图模型项
                var item = new GrantViewModel()
                {
                    ClientId = client.ClientId, // 客户端ID
                    ClientName = client.ClientName ?? client.ClientId, // 客户端名称，如果为空则使用客户端ID
                    ClientLogoUrl = client.LogoUri, // 客户端Logo URL
                    ClientUrl = client.ClientUri, // 客户端网站URL
                    Description = grant.Description, // 授权描述
                    Created = grant.CreationTime, // 授权创建时间
                    Expires = grant.Expiration, // 授权过期时间
                    // 身份资源名称列表
                    IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                    // API范围名称列表
                    ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                };

                list.Add(item);
            }
        }

        // 返回包含所有授权的视图模型
        return new GrantsViewModel
        {
            Grants = list
        };
    }
}
