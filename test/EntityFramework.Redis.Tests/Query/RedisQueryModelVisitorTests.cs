// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Query;
using Microsoft.Framework.Logging;
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
                new LoggerFactory().Create("Fake"),
                new LinqOperatorProvider(),
                new ResultOperatorHandler(),
                new EntityMaterializerSource(new MemberMapper(new FieldMatcher())), 
                new QueryMethodProvider()))
        {
        }

        [Fact]
        public void CreateQueryingExpressionTreeVisitor_returns_new_visitor()
        {
            var querySourceMock = new Mock<IQuerySource>();

            var queryModelVisitor = CreateQueryingExpressionTreeVisitor(querySourceMock.Object);

            Assert.IsType<RedisEntityQueryableExpressionTreeVisitor>(queryModelVisitor);
        }
    }
}
