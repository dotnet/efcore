// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.EntityFrameworkCore
{
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
        {
            return TryValidateModel(model);
        }
    }

    public interface ITestActionResult : IActionResult { }

    public class TestActionResult : ITestActionResult
    {
        private readonly IActionResult _innerResult;

        public TestActionResult(IActionResult innerResult)
        {
            _innerResult = innerResult;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            return _innerResult.ExecuteResultAsync(context);
        }
    }

    public class TestObjectResult : ObjectResult, ITestActionResult
    {
        public TestObjectResult(object innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestStatusCodeResult : StatusCodeResult, ITestActionResult
    {
        private readonly StatusCodeResult _innerResult;

        public TestStatusCodeResult(StatusCodeResult innerResult)
            : base(innerResult.StatusCode)
        {
            _innerResult = innerResult;
        }
    }

    public class TestNotFoundResult : TestStatusCodeResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestNotFoundObjectResult : TestObjectResult
    {
        public TestNotFoundObjectResult(NotFoundObjectResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestBadRequestResult : TestStatusCodeResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestBadRequestObjectResult : TestActionResult
    {
        public TestBadRequestObjectResult(BadRequestObjectResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestOkResult : TestStatusCodeResult
    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }

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

    public class TestStatusCodeObjectResult : TestObjectResult
    {
        public TestStatusCodeObjectResult(ObjectResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestCreatedResult : TestActionResult
    {
        public TestCreatedResult(CreatedResult innerResult)
            : base(innerResult)
        {
        }
    }

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
}
