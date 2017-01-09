// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class TestQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        public TestQuerySqlGenerator(
            IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper,
            IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            IRelationalTypeMapper relationalTypeMapper,
            SelectExpression selectExpression)
            : base(relationalCommandBuilderFactory, sqlGenerationHelper, parameterNameGeneratorFactory, relationalTypeMapper, selectExpression)
        {
        }
    }
}
