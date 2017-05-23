// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        public TestQuerySqlGenerator(
            QuerySqlGeneratorDependencies dependencies,
            SelectExpression selectExpression)
            : base(dependencies, selectExpression)
        {
        }
    }
}
