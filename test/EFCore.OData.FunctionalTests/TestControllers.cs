// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class TestODataController : ODataController
    {
        [NonAction]
        public new TestNotFoundResult NotFound()
            => new TestNotFoundResult(base.NotFound());

        [NonAction]
        public new TestNotFoundObjectResult NotFound(object value)
            => new TestNotFoundObjectResult(base.NotFound(value));

        [NonAction]
        public new TestBadRequestResult BadRequest()
            => new TestBadRequestResult(base.BadRequest());

        [NonAction]
        public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState)
            => new TestBadRequestObjectResult(base.BadRequest(modelState));

        [NonAction]
        public TestBadRequestObjectResult BadRequest(string message)
            => new TestBadRequestObjectResult(base.BadRequest(message));

        public new TestBadRequestObjectResult BadRequest(object obj)
            => new TestBadRequestObjectResult(base.BadRequest(obj));

        [NonAction]
        public new TestOkResult Ok()
            => new TestOkResult(base.Ok());

        [NonAction]
        public new TestOkObjectResult Ok(object value)
            => new TestOkObjectResult(value);

        [NonAction]
        public TestStatusCodeResult StatusCode(HttpStatusCode statusCode)
            => new TestStatusCodeResult(base.StatusCode((int)statusCode));

        [NonAction]
        public TestStatusCodeObjectResult StatusCode(HttpStatusCode statusCode, object value)
            => new TestStatusCodeObjectResult(base.StatusCode((int)statusCode, value));

        [NonAction]
        public new TestCreatedODataResult<T> Created<T>(T entity)
            => new TestCreatedODataResult<T>(entity);

        [NonAction]
        public new TestCreatedResult Created(string uri, object entity)
            => new TestCreatedResult(base.Created(uri, entity));

        [NonAction]
        public new TestUpdatedODataResult<T> Updated<T>(T entity)
            => new TestUpdatedODataResult<T>(entity);

        protected string GetServiceRootUri()
        {
            var routeName = Request.ODataFeature().RouteName;
            var requestLeftPartBuilder = new StringBuilder(Request.Scheme);
            requestLeftPartBuilder.Append("://");
            requestLeftPartBuilder.Append(Request.Host.HasValue ? Request.Host.Value : Request.Host.ToString());
            if (!string.IsNullOrEmpty(routeName))
            {
                requestLeftPartBuilder.Append("/");
                requestLeftPartBuilder.Append(routeName);
            }

            return requestLeftPartBuilder.ToString();
        }

        protected string GetRoutePrefix()
        {
            var oDataRoute = Request.HttpContext.GetRouteData().Routers
                .Where(r => r.GetType() == typeof(ODataRoute))
                .SingleOrDefault() as ODataRoute;

            Assert.NotNull(oDataRoute);

            return oDataRoute.RoutePrefix;
        }

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
