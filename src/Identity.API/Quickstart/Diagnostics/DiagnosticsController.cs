// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 诊断控制器，用于展示身份验证相关信息
/// 此控制器仅允许从本地地址访问，提供系统诊断功能
/// </summary>
[SecurityHeaders]
[Authorize]
public class DiagnosticsController : Controller
{
    /// <summary>
    /// 显示当前用户身份验证信息的诊断页面
    /// </summary>
    /// <returns>包含身份验证信息的诊断视图</returns>
    public async Task<IActionResult> Index()
    {
        // 定义本地地址列表，包括环回地址和当前服务器的本地IP
        var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };

        // 安全检查：仅允许从本地地址访问此页面
        if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
        {
            return NotFound();
        }

        // 创建诊断视图模型，包含当前HTTP上下文的身份验证信息
        var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());

        // 返回包含身份验证诊断信息的视图
        return View(model);
    }
}
