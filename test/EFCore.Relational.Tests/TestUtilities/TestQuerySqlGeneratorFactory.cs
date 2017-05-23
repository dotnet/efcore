// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        public TestQuerySqlGeneratorFactory(QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new TestQuerySqlGenerator(Dependencies, selectExpression);
    }
}
