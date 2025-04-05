// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Text;
using System.Text.Json;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// 诊断视图模型类，用于展示身份验证的诊断信息
/// 包括身份验证结果和关联的客户端列表
/// </summary>
public class DiagnosticsViewModel
{
    /// <summary>
    /// 构造函数，初始化诊断视图模型
    /// </summary>
    /// <param name="result">身份验证结果，包含验证状态和属性信息</param>
    public DiagnosticsViewModel(AuthenticateResult result)
    {
        AuthenticateResult = result;

        // 检查属性中是否包含客户端列表
        if (result.Properties.Items.ContainsKey("client_list"))
        {
            // 获取Base64Url编码的客户端列表
            var encoded = result.Properties.Items["client_list"];
            // 解码Base64Url编码的字节
            var bytes = Base64Url.Decode(encoded);
            // 将字节转换为UTF8字符串
            var value = Encoding.UTF8.GetString(bytes);

            // 反序列化JSON字符串为字符串数组，得到客户端列表
            Clients = JsonSerializer.Deserialize<string[]>(value);
        }
    }

    /// <summary>
    /// 获取身份验证结果，包含认证状态和详细信息
    /// </summary>
    public AuthenticateResult AuthenticateResult { get; }

    /// <summary>
    /// 获取与当前认证相关联的客户端列表
    /// 默认为空列表
    /// </summary>
    public IEnumerable<string> Clients { get; } = new List<string>();
}
