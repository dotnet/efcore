// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Query;
using Moq;
using Remotion.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryCompilationContextTests
    {
        [Fact]
        public void Can_construct_RedisQueryCompilationContext()
        {
            var model = QueryTestType.Model();
            Assert.DoesNotThrow(() => 
                new RedisQueryCompilationContext(
                    model,
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler()));
        }

        [Fact]
        public void CreateQueryModelVisitor_returns_new_visitor()
        {
            var model = QueryTestType.Model();
            var redisQueryCompilationContext =
                new RedisQueryCompilationContext(
                    model,
                    new LinqOperatorProvider(),
                    new ResultOperatorHandler());
            var parentVisitor = new RedisQueryModelVisitor(redisQueryCompilationContext);
            var visitor = redisQueryCompilationContext.CreateQueryModelVisitor(parentVisitor);
            Assert.IsType<RedisQueryModelVisitor>(visitor);
            Assert.False(ReferenceEquals(visitor, parentVisitor));
        }
    }
}
