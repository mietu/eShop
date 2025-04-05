using Microsoft.AspNetCore.Http.HttpResults;
using CardType = eShop.Ordering.API.Application.Queries.CardType;
using Order = eShop.Ordering.API.Application.Queries.Order;

/// <summary>
/// 订单API静态类，提供订单相关的HTTP端点映射和处理方法
/// </summary>
public static class OrdersApi
{
    /// <summary>
    /// 映射订单API的V1版本端点
    /// </summary>
    /// <param name="app">终结点路由构建器</param>
    /// <returns>配置后的路由组构建器</returns>
    public static RouteGroupBuilder MapOrdersApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/orders").HasApiVersion(1.0);

        api.MapPut("/cancel", CancelOrderAsync);       // 取消订单
        api.MapPut("/ship", ShipOrderAsync);           // 发货订单
        api.MapGet("{orderId:int}", GetOrderAsync);    // 获取指定订单
        api.MapGet("/", GetOrdersByUserAsync);         // 获取用户所有订单
        api.MapGet("/cardtypes", GetCardTypesAsync);   // 获取支付卡类型
        api.MapPost("/draft", CreateOrderDraftAsync);  // 创建订单草稿
        api.MapPost("/", CreateOrderAsync);            // 创建正式订单

        return api;
    }

    /// <summary>
    /// 取消订单的异步处理方法
    /// </summary>
    /// <param name="requestId">请求ID，从请求头x-requestid获取</param>
    /// <param name="command">取消订单命令</param>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>操作结果，包含成功、错误请求或处理问题的可能结果</returns>
    public static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CancelOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestId,
        CancelOrderCommand command,
        [AsParameters] OrderServices services)
    {
        if (requestId == Guid.Empty)
        {
            return TypedResults.BadRequest("空 GUID 对请求 ID 无效");
        }

        // 创建标识化命令，包装原始命令和请求ID
        var requestCancelOrder = new IdentifiedCommand<CancelOrderCommand, bool>(command, requestId);

        services.Logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            requestCancelOrder.GetGenericTypeName(),
            nameof(requestCancelOrder.Command.OrderNumber),
            requestCancelOrder.Command.OrderNumber,
            requestCancelOrder);

        // 发送命令到中介者处理
        var commandResult = await services.Mediator.Send(requestCancelOrder);

        if (!commandResult)
        {
            return TypedResults.Problem(detail: "取消订单处理失败。", statusCode: 500);
        }

        return TypedResults.Ok();
    }

    /// <summary>
    /// 发货订单的异步处理方法
    /// </summary>
    /// <param name="requestId">请求ID，从请求头x-requestid获取</param>
    /// <param name="command">发货订单命令</param>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>操作结果，包含成功、错误请求或处理问题的可能结果</returns>
    public static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> ShipOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestId,
        ShipOrderCommand command,
        [AsParameters] OrderServices services)
    {
        if (requestId == Guid.Empty)
        {
            return TypedResults.BadRequest("空 GUID 对请求 ID 无效");
        }

        // 创建标识化命令，包装原始命令和请求ID
        var requestShipOrder = new IdentifiedCommand<ShipOrderCommand, bool>(command, requestId);

        services.Logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            requestShipOrder.GetGenericTypeName(),
            nameof(requestShipOrder.Command.OrderNumber),
            requestShipOrder.Command.OrderNumber,
            requestShipOrder);

        // 发送命令到中介者处理
        var commandResult = await services.Mediator.Send(requestShipOrder);

        if (!commandResult)
        {
            return TypedResults.Problem(detail: "发货订单处理失败。", statusCode: 500);
        }

        return TypedResults.Ok();
    }

    /// <summary>
    /// 获取指定订单的异步处理方法
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>订单信息或未找到结果</returns>
    public static async Task<Results<Ok<Order>, NotFound>> GetOrderAsync(int orderId, [AsParameters] OrderServices services)
    {
        try
        {
            // 尝试获取订单信息
            var order = await services.Queries.GetOrderAsync(orderId);
            return TypedResults.Ok(order);
        }
        catch
        {
            // 如果发生异常，返回未找到结果
            return TypedResults.NotFound();
        }
    }

    /// <summary>
    /// 获取当前用户所有订单的异步处理方法
    /// </summary>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>用户订单摘要列表</returns>
    public static async Task<Ok<IEnumerable<OrderSummary>>> GetOrdersByUserAsync([AsParameters] OrderServices services)
    {
        // 获取当前用户ID
        var userId = services.IdentityService.GetUserIdentity();
        // 查询用户的所有订单
        var orders = await services.Queries.GetOrdersFromUserAsync(userId);
        return TypedResults.Ok(orders);
    }

    /// <summary>
    /// 获取所有支付卡类型的异步处理方法
    /// </summary>
    /// <param name="orderQueries">订单查询服务</param>
    /// <returns>支付卡类型列表</returns>
    public static async Task<Ok<IEnumerable<CardType>>> GetCardTypesAsync(IOrderQueries orderQueries)
    {
        var cardTypes = await orderQueries.GetCardTypesAsync();
        return TypedResults.Ok(cardTypes);
    }

    /// <summary>
    /// 创建订单草稿的异步处理方法
    /// </summary>
    /// <param name="command">创建订单草稿命令</param>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>订单草稿数据传输对象</returns>
    public static async Task<OrderDraftDTO> CreateOrderDraftAsync(CreateOrderDraftCommand command, [AsParameters] OrderServices services)
    {
        services.Logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
            command.GetGenericTypeName(),
            nameof(command.BuyerId),
            command.BuyerId,
            command);

        // 发送命令到中介者处理并返回草稿结果
        return await services.Mediator.Send(command);
    }

    /// <summary>
    /// 创建正式订单的异步处理方法
    /// </summary>
    /// <param name="requestId">请求ID，从请求头x-requestid获取</param>
    /// <param name="request">创建订单请求</param>
    /// <param name="services">订单相关服务集合</param>
    /// <returns>成功结果或错误请求结果</returns>
    public static async Task<Results<Ok, BadRequest<string>>> CreateOrderAsync(
        [FromHeader(Name = "x-requestid")] Guid requestId,
        CreateOrderRequest request,
        [AsParameters] OrderServices services)
    {
        // 注意：信用卡号码将被掩码处理

        services.Logger.LogInformation(
            "发送命令: {CommandName} - {IdProperty}: {CommandId}",
            request.GetGenericTypeName(),
            nameof(request.UserId),
            request.UserId); // 不记录完整请求内容，因为包含信用卡号

        if (requestId == Guid.Empty)
        {
            services.Logger.LogWarning("无效的集成事件 - 缺少请求 ID - {@IntegrationEvent}", request);
            return TypedResults.BadRequest("RequestId is missing.");
        }

        // 创建日志作用域，包含请求ID
        using (services.Logger.BeginScope(new List<KeyValuePair<string, object>> { new("IdentifiedCommandId", requestId) }))
        {
            // 掩码处理信用卡号，只保留后4位，其余用X替代
            var maskedCCNumber = request.CardNumber.Substring(request.CardNumber.Length - 4).PadLeft(request.CardNumber.Length, 'X');
            // 创建订单命令，使用掩码处理后的信用卡号
            var createOrderCommand = new CreateOrderCommand(request.Items, request.UserId, request.UserName, request.City, request.Street,
                request.State, request.Country, request.ZipCode,
                maskedCCNumber, request.CardHolderName, request.CardExpiration,
                request.CardSecurityNumber, request.CardTypeId);

            // 创建标识化命令，包装原始命令和请求ID
            var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, requestId);

            services.Logger.LogInformation(
                "发送命令: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                requestCreateOrder.GetGenericTypeName(),
                nameof(requestCreateOrder.Id),
                requestCreateOrder.Id,
                requestCreateOrder);

            // 发送命令到中介者处理
            var result = await services.Mediator.Send(requestCreateOrder);

            // 根据处理结果记录日志
            if (result)
            {
                services.Logger.LogInformation("CreateOrderCommand 成功 - RequestId: {RequestId}", requestId);
            }
            else
            {
                services.Logger.LogWarning("CreateOrderCommand 失败 - RequestId: {RequestId}", requestId);
            }

            return TypedResults.Ok();
        }
    }
}

/// <summary>
/// 创建订单请求记录类，包含创建订单所需的所有数据
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="UserName">用户名</param>
/// <param name="City">城市</param>
/// <param name="Street">街道</param>
/// <param name="State">州/省</param>
/// <param name="Country">国家</param>
/// <param name="ZipCode">邮政编码</param>
/// <param name="CardNumber">信用卡号</param>
/// <param name="CardHolderName">持卡人姓名</param>
/// <param name="CardExpiration">卡有效期</param>
/// <param name="CardSecurityNumber">卡安全码</param>
/// <param name="CardTypeId">卡类型ID</param>
/// <param name="Buyer">买家</param>
/// <param name="Items">订单项目列表</param>
public record CreateOrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);
