// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Specification.Tests.TestUtilities
{
    public class TestQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        public TestQuerySqlGeneratorFactory(
            IRelationalCommandBuilderFactory commandBuilderFactory,
            ISqlGenerationHelper sqlGenerationHelper,
            IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            IRelationalTypeMapper relationalTypeMapper)
            : base(commandBuilderFactory, sqlGenerationHelper, parameterNameGeneratorFactory, relationalTypeMapper)
        {
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
        => new TestQuerySqlGenerator(
                CommandBuilderFactory,
                SqlGenerationHelper,
                ParameterNameGeneratorFactory,
                RelationalTypeMapper,
                selectExpression);
    }
}
