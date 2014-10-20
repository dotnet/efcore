// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class RedisQueryCompilationContextTests
    {
        [Fact]
        public void CreateQueryModelVisitor_returns_new_visitor()
        {
            var model = QueryTestType.Model();
            var loggerFactory = new LoggerFactory();

            var redisQueryCompilationContext =
                new RedisQueryCompilationContext(
                    model,
                    loggerFactory.Create("Fake"),
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler(),
                    new QueryMethodProvider());

            var parentVisitor = new RedisQueryModelVisitor(redisQueryCompilationContext);
            var visitor = redisQueryCompilationContext.CreateQueryModelVisitor(parentVisitor);

            Assert.IsType<RedisQueryModelVisitor>(visitor);
            Assert.False(ReferenceEquals(visitor, parentVisitor));
        }
    }
}
