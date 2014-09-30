// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Moq;
using Remotion.Linq.Clauses;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class RedisQueryModelVisitorTests : RedisQueryModelVisitor
    {
        public RedisQueryModelVisitorTests()
            : base(new RedisQueryCompilationContext(
                QueryTestType.Model(),
                new LinqOperatorProvider(),
                new ResultOperatorHandler(),
                false))
        {
        }

        [Fact]
        public void Can_construct_RedisQueryModelVisitor()
        {
            // the fact that this class can be instantiated means that the base constructor works
        }

        [Fact]
        public void CreateQueryingExpressionTreeVisitor_returns_new_visitor()
        {
            var querySourceMock = new Mock<IQuerySource>();

            var queryModelVisitor = CreateQueryingExpressionTreeVisitor(querySourceMock.Object);

            Assert.IsType<RedisQueryingExpressionTreeVisitor>(queryModelVisitor);
        }
    }
}
