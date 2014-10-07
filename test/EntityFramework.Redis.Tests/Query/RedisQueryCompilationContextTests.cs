// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class RedisQueryCompilationContextTests
    {
        [Fact]
        public void Can_construct_sync_RedisQueryCompilationContext()
        {
            var model = QueryTestType.Model();

            var redisQueryCompilationContext =
                new RedisQueryCompilationContext(
                    model,
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler(),
                    new QueryMethodProvider());
        }

        [Fact]
        public void Can_construct_async_RedisQueryCompilationContext()
        {
            var model = QueryTestType.Model();

            var redisQueryCompilationContext =
                new RedisQueryCompilationContext(
                    model,
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler(),
                    new AsyncQueryMethodProvider());
        }

        [Fact]
        public void CreateQueryModelVisitor_returns_new_visitor()
        {
            var model = QueryTestType.Model();
            var redisQueryCompilationContext =
                new RedisQueryCompilationContext(
                    model,
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
