// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.EntityFrameworkCore;

public class TestODataController : ODataController
{
    [NonAction]
    public new TestNotFoundResult NotFound()
        => new(base.NotFound());

    [NonAction]
    public new TestNotFoundObjectResult NotFound(object value)
        => new(base.NotFound(value));

    [NonAction]
    public new TestBadRequestResult BadRequest()
        => new(base.BadRequest());

    [NonAction]
    public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        => new(base.BadRequest(modelState));

    public new TestBadRequestObjectResult BadRequest(object obj)
        => new(base.BadRequest(obj));

    [NonAction]
    public new TestOkResult Ok()
        => new(base.Ok());

    [NonAction]
    public new TestOkObjectResult Ok(object value)
        => new(value);

    [NonAction]
    public TestStatusCodeResult StatusCode(HttpStatusCode statusCode)
        => new(base.StatusCode((int)statusCode));

    [NonAction]
    public TestStatusCodeObjectResult StatusCode(HttpStatusCode statusCode, object value)
        => new(base.StatusCode((int)statusCode, value));

    [NonAction]
    public new TestCreatedODataResult<T> Created<T>(T entity)
        => new(entity);

    [NonAction]
    public new TestCreatedResult Created(string uri, object entity)
        => new(base.Created(uri, entity));

    [NonAction]
    public new TestUpdatedODataResult<T> Updated<T>(T entity)
        => new(entity);

    protected bool Validate(object model)
        => TryValidateModel(model);
}

public interface ITestActionResult : IActionResult;

public class TestActionResult(IActionResult innerResult) : ITestActionResult
{
    private readonly IActionResult _innerResult = innerResult;

    public Task ExecuteResultAsync(ActionContext context)
        => _innerResult.ExecuteResultAsync(context);
}

public class TestObjectResult(object innerResult) : ObjectResult(innerResult), ITestActionResult;

public class TestStatusCodeResult(StatusCodeResult innerResult) : StatusCodeResult(innerResult.StatusCode), ITestActionResult
{
    private readonly StatusCodeResult _innerResult = innerResult;
}

public class TestNotFoundResult(NotFoundResult innerResult) : TestStatusCodeResult(innerResult);

public class TestNotFoundObjectResult(NotFoundObjectResult innerResult) : TestObjectResult(innerResult);

public class TestBadRequestResult(BadRequestResult innerResult) : TestStatusCodeResult(innerResult);

public class TestBadRequestObjectResult(BadRequestObjectResult innerResult) : TestActionResult(innerResult);

public class TestOkResult(OkResult innerResult) : TestStatusCodeResult(innerResult);

public class TestOkObjectResult : TestObjectResult
{
    public TestOkObjectResult(object innerResult)
        : base(innerResult)
    {
        StatusCode = 200;
    }
}

public class TestOkObjectResult<T> : TestObjectResult
{
    public TestOkObjectResult(object innerResult)
        : base(innerResult)
    {
        StatusCode = 200;
    }

    public TestOkObjectResult(T content, TestODataController controller)
        : base(content)
    {
        // Controller is unused.
        StatusCode = 200;
    }
}

public class TestStatusCodeObjectResult(ObjectResult innerResult) : TestObjectResult(innerResult);

public class TestCreatedResult(CreatedResult innerResult) : TestActionResult(innerResult);

public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, ITestActionResult
{
    public TestUpdatedODataResult(T entity)
        : base(entity)
    {
    }

    public TestUpdatedODataResult(string uri, T entity)
        : base(entity)
    {
    }
}

public class TestCreatedODataResult<T> : CreatedODataResult<T>, ITestActionResult
{
    public TestCreatedODataResult(T entity)
        : base(entity)
    {
    }

    public TestCreatedODataResult(string uri, T entity)
        : base(entity)
    {
    }
}
