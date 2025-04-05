// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServerHost.Quickstart.UI
{
    /// <summary>
    /// 首页控制器，处理主页和错误页的显示逻辑
    /// 在生产环境中主页被禁用，仅在开发环境中可见
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        /// <summary>
        /// Identity Server交互服务，用于获取错误上下文等信息
        /// </summary>
        private readonly IIdentityServerInteractionService _interaction;

        /// <summary>
        /// Web主机环境信息，用于判断当前是开发环境还是生产环境
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// 日志记录器，用于记录控制器操作
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化首页控制器的新实例
        /// </summary>
        /// <param name="interaction">Identity Server交互服务</param>
        /// <param name="environment">Web主机环境</param>
        /// <param name="logger">日志记录器</param>
        public HomeController(
            IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment,
            ILogger<HomeController> logger)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// 显示首页
        /// 注意：首页仅在开发环境中可见，在生产环境中返回404错误
        /// </summary>
        /// <returns>开发环境中返回首页视图，生产环境中返回404</returns>
        public IActionResult Index()
        {
            if (_environment.IsDevelopment())
            {
                // 仅在开发环境中显示首页
                return View();
            }

            _logger.LogInformation("Homepage is disabled in production. Returning 404.");
            return NotFound();
        }

        /// <summary>
        /// 显示错误页面
        /// 从Identity Server获取错误上下文信息并显示
        /// 在生产环境中隐藏错误详情以提高安全性
        /// </summary>
        /// <param name="errorId">要显示的错误ID</param>
        /// <returns>包含错误信息的错误页视图</returns>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // 从Identity Server获取错误上下文信息
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    // 在生产环境中隐藏错误详情，提高安全性
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}
