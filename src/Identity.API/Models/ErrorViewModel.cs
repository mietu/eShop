// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace eShop.Identity.API.Models
{
    /// <summary>
    /// 表示用于显示错误信息的视图模型。
    /// 此视图模型用于在用户界面中呈现错误信息。
    /// </summary>
    public record ErrorViewModel
    {
        /// <summary>
        /// 获取或设置错误消息详情。
        /// 包含要显示给用户的实际错误信息。
        /// </summary>
        public ErrorMessage Error { get; set; }
    }
}
